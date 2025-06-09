using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Service.Models;

public class OpenAiChat(
    AttachmentService attachmentService
) : IChatModel
{
    public async Task<ChatModelResponse> GenerateResponse(Message entity, List<Message> messagesChain, AiModel config, Action<string> tokenCallback, Action<string> errorCallback)
    {
        ChatClient client = new(model: config.ModelId, new ApiKeyCredential(config.ApiKey), new OpenAIClientOptions()
        {
            Endpoint = new Uri(config.ApiUrl)
        });
        
        //create the chat messages
        List<ChatMessage> messages = new();
        if (!string.IsNullOrEmpty(config.SystemPrompt))
        {
            messages.Add(new SystemChatMessage(config.SystemPrompt));
        }
        
        //Reverse the messages chain to maintain the order of conversation
        messagesChain.Reverse();
        foreach (var message in messagesChain)
        {
            var userContentParts = new ChatMessageContentPart[]
            {
                ChatMessageContentPart.CreateTextPart(message.Text)
            };
            foreach (var attachment in message.Attachments ?? new ())
            {
                var content = attachmentService.GetAttachmentContent(attachment.Id, true);
                if (content.data != null && content.data.Length > 0)
                {
                    userContentParts = userContentParts.Append(ChatMessageContentPart.CreateImagePart(
                        BinaryData.FromBytes(content.data),
                        content.contentType
                    )).ToArray();
                }
            }
            messages.Add(new UserChatMessage(userContentParts));
            
            if (!string.IsNullOrEmpty(message.ModelResponse) && message.Complete)
            {
                var assistantContentParts = new ChatMessageContentPart[]
                {
                    ChatMessageContentPart.CreateTextPart(message.ModelResponse)
                };
                messages.Add(new AssistantChatMessage(assistantContentParts));
            }
        }
        
        //generate streaming response
        var response = client.CompleteChatStreamingAsync(messages, new ChatCompletionOptions()
        {
            //TODO: set other options like temperature, max tokens, etc.
        }, CancellationToken.None);
        
        //process the response
        await foreach (var update in response)
        {
            if (update.ContentUpdate.Count > 0)
            {
                try
                {
                    entity.ModelResponse += update.ContentUpdate[0].Text;
                    tokenCallback?.Invoke(update.ContentUpdate[0].Text);
                }
                catch (Exception ex)
                {
                    try
                    {
                        //handle error in token callback
                        errorCallback?.Invoke($"Error in token callback: {ex.Message}");
                    }
                    catch (Exception innerEx)
                    {
                        //log the error if the error callback fails
                        Console.WriteLine($"Error in error callback: {innerEx.Message}");
                    }
                }
            }
        }
        
        //finalize the response
        return new ChatModelResponse()
        {

        };
    }
}