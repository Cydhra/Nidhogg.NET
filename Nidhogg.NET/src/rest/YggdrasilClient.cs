using System;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using Nidhogg.data;
using Nidhogg.rest.exceptions;
using Nidhogg.rest.responses;
using RestSharp;
using RestSharp.Deserializers;

namespace Nidhogg.rest
{
    /// <summary>
    /// The Yggdrasil client is the interface using the rest client to connect to Yggdrasil and performing the supported
    /// requests. It encapsulates all JSON and REST functionality and just accepts and returns internal structures
    /// containing the required information
    /// </summary>
    public class YggdrasilClient
    {
        public const string NidhoggClientToken = "nidhogg";

        private const string YggdrasilHostServer = "https://authserver.mojang.com";
        private const string EndpointAuthenticate = "/authenticate";
        private const string EndpointRefresh = "/refresh";
        private const string EndpointValidate = "/validate";
        private const string EndpointSignout = "/signout";
        private const string EndpointInvalidate = "/invalidate";

        private static readonly Regex ErrorMessageRegex = new Regex("\\{\"error\":\\s*\"[a-zA-Z0-9\\s\\.\\,]*\",\\s*" +
                                                             "\"errorMessage\":\\s*\"[a-zA-Z0-9\\s\\.\\,]*\"(," +
                                                             "\\s*\"cause\":\\s*\"[a-zA-Z0-9\\s]*\")*\\}");

        private readonly JsonDeserializer _deserializer = new JsonDeserializer();

        /// <summary>
        /// POST a login request to Yggdrasil with given credentials.
        /// </summary>
        ///
        /// <param name="data">Account Credentials structure containing either username or email and a password</param>
        /// <param name="sessionClientToken">the session client token (for Yggdrasil to recognize different sessions
        /// of different launchers). Defaulted to Nidhogg and version identifier</param>
        /// <param name="agentName">Name of the agent. Determines the Mojang product to authenticate to. Defaulted to
        /// "Minecraft"</param>
        ///
        /// <returns>A Session structure containing a valid access token, the used client token and the account's
        /// username</returns>
        ///
        /// <exception cref="UserMigratedException">If the account credentials contained a username, but account
        /// needs an E-Mail for login.</exception>
        /// <exception cref="InvalidCredentialException">If the given credentials where invalid</exception>
        /// <exception cref="YggdrasilBanException">If Yggdrasil server banned the client application due to
        /// too many failed login attempts.</exception>
        /// <exception cref="ArgumentException">If user credentials where empty or partially empty</exception>
        public Session Login(AccountCredentials data, string sessionClientToken = NidhoggClientToken, string agentName
            = "Minecraft")
        {
            if (data.Username.Equals("") || data.Password.Equals(""))
            {
                throw new ArgumentException("User Credentials may not be empty");
            }

            IRestResponse response = ExecuteRequest(EndpointAuthenticate, new
            {
                agent = new
                {
                    name = agentName,
                    version = 1
                },

                username = data.Username,
                password = data.Password,
                clientToken = sessionClientToken,
                requestUser = true
            });

            // if response is an error
            ThrowOnError(response);

            AuthenticationResponse authResponse = _deserializer.Deserialize<AuthenticationResponse>(response);
            return new Session(authResponse.selectedProfile.name, authResponse.accessToken, authResponse.clientToken);
        }

        /// <summary>
        /// Validates a session at Yggdrasil.
        /// </summary>
        /// <param name="session">Session structure containing a access token</param>
        ///
        /// <returns>true, if the session is valid. The method never returns false</returns>
        ///
        /// <exception cref="ArgumentException">If the session contains an empty access token</exception>
        /// <exception cref="InvalidSessionException">If the access token is either invalid or expired</exception>
        public bool Validate(Session session)
        {
            if (session.AccessToken.Equals(""))
            {
                throw new ArgumentException("Access token may not be empty");
            }

            IRestResponse response = ExecuteRequest(EndpointValidate, new
            {
                accessToken = session.AccessToken,
                clientToken = session.ClientToken
            });

            // invalid session or other error
            ThrowOnError(response);

            // session is valid
            return true;
        }

        /// <summary>
        /// Refreshes a valid or expired session. Note: if the session is still valid, its access token is invalidated
        /// and replaced with a new one.
        /// </summary>
        ///
        /// <param name="session">Session structure containing a valid/expired access token</param>
        ///
        /// <exception cref="ArgumentException">If the session contains an empty access token</exception>
        /// <exception cref="InvalidSessionException">If the session was never valid</exception>
        public void Refresh(ref Session session)
        {
            if (session.AccessToken.Equals(""))
            {
                throw new ArgumentException("Access token may not be empty");
            }

            IRestResponse response = ExecuteRequest(EndpointRefresh, new
            {
                accessToken = session.AccessToken,
                clientToken = session.ClientToken,
                requestUser = true
            });

            ThrowOnError(response);

            // the refresh response is similar to authentication response, except that the available profiles are
            // missing.
            AuthenticationResponse refreshResponse = _deserializer.Deserialize<AuthenticationResponse>(response);

            // refresh the session with latest data
            session.AccessToken = refreshResponse.accessToken;
            session.ClientToken = refreshResponse.clientToken;
            session.UserName = refreshResponse.selectedProfile.name;
        }

        /// <summary>
        /// Invalidates sessions associated with given account credentials
        /// </summary>
        ///
        /// <param name="data">Account credentials</param>
        ///
        /// <exception cref="ArgumentException">If user credentials where empty or partially empty</exception>
        /// <exception cref="UserMigratedException">If the account credentials contained a username, but account
        /// needs an E-Mail for login.</exception>
        /// <exception cref="InvalidCredentialException">If the given credentials where invalid</exception>
        public void SignOut(AccountCredentials data)
        {
            if (data.Username.Equals("") || data.Password.Equals(""))
            {
                throw new ArgumentException("User Credentials may not be empty");
            }

            IRestResponse response = ExecuteRequest(EndpointSignout, new
            {
                username = data.Username,
                password = data.Password
            });

            ThrowOnError(response);
        }

        /// <summary>
        /// Invalidates a session
        /// </summary>
        ///
        /// <param name="session">Session structure containing valid access token</param>
        ///
        /// <exception cref="ArgumentException">If the session contains an empty access token</exception>
        /// <exception cref="InvalidSessionException">If the session was already invalid or was never valid</exception>
        public void Invalidate(Session session)
        {
            if (session.AccessToken.Equals(""))
            {
                throw new ArgumentException("Access token may not be empty");
            }

            IRestResponse response = ExecuteRequest(EndpointInvalidate, new
            {
                accessToken = session.AccessToken,
                clientToken = session.ClientToken
            });

            ThrowOnError(response);
        }

        ///  <summary>
        ///  Executes a POST request to Yggdrasil with given request body and default settings (like User-Agent)
        ///  </summary>
        /// <param name="endpoint">The authentication REST service endpoint</param>
        /// <param name="body">Request body provided as an object that will be serialized to JSON</param>
        ///
        ///  <returns>the request response object</returns>
        private static IRestResponse ExecuteRequest(string endpoint, object body)
        {
            RestClient client = new RestClient(YggdrasilHostServer);
            RestRequest request = new RestRequest(endpoint, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            request.AddHeader("User-Agent", Nidhogg.NameAndVersion);

            request.AddBody(body);
            return client.Execute(request);
        }

        /// <summary>
        /// Analyzes a Yggdrasil web response and throws an exception, if it is an error response
        /// </summary>
        ///
        /// <param name="response">The JSON response of Yggdrasil</param>
        private void ThrowOnError(IRestResponse response)
        {
            if (!ErrorMessageRegex.IsMatch(response.Content)) return;

            ErrorResponse errorResponse = _deserializer.Deserialize<ErrorResponse>(response);

            // on user migrated error
            if (errorResponse.cause != null && errorResponse.cause.Equals("UserMigratedException"))
            {
                throw new UserMigratedException("User account has been migrated. Login with username is not allowed.");
            }

            // on invalid credentials
            if (errorResponse.errorMessage.Equals("Invalid credentials. Invalid username or password."))
            {
                throw new InvalidCredentialException("Provided account credentials where invalid.");
            }

            // on mojang authentication ban
            if (errorResponse.errorMessage.Equals("Invalid credentials."))
            {
                throw new YggdrasilBanException("The client is currently banned from Mojang authentication service " +
                                                "due to too many login attempts with invalid credentials. Last " +
                                                "credentials may be valid, though");
            }

            // access token invalid
            if (errorResponse.errorMessage.Equals("Invalid token"))
            {
                throw new InvalidSessionException("Invalid access token provided");
            }

            // unknown or unexpected exception

            throw new Exception((errorResponse.error ?? "unknown error") + ":\n" +
                                (errorResponse.errorMessage ?? "no description") + "\nCause: " +
                                (errorResponse.cause ?? "no cause"));
        }
    }
}