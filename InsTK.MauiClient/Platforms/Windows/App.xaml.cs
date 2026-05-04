// Copyright (c) Robert Garner. All rights reserved.

using Microsoft.UI.Xaml;

namespace InsTK.MauiClient.WinUI;

/// <summary>
/// Provides the Windows-specific MAUI application bootstrapper.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Creates the shared MAUI application for the Windows host.
    /// </summary>
    /// <returns>The configured MAUI application instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
