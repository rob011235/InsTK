// Copyright (c) Robert Garner. All rights reserved.

namespace InsTK.MauiClient.Services.Ollama;

/// <summary>
/// Represents a generated Ollama response along with the model that produced it.
/// </summary>
/// <param name="Response">The generated response text.</param>
/// <param name="Model">The model that produced the response.</param>
/// <param name="UsedFallback">A value indicating whether the fallback model was used.</param>
public sealed record OllamaPromptResult(string Response, string Model, bool UsedFallback);
