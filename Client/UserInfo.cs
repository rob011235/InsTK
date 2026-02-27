// <copyright file="UserInfo.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Client
{
    /// <summary>
    /// Represents the authenticated user payload persisted for client-side authentication state.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        required public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        required public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the user.
        /// </summary>
        required public IEnumerable<string> Roles { get; set; }
    }
}
