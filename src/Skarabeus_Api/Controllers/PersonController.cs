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

namespace Skarabeus_Api.Controllers
{
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

        [HttpPost]
        public async Task<ActionResult> Create(
            [FromBody] PersonCreateModel createModel
            )
        {
            var now = _clock.GetCurrentInstant();

            var newPerson = new Person
            {
                FirstName = createModel.FirstName,
                LastName = createModel.LastName,
                Gender = createModel.Gender,
                DateOfBirth = createModel.DateOfBirth,
                EmailOfMother = createModel.EmailOfMother,
                EmailOfFather = createModel.EmailOfFather,
                Email = createModel.Email,
                PhoneNummberOfMother = createModel.PhoneNummberOfMother,
                PhoneNUmmberOfFather = createModel.PhoneNUmmberOfFather,
                PhoneNummber = createModel.PhoneNummber,
                FullNameOfMother = createModel.FullNameOfMother,
                FullNameOfFather = createModel.FullNameOfFather,
                Active = createModel.Active
            };

            if (User.Identity != null && User.Identity.IsAuthenticated) newPerson.SetCreateBy(User.GetName(), now);
            else newPerson.SetCreateBySystem(now);

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            _dbContext.Add(newPerson);
            await _dbContext.SaveChangesAsync();

            return Ok(newPerson);
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var list = await _dbContext.Persons.Filter().ToArrayAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(Guid id)
        {
            var ingredient = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == id);
            if (ingredient == null) return NotFound();
            return Ok(ingredient);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var now = _clock.GetCurrentInstant();

            var ingredient = await _dbContext.Persons.FirstOrDefaultAsync(x => x.Id == id);
            if (ingredient == null) return NotFound();

            if (User.Identity != null && User.Identity.IsAuthenticated) ingredient.SetDeleteBy(User.GetName(), now);
            else ingredient.SetDeleteBySystem(now);

            await _dbContext.SaveChangesAsync();

            return Ok(ingredient);
        }

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
                DateOfBirth = person.DateOfBirth,
                EmailOfMother = person.EmailOfMother,
                EmailOfFather = person.EmailOfFather,
                Email = person.Email,
                PhoneNummberOfMother = person.PhoneNummberOfMother,
                PhoneNUmmberOfFather = person.PhoneNUmmberOfFather,
                PhoneNummber = person.PhoneNummber,
                FullNameOfMother = person.FullNameOfMother,
                FullNameOfFather = person.FullNameOfFather,
                Active = person.Active
            };

            patch.ApplyTo(toUpdate);

            if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
            {
                return ValidationProblem(ModelState);
            }

            person.FirstName = toUpdate.FirstName;
            person.LastName = toUpdate.LastName;
            person.Gender = toUpdate.Gender;
            person.DateOfBirth = toUpdate.DateOfBirth;
            person.EmailOfMother = toUpdate.EmailOfMother;
            person.EmailOfFather = toUpdate.EmailOfFather;
            person.Email = toUpdate.Email;
            person.PhoneNummberOfMother = toUpdate.PhoneNummberOfMother;
            person.PhoneNUmmberOfFather = toUpdate.PhoneNUmmberOfFather;
            person.PhoneNummber = toUpdate.PhoneNummber;
            person.FullNameOfMother = toUpdate.FullNameOfMother;
            person.FullNameOfFather = toUpdate.FullNameOfFather;
            person.Active = toUpdate.Active;


            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                person.SetModifyBy(User.GetName(), now);
            }
            else
            {
                person.SetModifyBySystem(now);
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}

