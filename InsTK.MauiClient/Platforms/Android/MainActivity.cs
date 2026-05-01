// Copyright (c) Robert Garner. All rights reserved.

using Android.App;
using Android.Content.PM;
using Android.OS;

namespace InsTK.MauiClient;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
/// <summary>
/// Represents the Android entry activity for the MAUI client.
/// </summary>
public class MainActivity : MauiAppCompatActivity
{
}
