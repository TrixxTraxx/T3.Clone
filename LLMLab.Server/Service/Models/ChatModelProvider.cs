using LLMLab.Server.Data;

namespace LLMLab.Server.Service.Models;

public class ChatModelProvider(
    IServiceProvider serviceProvider,
    ApplicationDbContext context
)
{
    private Dictionary<string, Type> _chatModels = new Dictionary<string, Type>()
    {
        { "OpenAi", typeof(OpenAiChat)},
        { "OpenAiReasoning", typeof(OpenAiReasoningChat)},
        { "Anthropic", typeof(AnthropicChat)},
        { "Google", typeof(GoogleChat)}
    };
    
    public IChatModel GetChatModel(AiModel aiModel)
    {
        var type = _chatModels[aiModel.Provider];
        return (serviceProvider.GetService(type) as IChatModel)!;
    }
    
    public IChatModel GetThreadGenerationModel()
    {
        var defaultModel = context.AiModels.FirstOrDefault(x => x.IsDefault);
        var type = _chatModels[defaultModel.Provider];
        return (serviceProvider.GetService(type) as IChatModel)!;
    }
}