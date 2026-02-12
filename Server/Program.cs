// <copyright file="Program.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Server
{
    using System.Threading.Tasks;
    using Client.Pages;
    using Common.Interfaces;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Server.Components;
    using Server.Components.Account;
    using Server.Data;
    using Server.Data.Services;

    /// <summary>
    /// Provides the main entry point for the InsTK application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the InsTK application.
        /// </summary>
        /// <param name="args">An array of command-line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

            // GitHub auth is separate because AddIdentityCookies() returns IdentityCookiesBuilder
            builder.Services
                .AddAuthentication()
                .AddCookie("GitHubCookie")
                .AddOAuth("GitHub", options =>
                {
                    options.ClientId = builder.Configuration["GitHub:ClientId"]!;
                    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]!;
                    options.CallbackPath = "/signin-github";

                    // IMPORTANT: keep GitHub auth isolated from Identity sign-in
                    options.SignInScheme = "GitHubCookie";

                    // Pick one:
                    options.Scope.Add("public_repo");

                    // options.Scope.Add("repo");

                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";

                    options.SaveTokens = true;

                    options.Events.OnCreatingTicket = async ctx =>
                    {
                        using var req = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                        req.Headers.Add("Accept", "application/vnd.github+json");
                        req.Headers.Add("User-Agent", "InsTK");
                        req.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);

                        using var resp = await ctx.Backchannel.SendAsync(req);
                        resp.EnsureSuccessStatusCode();
                    };
                });

            builder.Services.AddHttpClient();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddBlazorBootstrap();

            // Data services
            builder.Services.AddTransient<ICoursesDataService, CoursesDataService>();
            builder.Services.AddTransient<IObjectivesDataService, ObjectivesDataService>();

            // To enable web api
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();
            app.UseStaticFiles();
            app.MapStaticAssets();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            RunMigrations(app);

            // Add default admin user
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "Instructor" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        IdentityRole roleRole = new (role);
                        await roleManager.CreateAsync(roleRole);
                    }
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                string? email = builder.Configuration.GetSection("Admin:Email").Value;
                string? password = builder.Configuration.GetSection("Admin:Password").Value;
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password) && await userManager.FindByNameAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,

                        // Optional, add if you want the account live right away without email confirmation
                        EmailConfirmed = true,
                    };

                    var results = await userManager.CreateAsync(user, password);
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            // To enable web api
            app.MapControllers();

            app.Run();
        }

        private static void RunMigrations(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }
        }
    }
}
