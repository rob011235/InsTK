namespace Server.Components.Account
{
    /// <summary>
    /// Represents the passkey operation requested by the account UI.
    /// </summary>
    public enum PasskeyOperation
    {
        /// <summary>
        /// Creates a new passkey credential.
        /// </summary>
        Create = 0,

        /// <summary>
        /// Requests authentication with an existing passkey.
        /// </summary>
        Request = 1,
    }
}
