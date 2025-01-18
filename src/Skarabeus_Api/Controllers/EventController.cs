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
            Start = createModel.Start,
            End = createModel.End,
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
            .Select(x => x.ToDetail(false))
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
            Start = eventItem.Start,
            End = eventItem.End,
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
        eventItem.Start = toUpdate.Start;
        eventItem.End = toUpdate.End;
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
    /// adds persons to event by Ids
    /// </summary>
    /// <param name="id">id of the event</param>
    /// <param name="personIds">list of ids of persons</param>
    /// <returns>returns the event details</returns>
    [HttpPost("AddPersonToEvent/{id}")]
    public async Task<ActionResult> AddPersonsToEvent(
        Guid id,
        Guid[] personIds
        )
    {
        var eventItem =await  _dbContext.Events
            .Include(x=>x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null) return NotFound("event was not found");

        var persons = _dbContext.Persons.Where(x=>personIds.Contains(x.Id));

        foreach(var per in persons) eventItem.Participants.Add(per);

        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// removes persons from event by Ids
    /// </summary>
    /// <param name="id">id of the event</param>
    /// <param name="personIds">list of ids of persons</param>
    /// <returns>returns the event details</returns>
    [HttpDelete("RemovePersonsFromEvent/{id}")]
    public async Task<ActionResult> RemovePersonsFromEvent(
        Guid id,
        Guid[] personIds
        )
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null) return NotFound("event was not found");

        var persons = eventItem.Participants.Where(x => personIds.Contains(x.Id));

        foreach (var per in persons) eventItem.Participants.Remove(per);

        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// adds dishes to event by Ids
    /// </summary>
    /// <param name="id">id of the event</param>
    /// <param name="dishesIds">list of ids of dishes</param>
    /// <returns>returns the event details</returns>
    [HttpPost("AddDishesToEvent/{id}")]
    public async Task<ActionResult> AddDishesToEvent(
        Guid id,
        Guid[] dishesIds
        )
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Dishes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null) return NotFound("event was not found");

        var dishes = _dbContext.Dishes.Where(x => dishesIds.Contains(x.Id));

        foreach (var d in dishes) eventItem.Dishes.Add(d);

        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

    /// <summary>
    /// removes dishes from event by Ids
    /// </summary>
    /// <param name="id">id of the event</param>
    /// <param name="dishesIds">list of ids of dishes</param>
    /// <returns>returns the event details</returns>
    [HttpDelete("RemoveDishesFromEvent/{id}")]
    public async Task<ActionResult> RemoveDishesFromEvent(
        Guid id,
        Guid[] dishesIds
        )
    {
        var eventItem = await _dbContext.Events
            .Include(x => x.Dishes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (eventItem == null) return NotFound("event was not found");

        var dishes = _dbContext.Dishes.Where(x => dishesIds.Contains(x.Id));

        foreach (var d in dishes) eventItem.Dishes.Remove(d);

        await _dbContext.SaveChangesAsync();

        return Ok(eventItem.ToDetail(true));
    }

}