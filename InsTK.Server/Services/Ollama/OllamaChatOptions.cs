// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.Server.Services.Ollama;

/// <summary>
/// Stores configurable Ollama settings for the web application.
/// </summary>
public sealed class OllamaChatOptions
{
    /// <summary>
    /// Gets or sets the Ollama server base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "http://127.0.0.1:11434";

    /// <summary>
    /// Gets or sets the default model name used for prompt generation.
    /// </summary>
    public string Model { get; set; } = "qwen3-coder:30b";
}
