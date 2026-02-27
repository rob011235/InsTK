// <copyright file="IdentityRevalidatingAuthenticationStateProvider.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.Server;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Server.Data;

    /// <summary>
    /// Provides a server-side <see cref="AuthenticationStateProvider"/> that revalidates the security stamp
    /// for the connected user every 30 minutes while an interactive circuit is connected.
    /// </summary>
    internal sealed class IdentityRevalidatingAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityOptions> options)
        : RevalidatingServerAuthenticationStateProvider(loggerFactory)
    {
        /// <summary>
        /// Gets the interval between security stamp revalidations.
        /// </summary>
        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        /// <summary>
        /// Validates the authentication state by checking the user's security stamp.
        /// </summary>
        /// <param name="authenticationState">The current authentication state.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>True if the authentication state is valid; otherwise, false.</returns>
        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            // Get the user manager from a new scope to ensure it fetches fresh data
            await using var scope = scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            return await this.ValidateSecurityStampAsync(userManager, authenticationState.User);
        }

        /// <summary>
        /// Validates the security stamp for the specified principal.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="principal">The claims principal.</param>
        /// <returns>True if the security stamp is valid; otherwise, false.</returns>
        private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return false;
            }
            else if (!userManager.SupportsUserSecurityStamp)
            {
                return true;
            }
            else
            {
                var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
                var userStamp = await userManager.GetSecurityStampAsync(user);
                return principalStamp == userStamp;
            }
        }
    }
}
