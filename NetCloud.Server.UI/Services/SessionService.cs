namespace DotNetCloud.Server.UI.Services
{
    public class SessionService
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Check if the current user has a specific role
        /// </summary>
        public bool HasRole(string roleName)
        {
            return Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if the current user is an Administrator
        /// </summary>
        public bool IsAdministrator => HasRole("Administrator");
    }
}
