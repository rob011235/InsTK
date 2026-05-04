# VS Code F5 Setup

Use the checked-in VS Code debug configuration to launch both the server and the MAUI client together.

## Requirements

- Install the C# extension or C# Dev Kit in VS Code.
- Install the .NET MAUI extension in VS Code for the MAUI project experience.
- Ensure the .NET SDK and MAUI workloads are installed on the machine.

## How To Run

1. Open the repo root in VS Code.
2. Open the Run and Debug view.
3. Select `InsTK Server + MAUI` from the debug dropdown.
4. Press `F5`.

## What The Workspace Provides

- `.vscode/launch.json`
  - `InsTK Server`
  - `InsTK MAUI Client`
  - compound configuration: `InsTK Server + MAUI`
- `.vscode/tasks.json`
  - builds the server project before launch
  - builds the MAUI client before launch

## Notes

- The server uses the project launch settings and should listen on the development URLs defined in `InsTK.Server/Properties/launchSettings.json`.
- The MAUI client defaults its backend URL to `https://localhost:7016` because the ASP.NET development certificate reliably covers `localhost`.
- If the MAUI client is already running, stop it before pressing `F5` again to avoid output file locking during the build.
