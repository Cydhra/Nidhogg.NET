namespace Nidhogg.data
{
    /// <summary>
    /// A Mojang Session object, similar to the one used by Minecraft. It contains the user name of authorized player,
    /// the session access token and a the client token used for authentication
    /// </summary>
    public struct Session
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }

        public Session(string userName, string accessToken, string clientToken)
        {
            this.UserName = userName;
            this.AccessToken = accessToken;
            this.ClientToken = clientToken;
        }
    }
}