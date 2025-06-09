namespace T3.Clone.Client.Services;

public class ChatModelResponse
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public string Response { get; set; } = string.Empty;
    public bool IsError { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public string ModelProvider { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    
}