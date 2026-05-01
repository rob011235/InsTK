using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using InsTK.WebClient.Pages;
using InsTK.Server.Components;
using InsTK.Server.Components.Account;
using InsTK.Server.Data;
using InsTK.Server.Services.Ollama;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace InsTK.Server
{
    public class Program
    {
        private const long MaxMarkdownImageBytes = 2 * 1024 * 1024;

        private static readonly HashSet<string> AllowedMarkdownImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".gif"
        };

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services
                .AddBlazorise(options => options.Immediate = true)
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
            builder.Services.Configure<OllamaChatOptions>(builder.Configuration.GetSection("Ollama"));
            builder.Services.AddHttpClient<IOllamaChatService, OllamaChatService>((services, client) =>
            {
                var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaChatOptions>>().Value;
                var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl) ? "http://127.0.0.1:11434" : options.BaseUrl.Trim().TrimEnd('/');
                client.BaseAddress = new Uri($"{baseUrl}/", UriKind.Absolute);
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
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

            app.UseAntiforgery();
            app.UseStaticFiles();

            app.MapPost("/tutorials/markdown-images", async (HttpRequest request, IWebHostEnvironment environment) =>
                {
                    var form = await request.ReadFormAsync();
                    var image = form.Files["image"];

                    if (image is null)
                    {
                        return Results.BadRequest(new { error = "Select an image to upload." });
                    }

                    var extension = Path.GetExtension(image.FileName ?? string.Empty);
                    if (!AllowedMarkdownImageExtensions.Contains(extension))
                    {
                        return Results.BadRequest(new { error = "Only PNG, JPG, JPEG, and GIF images are allowed." });
                    }

                    if (image.Length > MaxMarkdownImageBytes)
                    {
                        return Results.BadRequest(new { error = "Image must be 2 MB or smaller." });
                    }

                    var uploadsRoot = Path.Combine(environment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsRoot);

                    var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                    var physicalPath = Path.Combine(uploadsRoot, fileName);

                    await using var targetStream = File.Create(physicalPath);
                    await using var sourceStream = image.OpenReadStream();
                    await sourceStream.CopyToAsync(targetStream);

                    return Results.Json(new { data = new { filePath = $"/uploads/{fileName}" } });
                })
                .DisableAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(WebClient._Imports).Assembly);

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
