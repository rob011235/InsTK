// <copyright file="GitHubRepoZipController.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for downloading a GitHub repository as a zip file using the authenticated user's access token.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = "GitHubCookie")]
public class GitHubRepoZipController : ControllerBase
{
    /// <summary>
    /// Downloads the specified GitHub repository as a zip file.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repo">The name of the repository.</param>
    /// <param name="ref">The name of the commit/branch/tag. Defaults to the repository’s default branch.</param>
    /// <returns>The zip file of the repository, or an error response.</returns>
    [HttpGet("/github/repo-zip")]
    public async Task<IActionResult> DownloadZip([FromQuery] string owner, [FromQuery] string repo, [FromQuery] string? @ref)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return this.BadRequest("owner and repo are required");
        }

        var token = await this.HttpContext.GetTokenAsync("GitHubCookie", "access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return this.Unauthorized("Missing GitHub access token. Login again.");
        }

        var refPart = string.IsNullOrWhiteSpace(@ref) ? string.Empty : $"/{Uri.EscapeDataString(@ref)}";
        var apiUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/zipball{refPart}";

        using var handler = new HttpClientHandler { AllowAutoRedirect = true };
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        request.Headers.Add("Accept", "application/vnd.github+json");
        request.Headers.Add("User-Agent", "InsTK");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            return this.StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var fileName = $"{owner}-{repo}{(string.IsNullOrWhiteSpace(@ref) ? string.Empty : "-" + @ref)}.zip";

        return this.File(stream, "application/zip", fileName);
    }
}
