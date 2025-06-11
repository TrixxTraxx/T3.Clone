using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using T3.Clone.Client.Services;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;

namespace T3.Clone.Server.Service.Models;

public class OpenAiReasoningChat(
    ApplicationDbContext dbContext,
    AttachmentService attachmentService
) : IChatModel
{
        public async Task<ChatModelResponse> GenerateResponse(Message entity, List<Message> messagesChain, AiModel config,
        Action<string> tokenCallback, Action<string> thinkingTokenCallback, Action<string> errorCallback)
    {
        if (false)//(entity.ReasoningEffort == ReasoningEffortLevel.None)
        {
            return await new OpenAiChat(dbContext, attachmentService)
                .GenerateResponse(entity, messagesChain, config, tokenCallback, thinkingTokenCallback, errorCallback);
        }
        var keyOverride = dbContext.AiModelKeys
            .Where(x => x.UserId == entity.Thread.UserId)
            .ToList();
        var apiKey = config.ApiKey;
        if (keyOverride.Count > 0)
        {
            foreach (var key in keyOverride)
            {
                apiKey = apiKey.Replace("{" + key.Identifier + "}", key.ApiKey);   
            }
        }

        OpenAIResponseClient client = new(model: config.ModelId, new ApiKeyCredential(apiKey), new OpenAIClientOptions()
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
                    messages.Add(ResponseItem.CreateReasoningItem(new []{message.ThinkingResponse}));
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
                ReasoningEffortLevel = entity.ReasoningEffort switch{
                    ReasoningEffortLevel.Low => ResponseReasoningEffortLevel.Low,
                    ReasoningEffortLevel.Medium => ResponseReasoningEffortLevel.Medium,
                    ReasoningEffortLevel.High => ResponseReasoningEffortLevel.High,
                    _ => ResponseReasoningEffortLevel.Low
                }
            }
        }, CancellationToken.None);
        
        Console.WriteLine($"[Reasoning] Starting Reasoning Chat!");

        //process the response
        await foreach (var update in response)
        {
            Console.WriteLine($"[Reasoning] Update: {update.GetType().Name}");
            /*if (update is StreamingResponseUpdate itemUpdate
                && itemUpdate.Item is ReasoningResponseItem reasoningItem)
            {
                Console.WriteLine($"[Reasoning] ({reasoningItem.Status})");
            }*/
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