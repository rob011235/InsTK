// <copyright file="GitHubOAuthController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for handling GitHub OAuth authentication.
/// </summary>
[ApiController]
public class GitHubOAuthController : ControllerBase
{
    /// <summary>
    /// Initiates the GitHub OAuth login process.
    /// </summary>
    /// <param name="returnUrl">The URL to redirect to after login. Defaults to "/github-zip".</param>
    /// <returns>A challenge result that triggers the GitHub authentication flow.</returns>
    [HttpGet("/auth/github/login")]
    public IActionResult Login([FromQuery] string? returnUrl = "/github-zip")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/github-zip",
        };

        return this.Challenge(props, "GitHub");
    }

    /// <summary>
    /// Logs the user out of the GitHub authentication session.
    /// </summary>
    /// <param name="returnUrl">The URL to redirect to after logout. Defaults to "/github-zip".</param>
    /// <returns>A redirect to the specified return URL.</returns>
    [Authorize(AuthenticationSchemes = "GitHubCookie")]
    [HttpGet("/auth/github/logout")]
    public async Task<IActionResult> Logout([FromQuery] string? returnUrl = "/github-zip")
    {
        await this.HttpContext.SignOutAsync("GitHubCookie");
        return this.LocalRedirect(returnUrl ?? "/github-zip");
    }
}
