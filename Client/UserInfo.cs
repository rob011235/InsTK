namespace Client
{
    // Add properties to this class and update the server and client AuthenticationStateProviders
    // to expose more information about the authenticated user to the client.
    /// <summary>
    /// Represents the authenticated user payload persisted for client-side authentication state.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the user.
        /// </summary>
        public required IEnumerable<string> Roles { get; set; }
    }
}
