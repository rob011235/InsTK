// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient;

/// <summary>
/// Represents the root .NET MAUI application for the instructor workstation.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Creates the initial application window for the workstation shell.
    /// </summary>
    /// <param name="activationState">The activation state supplied by the MAUI host.</param>
    /// <returns>The configured application window.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "InsTK Instructor Workstation" };
    }
}
