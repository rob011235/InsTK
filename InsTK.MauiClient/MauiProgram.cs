using InsTK.MauiClient.Services.Backend;
using InsTK.MauiClient.Services.Ollama;
using InsTK.MauiClient.Services.Settings;
using InsTK.MauiClient.Services.Workspace;
using Microsoft.Extensions.Logging;

namespace InsTK.MauiClient;

public static class MauiProgram
{
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
        builder.Services.AddSingleton<IWorkspaceService, WorkspaceService>();
        builder.Services.AddSingleton<IOllamaService, OllamaService>();
        builder.Services.AddSingleton<IBackendSessionService, BackendSessionService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
