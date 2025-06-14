using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using T3.Clone.Client.Services;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;

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

                if (message.Attachments?.Any() == true)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var content = attachmentService.GetAttachmentContent(attachment.Id, true);
                        if (content.data != null && content.data.Length > 0)
                        {
                            userContent.Add(new ImageContent()
                            {
                                Source = new ImageSource()
                                {
                                    Type = SourceType.base64,
                                    Data = Convert.ToBase64String(content.data),
                                    MediaType = content.contentType,
                                },
                            });
                        }
                    }
                }

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

            // Create message parameters for reasoning models (Claude 3.7 Sonnet or Claude 4 Sonnet)
            var parameters = new MessageParameters()
            {
                Messages = messages,
                MaxTokens = config.MaxOutputTokens > 0 ? config.MaxOutputTokens : 8000,
                Model = config.ModelId, // Should be claude-3-7-sonnet-20250101 or similar for thinking
                Stream = true,
            };
            
            var isThinkingContent = entity.ReasoningEffortLevel != ReasoningEffortLevel.None;

            if (isThinkingContent)
            {
                parameters.Thinking = new ThinkingParameters()
                {
                    BudgetTokens = entity.ReasoningEffortLevel switch
                    {
                        ReasoningEffortLevel.Low => 2048,
                        ReasoningEffortLevel.Medium => 4096,
                        ReasoningEffortLevel.High => 8192,
                    }
                };
            }

            Console.WriteLine($"[Anthropic Reasoning] Starting Reasoning Chat with model: {config.ModelId}");

            // Generate streaming response
            var response = client.Messages.StreamClaudeMessageAsync(parameters);

            var inputTokens = 0;
            var outputTokens = 0;
            
            await foreach (var res in response)
            {
                Console.WriteLine($"[Anthropic Reasoning] Update: {res.Type}");
                
                // Handle thinking content vs regular content
                // For Claude 3.7 Sonnet and Claude 4 with thinking, there will be different content blocks
                if (res.Type == "content_block_start")
                {
                    // Check if this is a thinking block or regular text block
                    isThinkingContent = res.ContentBlock?.Type == "thinking";
                    Console.WriteLine($"[Anthropic Reasoning] Content block start - Thinking: {isThinkingContent}");
                }
                else if (res.Delta?.Thinking != null)
                {
                    if (!isThinkingContent)
                    {
                        Console.WriteLine($"[Anthropic Reasoning] Warning: Received thinking content outside of a thinking block. This may indicate an issue with the model response structure.");
                    }
                    // This is thinking/reasoning content
                    entity.ThinkingResponse += res.Delta.Thinking;
                    thinkingTokenCallback?.Invoke(res.Delta.Thinking);
                    Console.WriteLine(
                        $"[Anthropic Reasoning] Thinking: {res.Delta.Thinking}...");
                }
                else if (res.Delta?.Text != null)
                {
                    if (isThinkingContent)
                    {
                        Console.WriteLine($"[Anthropic Reasoning] Warning: Received regular text content while in a thinking block. This may indicate an issue with the model response structure.");
                    }
                    // This is regular response content
                    entity.ModelResponse += res.Delta.Text;
                    tokenCallback?.Invoke(res.Delta.Text);
                }
                else if (res.Type == "content_block_stop")
                {
                    Console.WriteLine($"[Anthropic Reasoning] Content block stop - Was thinking: {isThinkingContent}");
                    isThinkingContent = false;
                }

                // Track token usage if available
                if (res.Usage != null)
                {
                    inputTokens = res.Usage.InputTokens;
                    outputTokens = res.Usage.OutputTokens;
                }
            }

            Console.WriteLine($"[Anthropic Reasoning] Completed. Input tokens: {inputTokens}, Output tokens: {outputTokens}");
            Console.WriteLine($"[Anthropic Reasoning] Thinking response length: {entity.ThinkingResponse?.Length ?? 0}");
            Console.WriteLine($"[Anthropic Reasoning] Final response length: {entity.ModelResponse?.Length ?? 0}");

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
                ModelId = config.ModelId,
                ModelName = config.Name
            };
        }
    }
} 