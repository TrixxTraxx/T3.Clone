namespace LLMLab.Dtos.Model;

public class AiModelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public string RequiredApiKey { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool HasRequiredApiKey { get; set; } = true;

    public bool IsDefault { get; set; } = false;

    public bool HasImageSupport { get; set; } = false;
    public string SupportedContentTypes { get; set; } = string.Empty;
    
    //unused for now
    public bool HasImageGenerationSupport { get; set; } = false;
    
    public bool HasThinkingSupport { get; set; } = false;
    public bool RequireThinking { get; set; } = false;
    
    // Max tokens for input and output
    public int MaxInputTokens { get; set; } = 0;
    public int MaxOutputTokens { get; set; } = 0;
}