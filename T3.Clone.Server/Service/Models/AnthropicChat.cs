using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;
using DataMessage = T3.Clone.Server.Data.Message;
using AnthropicMessage = Anthropic.SDK.Messaging.Message;

namespace T3.Clone.Server.Service.Models;

public class AnthropicChat(
    ApplicationDbContext dbContext,
    AttachmentService attachmentService,
    AiKeyService keyService
) : IChatModel
{
    public async Task<ChatModelResponse> GenerateResponse(T3.Clone.Server.Data.Message entity, List<T3.Clone.Server.Data.Message> messagesChain, AiModel config,
        Action<string> tokenCallback, Action<string> thinkingTokenCallback, Action<string> errorCallback)
    {
        try
        {
            var apiKey = keyService.ResolveKey(entity.Thread.UserId, config.ApiKey);

            var client = new AnthropicClient(apiKey);

            // Create the message list
            var messages = new List<Anthropic.SDK.Messaging.Message>();

            // Reverse the messages chain to maintain the order of conversation
            messagesChain.Reverse();
            
            foreach (var message in messagesChain)
            {
                var userContent = new List<ContentBase>
                {
                    new TextContent()
                    {
                        Text = message.Text
                    }
                };

                // Add basic support for images (simplified for now)
                // TODO: Add proper image attachment support

                messages.Add(new Anthropic.SDK.Messaging.Message()
                {
                    Role = RoleType.User,
                    Content = userContent
                });

                if (!string.IsNullOrEmpty(message.ModelResponse) && message.Complete)
                {
                    messages.Add(new Anthropic.SDK.Messaging.Message()
                    {
                        Role = RoleType.Assistant,
                        Content = new List<ContentBase>()
                        {
                            new TextContent()
                            {
                                Text = message.ModelResponse
                            }
                        }
                    });
                }
            }

            // Create message parameters
            var parameters = new MessageParameters()
            {
                Messages = messages,
                MaxTokens = config.MaxOutputTokens > 0 ? config.MaxOutputTokens : 4000,
                Model = config.ModelId,
                Stream = true
            };

            // Generate streaming response
            var response = client.Messages.StreamClaudeMessageAsync(parameters);

            var inputTokens = 0;
            var outputTokens = 0;

            await foreach (var res in response)
            {
                // Handle different response types based on the streaming response
                if (res.Delta?.Text != null)
                {
                    try
                    {
                        entity.ModelResponse += res.Delta.Text;
                        tokenCallback?.Invoke(res.Delta.Text);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            errorCallback?.Invoke($"Error in token callback: {ex.Message}");
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"Error in error callback: {innerEx.Message}");
                        }
                    }
                }

                // Track token usage if available
                if (res.Usage != null)
                {
                    inputTokens = res.Usage.InputTokens;
                    outputTokens = res.Usage.OutputTokens;
                }
            }

            return new ChatModelResponse
            {
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                Response = entity.ModelResponse,
                IsError = false,
                ModelName = config.Name,
                ModelVersion = config.ModelId,
                ModelProvider = "Anthropic",
                ModelId = config.ModelId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Anthropic Chat generation: {ex.Message}");
            errorCallback?.Invoke($"Error during Anthropic Chat generation: {ex.Message}");
            
            return new ChatModelResponse
            {
                IsError = true,
                ErrorMessage = ex.Message,
                ModelProvider = "Anthropic",
                ModelId = config.ModelId
            };
        }
    }
} 