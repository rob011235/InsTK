// <copyright file="UserInfo.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Client
{
    /// <summary>
    /// Represents information about an authenticated user.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        required public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        required public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles assigned to the user.
        /// </summary>
        required public IEnumerable<string> Roles { get; set; }
    }
}
