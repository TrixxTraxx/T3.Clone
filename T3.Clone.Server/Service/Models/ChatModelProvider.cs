using T3.Clone.Server.Data;
using T3.Clone.Server.Service.Models;

namespace T3.Clone.Client.Services;

public class ChatModelProvider(IServiceProvider serviceProvider)
{
    private Dictionary<string, Type> _chatModels = new Dictionary<string, Type>()
    {
        { "OpenAi", typeof(OpenAiChat)}
    };
    
    public IChatModel GetChatModel(AiModel aiModel)
    {
        var type = _chatModels[aiModel.Provider];
        return (serviceProvider.GetService(type) as IChatModel)!;
    }
}