// <copyright file="IdentityNoOpEmailSender.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server.Components.Account
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Server.Data;

    /// <summary>
    /// Provides a no-operation implementation of <see cref="IEmailSender{TUser}"/> for <see cref="ApplicationUser"/>.
    /// Used as a placeholder until a real email sender is implemented.
    /// </summary>
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        /// <summary>
        /// Sends a confirmation link email to the specified user.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <param name="email">The email address to send to.</param>
        /// <param name="confirmationLink">The confirmation link.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        /// <summary>
        /// Sends a password reset link email to the specified user.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <param name="email">The email address to send to.</param>
        /// <param name="resetLink">The password reset link.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        /// <summary>
        /// Sends a password reset code email to the specified user.
        /// </summary>
        /// <param name="user">The application user.</param>
        /// <param name="email">The email address to send to.</param>
        /// <param name="resetCode">The password reset code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
