// <copyright file="IdentityRedirectManager.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Manages redirection logic for identity-related components, including status message handling via cookies.
    /// </summary>
    internal sealed class IdentityRedirectManager
    {
        /// <summary>
        /// The name of the status message cookie.
        /// </summary>
        public const string StatusCookieName = "Identity.StatusMessage";

        /// <summary>
        /// The <see cref="CookieBuilder"/> used to configure the status message cookie.
        /// </summary>
        private static readonly CookieBuilder StatusCookieBuilder = new ()
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
        /// <param name="navigationManager">The navigation manager used for redirects.</param>
        public IdentityRedirectManager(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        /// <summary>
        /// Redirects to the specified URI.
        /// </summary>
        /// <param name="uri">The URI to redirect to.</param>
        /// <exception cref="InvalidOperationException">Thrown if used outside of static rendering.</exception>
        [DoesNotReturn]
        public void RedirectTo(string? uri)
        {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = navigationManager.ToBaseRelativePath(uri);
            }

            // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
            // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
            navigationManager.NavigateTo(uri);
            throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
        }

        /// <summary>
        /// Redirects to the specified URI with additional query parameters.
        /// </summary>
        /// <param name="uri">The base URI to redirect to.</param>
        /// <param name="queryParameters">The query parameters to append.</param>
        [DoesNotReturn]
        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
        {
            var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectTo(newUri);
        }

        /// <summary>
        /// Redirects to the specified URI and sets a status message in a cookie.
        /// </summary>
        /// <param name="uri">The URI to redirect to.</param>
        /// <param name="message">The status message to set.</param>
        /// <param name="context">The HTTP context for setting the cookie.</param>
        [DoesNotReturn]
        public void RedirectToWithStatus(string uri, string message, HttpContext context)
        {
            context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
            RedirectTo(uri);
        }

        /// <summary>
        /// Gets the current absolute path.
        /// </summary>
        private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

        /// <summary>
        /// Redirects to the current page.
        /// </summary>
        [DoesNotReturn]
        public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

        /// <summary>
        /// Redirects to the current page and sets a status message in a cookie.
        /// </summary>
        /// <param name="message">The status message to set.</param>
        /// <param name="context">The HTTP context for setting the cookie.</param>
        [DoesNotReturn]
        public void RedirectToCurrentPageWithStatus(string message, HttpContext context)
            => RedirectToWithStatus(CurrentPath, message, context);
    }
}
