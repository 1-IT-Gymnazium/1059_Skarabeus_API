using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Skarabeus_Api.Controllers.Models.EventModels;
using Skarabeus_Api.Utils;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;

namespace Skarabeus_Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public EventController(
        ILogger<EventController> logger,
        IClock clock,
        ApplicationDbContext dbContext
    )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new event with the provided details.
    /// </summary>
    /// <param name="createModel">The model containing event details.</param>
    /// <returns>The created event or validation errors.</returns>
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] EventCreateModel createModel
        )
    {
        var now = _clock.GetCurrentInstant();

        var newEvent = new Event
        {
            Name = createModel.Name,
            Description = createModel.Description,
            ResponsiblePersonId = createModel.ResponsiblePersonId,
            Start = DateTime.Parse(createModel.Start).ToUniversalTime(),
            End = DateTime.Parse(createModel.End).ToUniversalTime(),
            Place = createModel.Place
        };

        if (User.Identity != null && User.Identity.IsAuthenticated) newEvent.SetCreateBy(User.GetName(), now);
        else newEvent.SetCreateBySystem(now);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _dbContext.Add(newEvent);
        await _dbContext.SaveChangesAsync();

        return Ok(newEvent.ToDetail(true));
    }

    /// <summary>
    /// Retrieves a list of all events.
    /// </summary>
    /// <returns>A list of event detail models.</returns>
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var list = await _dbContext.Events
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.Participants)
            .Include(x => x.Dishes)
            .Select(x => x.ToDetail(true))
            .ToArrayAsync();
        return Ok(list);
    }

    /// <summary>
    /// Retrieves details of a specific event by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <returns>The event details or a NotFound response.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var eventItem = await _dbContext.Events
            .Include(e => e.ResponsiblePerson)
            .Include(x => x.Participants)
            .Include(x=>x.Dishes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null) return NotFound();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// Updates the details of a specific event using a JSON patch document.
    /// </summary>
    /// <param name="id">The unique identifier of the event to update.</param>
    /// <param name="patch">The JSON patch document containing updates.</param>
    /// <returns>The updated event details or validation errors if invalid.</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<EventCreateModel> patch)
    {
        var now = _clock.GetCurrentInstant();

        var eventItem = await _dbContext.Events.FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }

        var toUpdate = new EventCreateModel
        {
            Name = eventItem.Name,
            Description = eventItem.Description,
            ResponsiblePersonId = eventItem.ResponsiblePersonId,
            Start = eventItem.Start.ToString(),
            End = eventItem.End.ToString(),
            Place = eventItem.Place,
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        eventItem.Name = toUpdate.Name;
        eventItem.Description = toUpdate.Description;
        eventItem.ResponsiblePersonId = toUpdate.ResponsiblePersonId;
        eventItem.Start = DateTime.Parse(toUpdate.Start).ToUniversalTime();
        eventItem.End = DateTime.Parse(toUpdate.Start).ToUniversalTime();
        eventItem.Place = toUpdate.Place;

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            eventItem.SetModifyBy(User.GetName(), now);
        }
        else
        {
            eventItem.SetModifyBySystem(now);
        }

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Deletes an event by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the event to delete.</param>
    /// <returns>Success or a NotFound response if the event does not exist.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var eventItem = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventItem == null) return NotFound();

        _dbContext.Events.Remove(eventItem);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    /// <summary>
    /// Adds a person to an event by their ID.
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <param name="personId">ID of the person</param>
    /// <returns>Returns the updated event details</returns>
    [HttpGet("AddPersonToEvent/{id}/{personId}")]
    public async Task<ActionResult> AddPersonToEvent(Guid id, Guid personId)
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);

        var person = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == personId);

        if (eventItem == null || person == null) return NotFound("Event or person was not found");

        eventItem.Participants.Add(person);
        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// Removes a person from an event by their ID.
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <param name="personId">ID of the person</param>
    /// <returns>Returns the updated event details</returns>
    [HttpDelete("RemovePersonFromEvent/{id}/{personId}")]
    public async Task<ActionResult> RemovePersonFromEvent(Guid id, Guid personId)
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);

        var person = eventItem?.Participants.FirstOrDefault(x => x.Id == personId);

        if (eventItem == null || person == null) return NotFound("Event or person was not found");

        eventItem.Participants.Remove(person);
        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// Adds a dish to an event by its ID.
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <param name="dishId">ID of the dish</param>
    /// <returns>Returns the updated event details</returns>
    [HttpGet("AddDishToEvent/{id}/{dishId}")]
    public async Task<ActionResult> AddDishToEvent(Guid id, Guid dishId)
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Dishes)
            .FirstOrDefaultAsync(x => x.Id == id);

        var dish = await _dbContext.Dishes.FirstOrDefaultAsync(x => x.Id == dishId);

        if (eventItem == null || dish == null) return NotFound("Event or dish was not found");

        eventItem.Dishes.Add(dish);
        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// Removes a dish from an event by its ID.
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <param name="dishId">ID of the dish</param>
    /// <returns>Returns the updated event details</returns>
    [HttpDelete("RemoveDishFromEvent/{id}/{dishId}")]
    public async Task<ActionResult> RemoveDishFromEvent(Guid id, Guid dishId)
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Dishes)
            .FirstOrDefaultAsync(x => x.Id == id);

        var dish = eventItem?.Dishes.FirstOrDefault(x => x.Id == dishId);

        if (eventItem == null || dish == null) return NotFound("Event or dish was not found");

        eventItem.Dishes.Remove(dish);
        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }


}