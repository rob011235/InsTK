// Copyright (c) Robert Garner. All rights reserved.

using Foundation;

namespace InsTK.MauiClient;

[Register("AppDelegate")]
/// <summary>
/// Represents the Mac Catalyst application delegate for the MAUI client.
/// </summary>
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>
    /// Creates the shared MAUI application for Mac Catalyst.
    /// </summary>
    /// <returns>The configured MAUI application instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
