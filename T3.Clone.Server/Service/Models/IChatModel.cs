using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;

namespace T3.Clone.Client.Services;

public interface IChatModel
{
    public Task<ChatModelResponse> GenerateResponse(Message entity, List<Message> messagesChain, AiModel config, Action<string> tokenCallback, Action<string> errorCallback);
}