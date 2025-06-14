using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Service.Models;

public class OpenAiChat(
    ApplicationDbContext dbContext,
    AttachmentService attachmentService,
    AiKeyService keyService
) : IChatModel
{
    public async Task<ChatModelResponse> GenerateResponse(Message entity, List<Message> messagesChain, AiModel config,
        Action<string> tokenCallback, Action<string> thinkingTokenCallback, Action<string> errorCallback)
    {
        try
        {
            var apiKey = keyService.ResolveKey(entity.Thread.UserId, config.ApiKey);

            ChatClient client = new(model: config.ModelId, new ApiKeyCredential(apiKey), new OpenAIClientOptions()
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
                foreach (var attachment in message.Attachments ?? new())
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

            var inputTokens = 0;
            var outputTokens = 0;

            //process the response
            await foreach (StreamingChatCompletionUpdate? update in response)
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

                // Track token usage if available
                if (update.Usage != null)
                {
                    inputTokens = update.Usage.InputTokenCount;
                    outputTokens = update.Usage.OutputTokenCount;
                }
            }

            //finalize the response
            return new ChatModelResponse()
            {
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                Response = entity.ModelResponse,
                IsError = false,
                ModelName = config.Name,
                ModelVersion = config.ModelId,
                ModelProvider = "OpenAI",
                ModelId = config.ModelId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OpenAI Chat generation: {ex.Message}");
            errorCallback?.Invoke($"Error during OpenAI Chat generation: {ex.Message}");
            
            return new ChatModelResponse
            {
                IsError = true,
                ErrorMessage = ex.Message,
                ModelProvider = "OpenAI",
                ModelId = config.ModelId,
                ModelName = config.Name
            };
        }
    }
}