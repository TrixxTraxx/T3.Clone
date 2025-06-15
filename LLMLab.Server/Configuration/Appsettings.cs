namespace LLMLab.Server.Configuration;

public class Appsettings
{
    public string ClientUrl { get; set; } = string.Empty;
    public string CookieDomain { get; set; } = string.Empty;
    public string[] AllowedOrigins { get; set; }
    
    //settings to generate thread titles
    //currently only supports OpenRouter and OpenAi
    public string ThreadTitleModel { get; set; } = "gpt-4.1-nano";
    public string ThreadTitleApiKey { get; set; } = "{OpenAi}";
    public string ThreadTitleUrl { get; set; } = "https://api.openai.com/v1";
    //Max length of the generated title in tokens
    public int MaxTitleLength { get; set; } = 20;
    //Max length of the generated input to the llm in characters, used to limit cost
    public int MaxTitleInputLenght { get; set; } = 1000;
    public string ThreadTitleGenerationPrompt { get; set; } = 
        "Generate a short and descriptive title for the following conversation:\n\n{0}\n\nThe title should be concise, informative, and relevant to the content of the conversation. It should not exceed 20 Charcters in length. The content of the first message of this thread is: ```{MessageContent}```";
}