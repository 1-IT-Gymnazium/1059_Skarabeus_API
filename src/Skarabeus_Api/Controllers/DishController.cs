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
using Skarabeus_Data.Entities.ConnectionTables;
using Skarabeus_Data.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Skarabeus_Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class DishController : ControllerBase
{
    private readonly ILogger<IngredientController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public DishController(
        ILogger<IngredientController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    // GET: api/<ValuesController>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DishDetailModel>>> GetList()
    {
        var list = await _dbContext.Set<Dish>()
            .Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FilterDeleted()
            .Select(x => new DishDetailModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ingredients = (ICollection<IngredientDishDetailModel>)x.Ingredients.Select(y => new IngredientDishDetailModel()
                {
                    Id = y.Id,
                    IngredientId = y.IngredientId,
                    Name = y.Ingredient.Name,
                    PriceForUnit = y.Ingredient.PriceForUnit,
                    Amount = y.AmountInBaseUnits,
                    Price = y.AmountInBaseUnits * y.Ingredient.PriceForUnit,
                })

            }).ToArrayAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] DishCreateModel dishmodel
        )
    {
        var now = _clock.GetCurrentInstant();

        var newDish = new Dish()
        {
            Name = dishmodel.Name,
            Description = dishmodel.Description,
        };


        if (User.Identity != null && User.Identity.IsAuthenticated) newDish.SetCreateBy(User.GetName(), now);
        else newDish.SetCreateBySystem(now);
        


        var uniqueCheck = await _dbContext.Dishes.FilterDeleted().AnyAsync(x => x.Name == dishmodel.Name);

        if (uniqueCheck)
        {
            ModelState.AddModelError<DishCreateModel>(x => x.Name, "name is not unique");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _dbContext.Add(newDish);
        await _dbContext.SaveChangesAsync();

        return Ok(newDish);
    }

    [HttpPost("AddIngredientToDish")]
    public async Task<ActionResult> AddIngredientToDish(
        [FromBody] DishAddIngredientModel addModel
        )
    {
        var now = _clock.GetCurrentInstant();

        var dish = await _dbContext.Dishes.FirstOrDefaultAsync(x => x.Id == addModel.DishId);
        var ingredient = await _dbContext.Ingredients.FirstOrDefaultAsync(x => x.Id == addModel.IngredientId);

        if ((dish == null) && (ingredient == null))
        {
            ModelState.AddModelError(string.Empty, "Dish or Ingredient does not exist");
            return ValidationProblem(ModelState);
        }

        var newIngredientDish = new IngredientDish()
        {
            Dish = dish,
            Ingredient = ingredient,
            AmountInBaseUnits = addModel.Amount
        };

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            newIngredientDish.SetCreateBy(User.GetName(), now);
        }
        else
        {
            newIngredientDish.SetCreateBySystem(now);
        }


        _dbContext.IngredientDishes.Add(newIngredientDish);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    [HttpPatch("UpdateIngredientDish/{id}")]
    public async Task<ActionResult> UpdateIngredientDish(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<DishAddIngredientModel> patch
        )
    {
        var ingredientDish = await _dbContext.IngredientDishes
            .FirstOrDefaultAsync(x => x.Id == id);
        

        if (ingredientDish == null)
        {
            return NotFound();
        }

        var now = _clock.GetCurrentInstant();

        var toUpdate = new DishAddIngredientModel
        {
            Amount = ingredientDish.AmountInBaseUnits,
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        ingredientDish.AmountInBaseUnits = toUpdate.Amount;

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            ingredientDish.SetModifyBy(User.GetName(), now);
        }
        else
        {
            ingredientDish.SetModifyBySystem(now);
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("RemoveIngredientFromDish")]
    public async Task<ActionResult> RemoveIngredientFromDish(
        [FromBody] DishIngredientModel removeModel
        )
    {
        var now = _clock.GetCurrentInstant();

        var ingredientDish = _dbContext.IngredientDishes
            .FirstOrDefault(x => 
            x.IngredientId == removeModel.IngredientId 
            && 
            x.DishId == removeModel.DishId);

        if(ingredientDish == null)
        {
            ModelState.AddModelError(string.Empty, "ingredient-dish not found");
            return ValidationProblem(ModelState);
        }


        _dbContext.IngredientDishes.Remove(ingredientDish);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    // GET api/<ValuesController>/5
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var dish = _dbContext.Dishes
            .Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .Where(x => x.Id == id)
            .Select(x => new DishDetailModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ingredients = (ICollection<IngredientDishDetailModel>)x.Ingredients.Select(y => new IngredientDishDetailModel()
                {
                    Id = y.Id,
                    IngredientId = y.IngredientId,
                    Name = y.Ingredient.Name,
                    PriceForUnit = y.Ingredient.PriceForUnit,
                    Amount = y.AmountInBaseUnits,
                    Price = y.AmountInBaseUnits * y.Ingredient.PriceForUnit,
                })
            });

        if (dish == null)
        {
            return NotFound();
        }
        return Ok(dish);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var dish = _dbContext.Dishes
            .Include(x => x.Ingredients)
            .FirstOrDefault(x => x.Id == id);
        if(dish == null)
        {
            return NotFound();
        }

        _dbContext.Dishes.Remove(dish);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<DishCreateModel> patch
        )
    {
        var dish = await _dbContext.Dishes.FirstOrDefaultAsync(x=>x.Id == id);


        if (dish == null)
        {
            return NotFound();
        }

        var now = _clock.GetCurrentInstant();

        var toUpdate = new DishCreateModel
        {
            Description = dish.Description,
            Name = dish.Name,
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        dish.Name = toUpdate.Name;
        dish.Description = toUpdate.Description;


        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            dish.SetModifyBy(User.GetName(), now);
        }
        else
        {
            dish.SetModifyBySystem(now);
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
