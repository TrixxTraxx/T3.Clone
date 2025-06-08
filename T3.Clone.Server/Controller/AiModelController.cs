using Microsoft.AspNetCore.Mvc;
using T3.Clone.Server.Service;

namespace T3.Clone.Server.Controller;

[Route("api/[controller]")]
public class AiModelController(
    AiModelService service
) : ControllerBase
{
    [HttpGet]
    public IActionResult GetModels()
    {
        return Ok(service.GetModels());
    }
}