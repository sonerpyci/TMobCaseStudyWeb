using Microsoft.AspNetCore.Mvc;

namespace TMobCaseStudy.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class SpaceCraftController : ControllerBase
{
    private readonly ILogger<SpaceCraftController> _logger;

    public SpaceCraftController(ILogger<SpaceCraftController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "hello")]
    public IActionResult Get()
    {
        return Ok(
            new { Success = true, Message = "Hello Mars!"}
        );

    }
}