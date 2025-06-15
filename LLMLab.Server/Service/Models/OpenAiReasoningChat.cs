using System.ClientModel;
using LLMLab.Dtos.Messages;
using LLMLab.Server.Data;
using OpenAI;
using OpenAI.Responses;

namespace LLMLab.Server.Service.Models;

public class OpenAiReasoningChat(
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
            if (entity.ReasoningEffortLevel == ReasoningEffortLevel.None)
            {
                return await new OpenAiChat(dbContext, attachmentService, keyService)
                    .GenerateResponse(entity, messagesChain, config, tokenCallback, thinkingTokenCallback,
                        errorCallback);
            }

            var apiKey = keyService.ResolveKey(entity.Thread.UserId, config.ApiKey);

            OpenAIResponseClient client = new(model: config.ModelId, new ApiKeyCredential(apiKey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri(config.ApiUrl)
                });

            //create the chat messages
            List<ResponseItem> messages = new();
            if (!string.IsNullOrEmpty(config.SystemPrompt))
            {
                messages.Add(ResponseItem.CreateSystemMessageItem(config.SystemPrompt));
            }

            //Reverse the messages chain to maintain the order of conversation
            messagesChain.Reverse();
            foreach (var message in messagesChain)
            {
                var userContentParts = new ResponseContentPart[]
                {
                    ResponseContentPart.CreateInputTextPart(message.Text)
                };
                foreach (var attachment in message.Attachments ?? new())
                {
                    var content = attachmentService.GetAttachmentContent(attachment.Id, true);
                    if (content.data != null && content.data.Length > 0)
                    {
                        userContentParts = userContentParts.Append(ResponseContentPart.CreateInputImagePart(
                            BinaryData.FromBytes(content.data),
                            content.contentType
                        )).ToArray();
                    }
                }

                messages.Add(ResponseItem.CreateUserMessageItem(userContentParts));

                if (!string.IsNullOrEmpty(message.ModelResponse) && message.Complete)
                {
                    if (!string.IsNullOrEmpty(message.ThinkingResponse))
                    {
                        // Add thinking response if available
                        messages.Add(ResponseItem.CreateReasoningItem(new[] { message.ThinkingResponse }));
                    }

                    messages.Add(ResponseItem.CreateAssistantMessageItem(message.Text));
                }
            }

            //generate streaming response
            var response = client.CreateResponseStreamingAsync(messages, new ResponseCreationOptions()
            {
                //TODO: set other options like temperature, max tokens, etc.
                ReasoningOptions = new ResponseReasoningOptions()
                {
                    ReasoningEffortLevel = entity.ReasoningEffortLevel switch
                    {
                        ReasoningEffortLevel.Low => ResponseReasoningEffortLevel.Low,
                        ReasoningEffortLevel.Medium => ResponseReasoningEffortLevel.Medium,
                        ReasoningEffortLevel.High => ResponseReasoningEffortLevel.High,
                        _ => ResponseReasoningEffortLevel.High
                    }
                }
            });

            //process the response
            await foreach (var update in response)
            {
                if (update is StreamingResponseInProgressUpdate inProgressUpdate)
                {
                    //Console.WriteLine($"[Reasoning] In Progress: {inProgressUpdate.Response.OutputItems}");
                }
                if(update is StreamingResponseOutputItemAddedUpdate itemAddedUpdate)
                {
                    try
                    {
                        if(itemAddedUpdate.Item is ReasoningResponseItem reasoningItem)
                        {
                            //Console.WriteLine($"[Reasoning] ({JsonSerializer.Serialize(reasoningItem.SummaryTextParts)})");
                            foreach (var textPart in reasoningItem.SummaryTextParts)
                            {
                                entity.ThinkingResponse += textPart;
                                thinkingTokenCallback?.Invoke(textPart);
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"[Reasoning] Unknown item type: {itemAddedUpdate.Item.GetType().Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            //handle error in token callback
                            errorCallback?.Invoke($"Error in token callback: {ex.Message}");
                            //Console.WriteLine($"Error in token callback: {ex.Message}");
                        }
                        catch (Exception innerEx)
                        {
                            //log the error if the error callback fails
                            //Console.WriteLine($"Error in error callback: {innerEx.Message}");
                        }
                    }
                }
                if (update is StreamingResponseOutputTextDeltaUpdate deltaUpdate)
                {
                    try
                    {
                        entity.ModelResponse += deltaUpdate.Delta;
                        tokenCallback?.Invoke(deltaUpdate.Delta);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            //handle error in token callback
                            //errorCallback?.Invoke($"Error in token callback: {ex.Message}");
                        }
                        catch (Exception innerEx)
                        {
                            //log the error if the error callback fails
                            //Console.WriteLine($"Error in error callback: {innerEx.Message}");
                        }
                    }
                }
            }

            //finalize the response
            return new ChatModelResponse()
            {
                InputTokens = 0, // TODO: Get actual token usage from response
                OutputTokens = 0, // TODO: Get actual token usage from response
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
            Console.WriteLine($"Error during OpenAI Reasoning Chat generation: {ex.Message}");
            errorCallback?.Invoke($"Error during OpenAI Reasoning Chat generation: {ex.Message}");
            
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