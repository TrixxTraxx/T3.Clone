using LLMLab.Server.Service;
using Microsoft.AspNetCore.Mvc;

namespace LLMLab.Server.Controller;

[Route("api/[controller]")]
public class AiModelsController(
    AiModelService service
) : ControllerBase
{
    [HttpGet]
    public IActionResult GetModels()
    {
        return Ok(service.GetModels());
    }
}