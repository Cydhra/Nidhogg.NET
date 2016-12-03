// ReSharper disable InconsistentNaming
// because naming is linked to JSON response naming

using System.Collections.Generic;

namespace Nidhogg.rest.responses
{
    /// <summary>
    /// An authentication response as received by the yggdrasil client on login. Contains references to further
    /// structures describing nested objects
    /// </summary>
    public struct AuthenticationResponse
    {
        public string accessToken { get; set; }
        public string clientToken { get; set; }

        public List<Profile> availableProfiles { get; set; }

        public Profile selectedProfile { get; set; }
        public User user { get; set; }
    }

    /// <summary>
    /// A Yggdrasil user profile
    /// </summary>
    public struct Profile
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool legacy { get; set; }
    }

    /// <summary>
    /// A Yggdrasil Minecraft user
    /// </summary>
    public struct User
    {
        public string id { get; set; }
        public List<UserProperty> properties { get; set; }
    }

    /// <summary>
    /// A user property consisting of a name and value pair
    /// </summary>
    public struct UserProperty
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}