// Copyright (c) Robert Garner. All rights reserved.

using ObjCRuntime;
using UIKit;

namespace InsTK.MauiClient;

/// <summary>
/// Provides the Mac Catalyst application entry point.
/// </summary>
public class Program
{
    /// <summary>
    /// Starts the Mac Catalyst application host.
    /// </summary>
    /// <param name="args">The process command-line arguments.</param>
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
