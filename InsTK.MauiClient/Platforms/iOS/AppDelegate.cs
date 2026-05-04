// Copyright (c) Robert Garner. All rights reserved.

using Foundation;

namespace InsTK.MauiClient;

[Register("AppDelegate")]
/// <summary>
/// Represents the iOS application delegate for the MAUI client.
/// </summary>
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>
    /// Creates the shared MAUI application for iOS.
    /// </summary>
    /// <returns>The configured MAUI application instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
