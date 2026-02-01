using Client;
using Client.Data.Services;
using Common.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthenticationStateDeserialization();

            // Add http client
            builder.Services.AddScoped(sp =>
            new HttpClient
            {
                BaseAddress = new Uri(builder.Configuration["FrontendUrl"] ??
                    "https://localhost:7150/")
            });

            // Data services
            builder.Services.AddTransient<ICoursesDataService, CoursesClientDataService>();
            builder.Services.AddTransient<IObjectivesDataService, ObjectivesClientDataService>();

            await builder.Build().RunAsync();
        }
    }
}
