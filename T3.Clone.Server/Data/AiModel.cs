namespace T3.Clone.Server.Data;

public class AiModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;


    public bool IsDefault { get; set; } = false;

    public bool HasImageSupport { get; set; } = false;
    public string SupportedContentTypes { get; set; } = string.Empty;

    public bool HasImageGenerationSupport { get; set; } = false;

    public bool HasThinkingSupport { get; set; } = false;

    public string SystemPrompt { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ApiUrl { get; set; } = null;
    public string ApiKey { get; set; } = null;
    
    // Cost per 1m tokens
    public double InputTokenCost { get; set; } = 0.0;
    public double OutputTokenCost { get; set; } = 0.0;
    public double ThinkingCost { get; set; } = 0.0;
    // Cost per image
    public double ImageGenerationCost { get; set; } = 0.0;
    // Max tokens for input and output
    public int MaxInputTokens { get; set; } = 0;
    public int MaxOutputTokens { get; set; } = 0;
}