// <copyright file="Program.cs" company="Rob Garner (rgarner011235@gmail.com)">
// Copyright (c) Rob Garner (rgarner011235@gmail.com). All rights reserved.
// </copyright>

namespace Client
{
    using Client;
    using Client.Data.Services;
    using Common.Interfaces;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

    /// <summary>
    /// The entry point for the WebAssembly client application.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Configures and runs the WebAssembly host.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private static async Task Main(string[] args)
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
                    "https://localhost:7150/"),
            });

            // Data services
            builder.Services.AddTransient<ICoursesDataService, CoursesClientDataService>();
            builder.Services.AddTransient<IObjectivesDataService, ObjectivesClientDataService>();

            await builder.Build().RunAsync();
        }
    }
}
