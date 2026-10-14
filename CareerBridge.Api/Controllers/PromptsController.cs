using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/prompts")]
public class PromptsController : ControllerBase
{
    [HttpGet("stage/{stage}")]
    public IActionResult GetPromptsForStage(TransitionStage stage) =>
        Ok(PromptCatalog.GetPrompts(stage));
}
