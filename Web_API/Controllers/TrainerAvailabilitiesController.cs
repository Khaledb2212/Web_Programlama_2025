using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
namespace Web_API.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/TrainerAvailabilities")]
    [ApiController]
    public class TrainerAvailabilitiesController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public TrainerAvailabilitiesController(ProjectDbContext context)
        {
            _context = context;
        }

        //https://localhost:7085/api/TrainerAvailabilities/GetTrainerAvailabilities
        [HttpGet("GetTrainerAvailabilities",Name = "GetTrainerAvailabilities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TrainerAvailability>>> GetTrainerAvailabilities()
        {
            try
            {
                // Include Trainer and Service info so the schedule is readable
                var availabilities = await _context.TrainerAvailabilities
                                                   .Include(ta => ta.Trainer)
                                                        .ThenInclude(t => t.person) // See Trainer Name
                                                   .Include(ta => ta.Service)       // See Service Name
                                                   .ToListAsync();

                if (availabilities == null || availabilities.Count == 0)
                {
                    return NotFound("No schedules found.");
                }

                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving schedules: " + ex.Message);
            }
        }

        //https://localhost:7085/api/TrainerAvailabilities/GetTrainerAvailability?id=
        [HttpGet("GetTrainerAvailability", Name = "GetTrainerAvailability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrainerAvailability>> GetTrainerAvailability(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid ID");
                }

                var trainerAvailability = await _context.TrainerAvailabilities
                                                        .Include(ta => ta.Trainer)
                                                        .Include(ta => ta.Service)
                                                        .FirstOrDefaultAsync(ta => ta.AvailabilityId == id);

                if (trainerAvailability == null)
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }

                return Ok(trainerAvailability);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving schedule: " + ex.Message);
            }
        }

        //https://localhost:7085/api/TrainerAvailabilities/PostTrainerAvailability
        [HttpPost("PostTrainerAvailability",Name = "AddTrainerAvailability")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TrainerAvailability>> PostTrainerAvailability(TrainerAvailability trainerAvailability)
        {
            // 1. Basic Validation
            if (trainerAvailability == null)
            {
                return BadRequest("Schedule data is null.");
            }

            /*the json format to work with this is : 
             {
              "availabilityId": 0,
              "dayOfWeek": 5,
              "startTime": "2024-01-01T09:00:00",
              "endTime": "2024-01-01T17:00:00",
              "trainerId": 2,
              "serviceTypeId": 5,
              "trainer": null,
              "service": null
            }
            */

            // 2. Time Logic Check (Start must be before End)
            if (trainerAvailability.StartTime >= trainerAvailability.EndTime)
            {
                return BadRequest("Start Time must be earlier than End Time.");
            }

            // 3. Integrity Checks
            bool trainerExists = await _context.Trainers.AnyAsync(t => t.TrainerID == trainerAvailability.TrainerId);
            if (!trainerExists)
            {
                return BadRequest($"TrainerID {trainerAvailability.TrainerId} does not exist.");
            }

            bool serviceExists = await _context.Services.AnyAsync(s => s.ServiceID == trainerAvailability.ServiceTypeId);
            if (!serviceExists)
            {
                return BadRequest($"ServiceID {trainerAvailability.ServiceTypeId} does not exist.");
            }

            bool hasSkill = await _context.TrainerSkills.AnyAsync(ts =>
                ts.TrainerId == trainerAvailability.TrainerId &&
                ts.ServiceId == trainerAvailability.ServiceTypeId);

            if (!hasSkill)
            {
                return BadRequest($"Trainer {trainerAvailability.TrainerId} is not qualified for Service {trainerAvailability.ServiceTypeId}. Add this skill in the TrainerSkills table first.");
            }   

            try
            {
                _context.TrainerAvailabilities.Add(trainerAvailability);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTrainerAvailability", new { id = trainerAvailability.AvailabilityId }, trainerAvailability);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating schedule: " + ex.Message);
            }
        }

        //https://localhost:7085/api/TrainerAvailabilities/PutTrainerAvailability?id=
        [HttpPut("PutTrainerAvailability", Name = "UpdateTrainerAvailability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutTrainerAvailability(int id, TrainerAvailability trainerAvailability)
        {
            if (id != trainerAvailability.AvailabilityId || trainerAvailability == null)
            {
                return BadRequest("ID mismatch or invalid data.");
            }

            /*the json format to work with this is : 
             {
              "availabilityId": 0, <-- Do Not Forget to match it in the URL
              "dayOfWeek": 5,
              "startTime": "2024-01-01T09:00:00",
              "endTime": "2024-01-01T17:00:00",
              "trainerId": 2,
              "serviceTypeId": 5,
              "trainer": null,
              "service": null
            }
            */

            

            if (trainerAvailability.StartTime >= trainerAvailability.EndTime)
            {
                return BadRequest("Start Time must be earlier than End Time.");
            }

            bool hasSkill = await _context.TrainerSkills.AnyAsync(ts =>
             ts.TrainerId == trainerAvailability.TrainerId &&
             ts.ServiceId == trainerAvailability.ServiceTypeId);

            if (!hasSkill)
            {
                return BadRequest($"Trainer {trainerAvailability.TrainerId} does not have the skill for Service {trainerAvailability.ServiceTypeId}. Please assign the skill in the TrainerSkills table first.");
            }

            _context.Entry(trainerAvailability).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainerAvailabilityExists(id))
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Concurrency error during update.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating schedule: " + ex.Message);
            }

            return NoContent();
        }
    
        //https://localhost:7085/api/TrainerAvailabilities/DeleteTrainerAvailability?id=
        [HttpDelete("DeleteTrainerAvailability", Name = "DeleteTrainerAvailability")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTrainerAvailability(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            try
            {
                var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
                if (trainerAvailability == null)
                {
                    return NotFound($"Schedule with ID {id} not found.");
                }

                _context.TrainerAvailabilities.Remove(trainerAvailability);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting schedule: " + ex.Message);
            }
        }

        private bool TrainerAvailabilityExists(int id)
        {
            return _context.TrainerAvailabilities.Any(e => e.AvailabilityId == id);
        }
    }
}