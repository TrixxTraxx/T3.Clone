namespace T3.Clone.Server.Data;

public class AiModelKeys
{
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public string Identifier { get; set; } = string.Empty;
    
    public string ApiKey { get; set; } = string.Empty;
}