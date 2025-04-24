using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Controllers.Models.DishModels;
using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Api.Utils;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System;
using System.ComponentModel;

namespace Skarabeus_Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
[ApiController]
public class PersonController : ControllerBase
{

    private readonly ILogger<IngredientController> _logger;
    private readonly IClock _clock;
    private readonly ApplicationDbContext _dbContext;

    public PersonController(
        ILogger<IngredientController> logger,
        IClock clock,
        ApplicationDbContext dbContext
        )
    {
        _clock = clock;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new person with the provided details.
    /// This endpoint does not check for unique information.
    /// </summary>
    /// <param name="createModel">The model containing person details.</param>
    /// <returns>The created person or validation errors.</returns>
    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] PersonCreateModel createModel
        )
    {
        var now = _clock.GetCurrentInstant();

        var newPerson = new Person
        {
            FirstName = createModel.FirstName,
            Nickname = createModel.Nickname,
            LastName = createModel.LastName,
            Gender = createModel.Gender,
            DateOfBirth = DateTime.SpecifyKind(DateTime.Parse(createModel.DateOfBirth), DateTimeKind.Utc),
            EmailOfMother = createModel.EmailOfMother,
            EmailOfFather = createModel.EmailOfFather,
            Email = createModel.Email,
            PhoneNumberOfMother = createModel.PhoneNumberOfMother,
            PhoneNumberOfFather = createModel.PhoneNumberOfFather,
            PhoneNumber = createModel.PhoneNumber,
            FullNameOfMother = createModel.FullNameOfMother,
            FullNameOfFather = createModel.FullNameOfFather,
            Active = createModel.Active,
            Status = createModel.Status
        };

        if (User.Identity != null && User.Identity.IsAuthenticated) newPerson.SetCreateBy(User.GetName(), now);
        else newPerson.SetCreateBySystem(now);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        _dbContext.Add(newPerson);
        await _dbContext.SaveChangesAsync();

        return Ok(newPerson.ToDetail());
    }

    /// <summary>
    /// Retrieves a list of all active and non-deleted persons.
    /// </summary>
    /// <returns>A list of person detail models.</returns>
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var list = await _dbContext.Persons.Filter().Select(x => x.ToDetail()).ToArrayAsync();
        return Ok(list);
    }

    /// <summary>
    /// Retrieves a list of all persons, including inactive and deleted ones.
    /// </summary>
    /// <returns>A list of all person detail models.</returns>
    [HttpGet("unfiltred")]
    public async Task<ActionResult> GetUnfiltred()
    {
        var list = await _dbContext.Persons.Select(x => x.ToDetail()).ToArrayAsync();
        return Ok(list);
    }

    /// <summary>
    /// Retrieves details of a specific person by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the person.</param>
    /// <returns>The person's details or a NotFound response.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var person = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == id);
        if (person == null) return NotFound();
        return Ok(person.ToDetail());
    }

    /// <summary>
    /// Soft deletes a person by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the person to delete.</param>
    /// <returns>The deleted person's details or a NotFound response.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var now = _clock.GetCurrentInstant();

        var person = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == id);
        if (person == null) return NotFound();
        if(_dbContext.Users.Any(x=>x.Person == person))
        {
            person.SetDeleteBy(User.GetName(), now);
            await _dbContext.SaveChangesAsync();
            return Conflict(new { message = "Person is tied to a user, so it cannot be deleted. It has been marked as deleted instead." });
        }
        else _dbContext.Persons.Remove(person);

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Updates the details of a specific person using a JSON patch document.
    /// </summary>
    /// <param name="id">The unique identifier of the person to update.</param>
    /// <param name="patch">The JSON patch document containing updates.</param>
    /// <returns>The updated person details or validation errors if invalid.</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<PersonCreateModel> patch
        )
    {
        var person = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == id);


        if (person == null)
        {
            return NotFound();
        }

        var now = _clock.GetCurrentInstant();

        var toUpdate = new PersonCreateModel
        {
            FirstName = person.FirstName,
            LastName = person.LastName,
            Gender = person.Gender,
            DateOfBirth = person.DateOfBirth.ToString(),
            EmailOfMother = person.EmailOfMother,
            EmailOfFather = person.EmailOfFather,
            Email = person.Email,
            PhoneNumberOfMother = person.PhoneNumberOfMother,
            PhoneNumberOfFather = person.PhoneNumberOfFather,
            PhoneNumber = person.PhoneNumber,
            FullNameOfMother = person.FullNameOfMother,
            FullNameOfFather = person.FullNameOfFather,
            Active = person.Active,
            Status = person.Status,
            Nickname = person.Nickname
        };

        patch.ApplyTo(toUpdate);

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        person.FirstName = toUpdate.FirstName;
        person.LastName = toUpdate.LastName;
        person.Gender = toUpdate.Gender;
        person.DateOfBirth = DateTime.SpecifyKind(DateTime.Parse(toUpdate.DateOfBirth), DateTimeKind.Utc);
        person.EmailOfMother = toUpdate.EmailOfMother;
        person.EmailOfFather = toUpdate.EmailOfFather;
        person.Email = toUpdate.Email;
        person.PhoneNumberOfMother = toUpdate.PhoneNumberOfMother;
        person.PhoneNumberOfFather = toUpdate.PhoneNumberOfFather;
        person.PhoneNumber = toUpdate.PhoneNumber;
        person.FullNameOfMother = toUpdate.FullNameOfMother;
        person.FullNameOfFather = toUpdate.FullNameOfFather;
        person.Active = toUpdate.Active;
        person.Status = toUpdate.Status;
        person.Nickname = toUpdate.Nickname;


        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            person.SetModifyBy(User.GetName(), now);
        }
        else
        {
            person.SetModifyBySystem(now);
        }

        await _dbContext.SaveChangesAsync();

        return Ok(person.ToDetail());
    }
}

