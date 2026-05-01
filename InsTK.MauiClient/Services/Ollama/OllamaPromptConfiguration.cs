// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Represents the effective endpoint and model used for interactive Ollama prompts.
/// </summary>
/// <param name="BaseUrl">The configured Ollama base URL.</param>
/// <param name="Model">The model name used for generation.</param>
public sealed record OllamaPromptConfiguration(string BaseUrl, string Model);
