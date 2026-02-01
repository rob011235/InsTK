// <copyright file="PasskeyInputModel.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    /// <summary>
    /// Represents the input model for passkey authentication, containing the credential JSON and any error message.
    /// </summary>
    public class PasskeyInputModel
    {
        /// <summary>
        /// Gets or sets the credential JSON string used for passkey authentication.
        /// </summary>
        public string? CredentialJson { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any, related to the passkey input.
        /// </summary>
        public string? Error { get; set; }
    }
}
