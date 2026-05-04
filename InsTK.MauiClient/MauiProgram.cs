using InsTK.MauiClient.Services.Backend;
using InsTK.MauiClient.Services.Brightspace;
using InsTK.MauiClient.Services.Ollama;
using InsTK.MauiClient.Services.Settings;
using InsTK.MauiClient.Services.Workstation;
using InsTK.MauiClient.Services.Workspace;
using Microsoft.Extensions.Logging;

namespace InsTK.MauiClient;

/// <summary>
/// Configures the InsTK MAUI Blazor application and its dependency injection container.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates the MAUI application instance.
    /// </summary>
    /// <returns>The configured MAUI application.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IClientSettingsService, ClientSettingsService>();
        builder.Services.AddSingleton<IWorkstationProfileService, WorkstationProfileService>();
        builder.Services.AddSingleton<IWorkspaceService, WorkspaceService>();
        builder.Services.AddSingleton<IOllamaModelCatalogService, OllamaModelCatalogService>();
        builder.Services.AddSingleton<IOllamaService, OllamaService>();
        builder.Services.AddSingleton<IOllamaPromptService, OllamaPromptService>();
        builder.Services.AddSingleton<IOllamaRuntimeService, OllamaRuntimeService>();
        builder.Services.AddSingleton<IBackendSessionService, BackendSessionService>();
        builder.Services.AddSingleton<IBrightspaceAutomationService, BrightspaceAutomationService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
