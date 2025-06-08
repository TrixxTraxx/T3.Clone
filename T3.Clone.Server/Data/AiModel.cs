namespace T3.Clone.Server.Data;

public class AiModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string ModelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ApiKey { get; set; } = null;
    public string? ApiUrl { get; set; } = null;
    
}