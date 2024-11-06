using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Skarabeus_Api.Controllers.Models.Ingredient;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Skarabeus_Api.Controllers;

[Controller]
[Route("api/v1/ingredient")]
public class IngredientController : Controller
{
    private readonly ILogger<IngredientController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public IngredientController(
        ILogger<IngredientController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<IngredientCreateModel>> Create( 
        [FromBody] IngredientCreateModel model
        )
    {

        var now = _clock.GetCurrentInstant();

        var newIngredient = new Ingredient
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            PriceForUnit = model.PriceForUnit,
        }.SetCreateBySystem(now);

        var uniqueCheck = await _dbContext.Set<Ingredient>().AnyAsync(x => x.Name == newIngredient.Name);

        if (uniqueCheck)
        {
            ModelState.AddModelError<IngredientCreateModel>(x => x.Name, "title is not unique");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _dbContext.Add(newIngredient);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<IngredientDetailModel>>> GetList()
    {
        var list = await _dbContext.Set<Ingredient>().Select(x => new IngredientDetailModel
        {
            Name = x.Name,
            PriceForUnit = x.PriceForUnit,
        }).ToArrayAsync();
        return Ok(list);
    }

}
