using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Service;

public class MessageService(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext context,
    AiGenerationService aiService
)
{
    public async Task<MessageCreationResult> CreateMessageAsync(MessageDto dto)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var user = await context.Users
            .FirstAsync(u => u.Id == userId);
        
        //create the message
        var newMessage = MessageMapper.Map(dto);
        
        //set some initial properties
        newMessage.Complete = false;
        newMessage.CreatedAt = DateTime.UtcNow;
        newMessage.ModelResponse = string.Empty; // Initialize with empty response
        
        //add attachments if any
        if (dto.AttachmentIds.Any())
        {
            var attachments = context.MessageAttachments
                .Where(a => dto.AttachmentIds.Contains(a.Id));
            foreach (var attachment in attachments)
            {
                if (attachment.MessageId != null)
                {
                    throw new BadHttpRequestException("Attachment already linked to another message.");
                }
                attachment.Message = newMessage;
            }
        }
        
        //create new thread if it doesn't exist
        var thread = await context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && t.UserId == userId);
        if (thread == null)
        {
            thread = new MessageThread
            {
                UserId = userId,
                Title = "New Thread" // Default title, can be changed later
            };
            context.MessageThreads.Add(thread);
            newMessage.Thread = thread; // Associate the new message with the new thread
            
            // Generate a Thread Title based on the first message in the background
            _ = Task.Run(async () => {
                try
                {
                    await aiService.GenerateThreadTitle(thread.Id, newMessage.Text);
                }
                catch (Exception ex)
                {
                    // Handle exceptions from AI generation
                    Console.WriteLine($"AI Thread Title generation failed: {ex.Message}");
                }
            });
        }
        user.ThreadVersion++;
        thread.Version = user.ThreadVersion; // Increment thread version
        
        //add message to database
        context.Messages.Add(newMessage);
        await context.SaveChangesAsync();
        
        //Start the Generation in the Background
        _ = Task.Run((Action) (async () => {
            try
            {
                await aiService.StartGeneration(newMessage.Id);
            }
            catch (Exception ex)
            {
                // Handle exceptions from AI generation
                Console.WriteLine($"AI Generation failed: {ex.Message}");
            }
        }));
        
        //return the result
        return new MessageCreationResult
        {
            Id = newMessage.Id
        };
    }

    public async Task<MessageDto> GetMessageAsync(int messageId)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.Thread.UserId == userId);
        
        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {messageId} not found.");
        }
        
        return MessageMapper.Map(message);
    }
}