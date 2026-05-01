// Copyright (c) Robert Garner. All rights reserved.

using Android.App;
using Android.Runtime;

namespace InsTK.MauiClient;

[Application]
/// <summary>
/// Represents the Android application host for the MAUI client.
/// </summary>
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainApplication"/> class.
    /// </summary>
    /// <param name="handle">The native Android application handle.</param>
    /// <param name="ownership">The ownership semantics for the native handle.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>
    /// Creates the shared MAUI application for Android.
    /// </summary>
    /// <returns>The configured MAUI application instance.</returns>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
