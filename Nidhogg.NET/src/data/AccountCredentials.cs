namespace Nidhogg.data
{
    /// <summary>
    /// Contains account credentials. Username can be the Mojang account email or the Minecraft username, if the
    /// account has not been migrated yet.
    /// </summary>
    public struct AccountCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public AccountCredentials(string userName, string password)
        {
            this.Username = userName;
            this.Password = password;
        }
    }
}