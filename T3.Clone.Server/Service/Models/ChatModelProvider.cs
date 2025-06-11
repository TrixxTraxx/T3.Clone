using T3.Clone.Server.Data;
using T3.Clone.Server.Service.Models;

namespace T3.Clone.Client.Services;

public class ChatModelProvider(
    IServiceProvider serviceProvider,
    ApplicationDbContext context
)
{
    private Dictionary<string, Type> _chatModels = new Dictionary<string, Type>()
    {
        { "OpenAi", typeof(OpenAiChat)},
        { "OpenAiReasoning", typeof(OpenAiReasoningChat)}
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