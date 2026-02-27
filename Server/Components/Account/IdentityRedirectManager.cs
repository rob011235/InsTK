// <copyright file="IdentityRedirectManager.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Identity;
    using Server.Data;

    /// <summary>
    /// Manages navigation and status message redirection for identity-related pages.
    /// </summary>
    internal sealed class IdentityRedirectManager
    {
        /// <summary>
        /// The name of the status message cookie.
        /// </summary>
        public const string StatusCookieName = "Identity.StatusMessage";

        private static readonly CookieBuilder StatusCookieBuilder = new()
        {
            SameSite = SameSiteMode.Strict,
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromSeconds(5),
        };

        private readonly NavigationManager navigationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRedirectManager"/> class.
        /// </summary>
        /// <param name="navigationManager">The navigation manager to use for redirects.</param>
        public IdentityRedirectManager(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        /// <summary>
        /// Gets the current absolute path.
        /// </summary>
        private string CurrentPath => this.navigationManager.ToAbsoluteUri(this.navigationManager.Uri).GetLeftPart(UriPartial.Path);

        /// <summary>
        /// Redirects to the specified URI.
        /// </summary>
        /// <param name="uri">The URI to redirect to.</param>
        public void RedirectTo(string? uri)
        {
            uri ??= string.Empty;

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = this.navigationManager.ToBaseRelativePath(uri);
            }

            this.navigationManager.NavigateTo(uri);
        }

        /// <summary>
        /// Redirects to the specified URI with query parameters.
        /// </summary>
        /// <param name="uri">The base URI.</param>
        /// <param name="queryParameters">The query parameters to append.</param>
        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
        {
            var uriWithoutQuery = this.navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = this.navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            this.RedirectTo(newUri);
        }

        /// <summary>
        /// Redirects to the specified URI and sets a status message cookie.
        /// </summary>
        /// <param name="uri">The URI to redirect to.</param>
        /// <param name="message">The status message to set in the cookie.</param>
        /// <param name="context">The HTTP context for setting the cookie.</param>
        public void RedirectToWithStatus(string uri, string message, HttpContext context)
        {
            context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
            this.RedirectTo(uri);
        }

        /// <summary>
        /// Redirects to the current page.
        /// </summary>
        public void RedirectToCurrentPage() => this.RedirectTo(this.CurrentPath);

        /// <summary>
        /// Redirects to the current page and sets a status message cookie.
        /// </summary>
        /// <param name="message">The status message to set in the cookie.</param>
        /// <param name="context">The HTTP context for setting the cookie.</param>
        public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
            => this.RedirectToWithStatus(this.CurrentPath, message, context);

        /// <summary>
        /// Redirects to the invalid user page with an error message.
        /// </summary>
        /// <param name="userManager">The user manager for retrieving the user ID.</param>
        /// <param name="context">The HTTP context for setting the cookie.</param>
        public void RedirectToInvalidUser(UserManager<ApplicationUser> userManager, HttpContext context)
            => this.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
    }
}
