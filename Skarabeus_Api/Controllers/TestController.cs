using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Skarabeus_Data;

namespace Skarabeus_Api.Controllers;
[Controller]
public class TestController
{

    private readonly ILogger<TestController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public TestController(
        ILogger<TestController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;   
        _dbContext = dbContext;
    }

    [HttpGet("api/v1/Project/{id}")]
    public async Task<ActionResult<int>> Get(
        [FromRoute] int id
        )
    {
        return id;
    }
}
