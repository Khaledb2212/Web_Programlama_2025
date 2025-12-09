using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
using Web_Project.Models;

namespace Web_API.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/People")]
    [ApiController]
    public class PeopleController : Controller
    {
        private readonly ProjectDbContext _context;

        // 1. Dependency Injection: We inject the DB context instead of creating "new"
        public PeopleController(ProjectDbContext context)
        {
            _context = context;
        }

        //https://localhost:7085/api/People/GetPeople
        [HttpGet("GetPeople", Name = "GetPeople")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Person>>> GetPeople()
        {
            try
            {
                var peopleList = await _context.Person.ToListAsync();

                // Optional: If you want to return 404 if the table is empty
                if (peopleList == null || peopleList.Count == 0)
                {
                    return NotFound("No records found in the database.");
                }

                return Ok(peopleList); // Status 200
            }
            catch (Exception ex)
            {
                // Log the error (ex) here in a real app
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        //https://localhost:7085/api/People/GetPerson?id=
        [HttpGet("GetPerson", Name = "GetPerson")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            try
            {
                var person = await _context.Person.FindAsync(id);

                if (person == null)
                {
                    return NotFound($"Person with Id = {id} not found"); // Status 404
                }

                return Ok(person); // Status 200
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }


        //https://localhost:7085/api/People/PutPerson?id=
        [HttpPut("PutPerson", Name = "PutPerson")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            if(id < 0 )
            {
                return BadRequest("This Id Is Invalid");
            }


            if (id != person.PersonID)
            {
                return BadRequest("Person ID mismatch"); // Status 400
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound($"Person with Id = {id} not found"); // Status 404
                }
                else
                {
                    // If it's a concurrency error but the person exists, rethrow strictly
                    return StatusCode(StatusCodes.Status500InternalServerError, "Concurrency error occurred");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }

            return NoContent(); // Status 204 (Success, but nothing to return)
        }


        //https://localhost:7085/api/People/PostPerson
        [HttpPost("PostPerson", Name = "PostPerson")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            try
            {
                if (person == null)
                {
                    return BadRequest("Person object is null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Returns validation errors (e.g., missing Name)
                }

                _context.Person.Add(person);
                await _context.SaveChangesAsync();

                // Status 201 Created + The Location Header to find the new item
                return CreatedAtAction(nameof(GetPerson), new { id = person.PersonID }, person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new person record");
            }
        }


        //https://localhost:7085/api/People/DeletePerson?id=
        [HttpDelete("DeletePerson", Name = "DeletePerson")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                var person = await _context.Person.FindAsync(id);
                if (person == null)
                {
                    return NotFound($"Person with Id = {id} not found");
                }

                _context.Person.Remove(person);
                await _context.SaveChangesAsync();

                return NoContent(); // Status 204
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.PersonID == id);
        }
    }
}
