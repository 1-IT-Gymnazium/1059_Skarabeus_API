using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Api.Controllers.Models.EventModels;
using Skarabeus_Api.Controllers.Models.DishModels;
using Skarabeus_Api.Controllers.Models.IngredientModels;
using Skarabeus_Data.Entities;
using Skarabeus_Data;
using Skarabeus_Data.Interfaces;
using Skarabeus_Api.Utils;
using NodaTime;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Web.Helpers;
using Skarabeus_Data.Entities.ConnectionTables;

namespace Skarabeus_Api.Controllers;

[Route("api/v1/Administration")]
[ApiController]
public class AdministrationController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IClock _clock;
    public AdministrationController(
            ApplicationDbContext dbContext,
            IClock clock
        )
    {
        _clock = clock;
        _dbContext = dbContext;
    }
    [HttpPost("Upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var now = _clock.GetCurrentInstant();
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (!file.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid file type. Only JSON files are allowed.");

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        string jsonContent = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        // Extract individual sections without deserializing the whole file
        var persons = root.TryGetProperty("persons", out var personsElement) ? personsElement : default;
        var events = root.TryGetProperty("events", out var eventsElement) ? eventsElement : default;
        var dishes = root.TryGetProperty("dishes", out var dishesElement) ? dishesElement : default;
        var ingredients = root.TryGetProperty("ingredients", out var ingredientsElement) ? ingredientsElement : default;
        var dishIngredients = root.TryGetProperty("ingredientDishes", out var ingredientDishesElement) ? ingredientDishesElement : default;

        // Deserialize only the needed sections
        var personList = persons.ValueKind != JsonValueKind.Undefined
            ? JsonSerializer.Deserialize<List<PersonCreateModel>>(persons.GetRawText())
            : new List<PersonCreateModel>();

        var eventList = events.ValueKind != JsonValueKind.Undefined
            ? JsonSerializer.Deserialize<List<EventCreateModel>>(events.GetRawText())
            : new List<EventCreateModel>();

        var dishList = dishes.ValueKind != JsonValueKind.Undefined
            ? JsonSerializer.Deserialize<List<DishCreateModel>>(dishes.GetRawText())
            : new List<DishCreateModel>();

        var ingredientList = ingredients.ValueKind != JsonValueKind.Undefined
            ? JsonSerializer.Deserialize<List<IngredientCreateModel>>(ingredients.GetRawText())
            : new List<IngredientCreateModel>();

        var dishIngridientList = dishIngredients.ValueKind != JsonValueKind.Undefined
            ? JsonSerializer.Deserialize<List<UploadDishAddIngredients>>(dishIngredients.GetRawText())
            : new List<UploadDishAddIngredients>();

        foreach (var ing in ingredientList)
        {

            var newIngredient = new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = ing.Name,
                PriceForUnit = ing.PriceForUnit,
            }
            .SetCreateBy(User.GetName(), now);

            if (!newIngredient.IsDatabaseValid())
            {
                ModelState.AddModelError("Ingredient", $"This ingredient is not valid: {newIngredient.Name}");
            }
            else
            {
                _dbContext.Add(newIngredient);
            }
        }

        foreach (var dish in dishList)
        {

            var newDish = new Dish
            {
                Id = Guid.NewGuid(),
                Name = dish.Name,
                Description = dish.Description
            }
            .SetCreateBy(User.GetName(), now);

            if (!newDish.IsDatabaseValid())
            {
                ModelState.AddModelError("Dish", $"This ingredient is not valid: {newDish.Name}");
            }
            else
            {
                _dbContext.Add(newDish);
            }
        }


        foreach (var dishIngredient in dishIngridientList)
        {
            var dishId = _dbContext.Dishes.FirstOrDefault(x => x.Name == dishIngredient.DishName).Id;
            var ingredientsToAdd = _dbContext.Ingredients.Where(x => dishIngredient.Ingredients.Any(y => y.IngredientName == x.Name));

            foreach (var ing in ingredientsToAdd)
            {
                var newDishIngredient = new IngredientDish
                {
                    Id = Guid.NewGuid(),

                }
                .SetCreateBy(User.GetName(), now);

                ModelState.AddModelError("Dish", $"This ingredient is not valid: {newDishIngredient}");
                
                else
                {
                    _dbContext.Add(newDishIngredient);
                }
            }
        }


        // Process each list separately
        return Ok(new
        {
            Message = "File uploaded successfully.",
            model = ModelState
        });


    }
}
