using Microsoft.AspNetCore.Mvc;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class AttachementController (
    AttachmentService service
): ControllerBase
{
    
    [HttpPost("upload")]
    //limit to 1MB
    [RequestSizeLimit(1 * 1024 * 1024)] // 1 MB
    public IActionResult UploadAttachment([FromQuery] string fileName, [FromQuery] string contentType, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var result = service.UploadAttachment(fileName, contentType, file);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetAttachment(int id)
    {
        var attachment = service.GetAttachment(id);
        if (attachment == null)
        {
            return NotFound();
        }

        return Ok(attachment);
    }
    
    [HttpGet("content/{id}")]
    public IActionResult GetAttachmentContent(int id)
    {
        var content = service.GetAttachmentContent(id);

        return File(content.data, content.contentType, content.fileName);
    }
}