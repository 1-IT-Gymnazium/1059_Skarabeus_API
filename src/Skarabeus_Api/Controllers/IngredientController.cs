using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Skarabeus_Api.Controllers.Models.DishModels;
using Skarabeus_Api.Controllers.Models.IngredientModels;
using Skarabeus_Api.Utils;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;

namespace Skarabeus_Api.Controllers;

[Controller]
[Route("api/v1/ingredient")]
//[Authorize]
public class IngredientController : ControllerBase
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
        };


        if (User.Identity != null && User.Identity.IsAuthenticated) newIngredient.SetCreateBy(User.GetName(), now);
        else newIngredient.SetCreateBySystem(now);

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
    public async Task<ActionResult> GetList(
        )
    {
        var list = await _dbContext.Set<Ingredient>().FilterDeleted().Select(x => x.ToDetail()).ToArrayAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var ingredient = await _dbContext.Set<Ingredient>().FirstOrDefaultAsync(x => x.Id == id);

        if (ingredient == null) return NotFound();

        return Ok(ingredient);
    }


    [HttpPatch("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<IngredientCreateModel> patch
        )
    {
        var ingredient = await _dbContext.Ingredients.FirstOrDefaultAsync(x => x.Id == id);


        if (ingredient == null)
        {
            return NotFound();
        }

        var now = _clock.GetCurrentInstant();

        var toUpdate = new IngredientCreateModel
        {
            PriceForUnit = ingredient.PriceForUnit,
            Name = ingredient.Name,
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        ingredient.Name = toUpdate.Name;
        ingredient.PriceForUnit = toUpdate.PriceForUnit;


        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            ingredient.SetModifyBy(User.GetName(), now);
        }
        else
        {
            ingredient.SetModifyBySystem(now);
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {

        var ingredient = await _dbContext.Set<Ingredient>().FirstOrDefaultAsync(x => x.Id == id);

        if (ingredient == null) return NotFound();

        _dbContext.Ingredients.Remove(ingredient);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

}
