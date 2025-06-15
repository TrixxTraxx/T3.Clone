using LLMLab.Server.Data;

namespace LLMLab.Server.Service.Models;

public interface IChatModel
{
    public Task<ChatModelResponse> GenerateResponse(Message entity, List<Message> messagesChain, AiModel config,
        Action<string> tokenCallback, Action<string> thinkingTokenCallback, Action<string> errorCallback);
}