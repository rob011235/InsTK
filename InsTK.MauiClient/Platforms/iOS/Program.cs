// Copyright (c) Robert Garner. All rights reserved.

using ObjCRuntime;
using UIKit;

namespace InsTK.MauiClient;

/// <summary>
/// Provides the iOS application entry point.
/// </summary>
public class Program
{
    /// <summary>
    /// Starts the iOS application host.
    /// </summary>
    /// <param name="args">The process command-line arguments.</param>
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
