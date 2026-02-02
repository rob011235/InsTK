// <copyright file="ApplicationUser.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Data
{
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Represents an application user with additional profile data.
    /// Inherits from <see cref="IdentityUser"/>.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
    }
}
