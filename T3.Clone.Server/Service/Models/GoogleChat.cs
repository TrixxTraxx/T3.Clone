using System.Buffers.Text;
using T3.Clone.Client.Services;
using T3.Clone.Server.Data;
using Mscc.GenerativeAI;
using T3.Clone.Dtos.Messages;
using DataMessage = T3.Clone.Server.Data.Message;

namespace T3.Clone.Server.Service.Models;

public class GoogleChat(
    ApplicationDbContext dbContext,
    AttachmentService attachmentService,
    AiKeyService keyService
) : IChatModel
{
    public async Task<ChatModelResponse> GenerateResponse(DataMessage entity, List<DataMessage> messagesChain, AiModel config,
        Action<string> tokenCallback, Action<string> thinkingTokenCallback, Action<string> errorCallback)
    {
        try
        {
            var apiKey = keyService.ResolveKey(entity.Thread.UserId, config.ApiKey);

            // Initialize Google AI client
            var googleAI = new GoogleAI(apiKey);
            var model = googleAI.GenerativeModel(config.ModelId);
            bool isReasoningEnabled = entity.ReasoningEffortLevel != ReasoningEffortLevel.None;
            var generationConfig = new GenerationConfig()
            {
                ThinkingConfig = new ThinkingConfig()
                {
                    IncludeThoughts = isReasoningEnabled,
                    ThinkingBudget = entity.ReasoningEffortLevel switch
                    {
                        ReasoningEffortLevel.None => 0,
                        ReasoningEffortLevel.Low => 2048,
                        ReasoningEffortLevel.Medium => 4096,
                        ReasoningEffortLevel.High => 8192
                    }
                }
            };

            // Build conversation content as simple text for now (due to API complexity)
            List<IPart> parts = new();
            
            // Add system prompt if available
            if (!string.IsNullOrEmpty(config.SystemPrompt))
            {
                parts.Add(new TextData()
                {
                    Text = "System Prompt:" + config.SystemPrompt
                });
            }

            // Process message chain (reverse to maintain conversation order)
            messagesChain.Reverse();
            
            foreach (var message in messagesChain)
            {
                // Add user message
                parts.Add(new TextData()
                {
                    Text = message.Text
                });

                // Add attachments context
                if (message.Attachments?.Any() == true)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var content = attachmentService.GetAttachmentContent(attachment.Id, true);
                        if (content.data != null && content.data.Length > 0)
                        {
                            parts.Add(new InlineData()
                            {
                                //encode as Base64
                                Data = Convert.ToBase64String(content.data),
                                MimeType = content.contentType
                            });
                        }
                    }
                }

                // Add assistant response if available and complete
                if (!string.IsNullOrEmpty(message.ModelResponse) && message.Complete)
                {
                    parts.Add(new TextData()
                    {
                        Text = message.ModelResponse
                    });
                }
            }

            // Generate response using text approach (will implement proper content parts later)
            await foreach (var chunk in model.GenerateContentStream(parts, generationConfig))
            {
                if (!string.IsNullOrEmpty(chunk.Text))
                {
                    bool finishedThinking = (!isReasoningEnabled) || chunk.UsageMetadata.CandidatesTokenCount != 0;
                    if (!finishedThinking)
                    {
                        entity.ThinkingResponse += chunk.Text;
                    
                        try
                        {
                            thinkingTokenCallback?.Invoke(chunk.Text);
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
                    else
                    {
                        entity.ModelResponse += chunk.Text;
                    
                        try
                        {
                            tokenCallback?.Invoke(chunk.Text);
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
                }
            }

            return new ChatModelResponse
            {
                InputTokens = 0, // TODO: Get actual token usage from response if available
                OutputTokens = 0, // TODO: Get actual token usage from response if available
                Response = entity.ModelResponse,
                IsError = false,
                ModelName = config.Name,
                ModelVersion = config.ModelId,
                ModelProvider = "Google",
                ModelId = config.ModelId
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Google Chat generation: {ex.Message}");
            errorCallback?.Invoke($"Error during Google Chat generation: {ex.Message}");
            
            return new ChatModelResponse
            {
                IsError = true,
                ErrorMessage = ex.Message,
                Response = string.Empty,
                InputTokens = 0,
                OutputTokens = 0,
                ModelProvider = "Google",
                ModelId = config.ModelId,
                ModelName = config.Name
            };
        }
    }
} 