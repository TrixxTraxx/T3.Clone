using System.Net.Mail;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using T3.Clone.Dtos.Messages;
using T3.Clone.Server.Data;
using T3.Clone.Server.Mappers;

namespace T3.Clone.Server.Service;

public class AttachmentService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor 
)
{
    public AttachmentDto UploadAttachment(string fileName, string contentType, IFormFile file)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var attachment = new MessageAttachment()
        {
            UserId = userId,
            FileName = fileName,
            ContentType = contentType,
            Content = new AttachementContent
            {
                Data = new byte[file.Length]
            },
            CreatedAt = DateTime.UtcNow
        };
        
        using (var stream = file.OpenReadStream())
        {
            stream.ReadExactly(attachment.Content.Data, 0, (int)file.Length);
        }
        
        context.MessageAttachments.Add(attachment);
        context.SaveChanges();
        return AttachmentMapper.Map(attachment);
    }

    public AttachmentDto? GetAttachment(int id)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        
        var attachment = context.MessageAttachments
            .FirstOrDefault(a => a.Id == id && a.UserId == userId);
        if (attachment == null)
        {
            return null;
        }
        
        return AttachmentMapper.Map(attachment);
    }
    
    public (byte[] data, string fileName, string contentType) GetAttachmentContent(int id, bool bypassAuth = false)
    {
        MessageAttachment? attachment;
        if (bypassAuth)
        {
            attachment = context.MessageAttachments
                .Include(messageAttachment => messageAttachment.Content)
                .FirstOrDefault(a => a.Id == id);
        }
        else
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
        
            attachment = context.MessageAttachments
                .Include(messageAttachment => messageAttachment.Content)
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);
        }
        
        if (attachment == null)
        {
            throw new FileNotFoundException("Attachment not found.");
        }
        
        return (
            attachment.Content.Data,
            attachment.FileName,
            attachment.ContentType
        );
    }
}