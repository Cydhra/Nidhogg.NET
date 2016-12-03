namespace Nidhogg.data
{
    /// <summary>
    /// Contains account credentials. UserName can be the Mojang account email or the Minecraft username, if the
    /// account has not been migrated yet.
    /// </summary>
    public struct AccountCredentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public AccountCredentials(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }
}