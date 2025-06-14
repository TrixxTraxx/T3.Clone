using System.Security.Claims;
using LLMLab.Dtos.Messages;
using LLMLab.Server.Data;
using LLMLab.Server.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.Service;

public class MessageService(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext context,
    AiGenerationService aiService,
    ThreadService threadService
)
{
    public async Task<MessageDto> CreateMessageAsync(MessageDto dto)
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
            await context.SaveChangesAsync();
            // Generate a Thread Title based on the first message in the background
            await aiService.GenerateThreadTitle(thread.Id, newMessage.Text);
        }
        newMessage.Thread = thread;
        user.ThreadVersion++;
        thread.UpdatedAt = DateTime.UtcNow;
        thread.Version = user.ThreadVersion; // Increment thread version
        
        //add message to database
        context.Messages.Add(newMessage);
        await context.SaveChangesAsync();
        threadService.SendThreadUpdate(thread.UserId);
        
        await aiService.StartGeneration(newMessage.Id);
        
        //return the result
        return MessageMapper.Map(newMessage);
    }

    public async Task<MessageDto> GetMessageAsync(int messageId)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var message = await context.Messages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.Thread.UserId == userId);
        
        if (message == null)
        {
            throw new KeyNotFoundException($"Message with ID {messageId} not found.");
        }
        
        return MessageMapper.Map(message);
    }
}