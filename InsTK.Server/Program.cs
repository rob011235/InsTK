using InsTK.WebClient.Pages;
using InsTK.Server.Components;
using InsTK.Server.Components.Account;
using InsTK.Server.Components.Pages.Tutorials;
using InsTK.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Syncfusion.Blazor;
using System.IO;

namespace InsTK.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            RegisterSyncfusionLicense(builder);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    options.DetailedErrors = builder.Environment.IsDevelopment();
                })
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services.AddSyncfusionBlazor();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

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

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.UseStaticFiles();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(WebClient._Imports).Assembly);

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            RunMigrations(app);
            await UpgradeLegacyTutorialContentAsync(app);

            await AddRolesAsync(app);
            await AddAdminUser(app);

            app.Run();
        }

        private static void RegisterSyncfusionLicense(WebApplicationBuilder builder)
        {
            var licenseKey = builder.Configuration["Syncfusion:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                return;
            }

            if (builder.Environment.IsDevelopment())
            {
                // TODO: Configure Syncfusion:LicenseKey via user secrets or an environment variable for local development.
                return;
            }

            Console.Error.WriteLine("Syncfusion:LicenseKey is not configured. Syncfusion components may show a license warning at runtime.");
        }

        private static void RunMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.Migrate();
        }

        private static async Task UpgradeLegacyTutorialContentAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
            await using var db = await factory.CreateDbContextAsync();

            var tutorials = await db.Tutorials
                .Where(t => !string.IsNullOrWhiteSpace(t.ContentHtml))
                .ToListAsync();

            var changed = false;
            foreach (var tutorial in tutorials)
            {
                if (TutorialContentFormatter.LooksLikeHtmlFragment(tutorial.ContentHtml))
                {
                    continue;
                }

                tutorial.ContentHtml = TutorialContentFormatter.BuildLegacyHtmlFromMarkdown(tutorial.ContentHtml);
                changed = true;
            }

            if (changed)
            {
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Adds an admin user to the identity system at application startup if the admin credentials are provided in configuration.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        private static async Task AddAdminUser(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                string? email = app.Configuration.GetSection("Admin:Email").Value;
                string? password = app.Configuration.GetSection("Admin:Password").Value;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    // Nothing to do when admin credentials are not provided.
                    return;
                }

                var user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                };

                var errors = new System.Collections.Generic.List<string>();

                bool isValidPassword = await ValidatePassword(userManager, password, user, errors);

                if (!isValidPassword)
                {
                    Console.Error.WriteLine($"Admin password does not meet complexity requirements: {string.Join(" ", errors)}");
                    return;
                }

                if (await userManager.FindByNameAsync(email) == null)
                {
                    var results = await userManager.CreateAsync(user, password);
                    if (results.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Failed to create admin user: {string.Join(", ", results.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates the specified password for the given user using the configured password validators.
        /// </summary>
        /// <param name="userManager">The user manager instance.</param>
        /// <param name="password">The password to validate.</param>
        /// <param name="user">The user for whom the password is being validated.</param>
        /// <param name="errors">A list to which any validation error messages will be added.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains <c>true</c> if the password is valid; otherwise, <c>false</c>.
        /// </returns>
        private static async Task<bool> ValidatePassword(UserManager<ApplicationUser> userManager, string password, ApplicationUser user, List<string> errors)
        {
            bool isValidPassword = true;

            // Validate password using configured Identity password validators
            var validators = userManager.PasswordValidators;
            if (validators != null && validators.Count > 0)
            {
                foreach (var validator in validators)
                {
                    var validationResult = await validator.ValidateAsync(userManager, user, password);
                    if (!validationResult.Succeeded)
                    {
                        errors.AddRange(validationResult.Errors.Select(e => e.Description));
                        isValidPassword = false;
                    }
                }
            }
            else
            {
                // Fallback basic checks in case no validators are configured
                if (password.Length < 6)
                {
                    errors.Add("Password must be at least 6 characters.");
                }

                if (!password.Any(char.IsUpper))
                {
                    errors.Add("Password must contain an uppercase letter.");
                }

                if (!password.Any(char.IsLower))
                {
                    errors.Add("Password must contain a lowercase letter.");
                }

                if (!password.Any(char.IsDigit))
                {
                    errors.Add("Password must contain a digit.");
                }

                if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    errors.Add("Password must contain a non-alphanumeric character.");
                }

                if (errors.Count > 0)
                {
                    isValidPassword = false;
                }
            }

            return isValidPassword;
        }

        /// <summary>
        /// Adds required roles to the identity system at application startup if they do not already exist.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        private static async Task AddRolesAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "Instructor" };
                foreach (var role in roles)
                {
                    if (await roleManager.RoleExistsAsync(role))
                    {
                        continue;
                    }

                    IdentityRole identityRole = new IdentityRole(role);
                    var result = await roleManager.CreateAsync(identityRole);
                    if (!result.Succeeded)
                    {
                        Console.Error.WriteLine($"Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
