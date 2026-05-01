// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.Server.Services.Ollama;

/// <summary>
/// Represents the effective Ollama connection settings used by the web application.
/// </summary>
/// <param name="BaseUrl">The Ollama server base URL.</param>
/// <param name="Model">The default Ollama model name.</param>
public sealed record OllamaChatConfiguration(string BaseUrl, string Model);
