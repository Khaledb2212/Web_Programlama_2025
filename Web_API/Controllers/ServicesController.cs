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
    [Route("api/Services")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public ServicesController(ProjectDbContext context)
        {
            _context = context;
        }

        //https://localhost:7085/api/Services/GetServices
        [HttpGet("GetServices",Name = "GetServices")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            try
            {
                var services = await _context.Services.ToListAsync();

                if (services == null || services.Count == 0)
                {
                    return NotFound("No services found.");
                }

                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving services: " + ex.Message);
            }
        }

        //https://localhost:7085/api/Services/GetService?id=
        [HttpGet("GetService", Name = "GetService")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid ID");
                }

                var service = await _context.Services.FindAsync(id);

                if (service == null)
                {
                    return NotFound($"Service with ID {id} not found.");
                }

                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving service: " + ex.Message);
            }
        }

        //https://localhost:7085/api/Services/PostService
        [HttpPost("PostService",Name = "AddService")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Service>> PostService(Service service)
        {
            if (service == null)
            {
                return BadRequest("Service data is null.");
            }

            try
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetService", new { id = service.ServiceID }, service);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating service: " + ex.Message);
            }
        }

        // PUT: api/Services/5
        [HttpPut("PutService", Name = "UpdateService")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutService(int id, Service service)
        {
            if (id <= 0)
            {
                return BadRequest("This Is An Invalid ID");
            }

            if (id != service.ServiceID || service == null)
            {
                return BadRequest("ID mismatch or invalid data for service.");
            }

            // 2. Load Existing Service
            // We fetch the record first to make sure it exists
            var existingService = await _context.Services.FindAsync(id);

            if (existingService == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }

            // 3. Update Service Properties
            // We manually copy values to prevent accidental null overwrites
            existingService.ServiceName = service.ServiceName;
            existingService.FeesPerHour = service.FeesPerHour;
            existingService.Details = service.Details;

            /*
             The Correct JSON Request:
             {
               "serviceID": 1,
               "serviceName": "Advanced Yoga",
               "feesPerHour": 200,
               "details": "A 90-minute intensive yoga session."
             }
             */

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating service: " + ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/Services/5
        [HttpDelete("DeleteService", Name = "DeleteService")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteService(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound($"Service with ID {id} not found.");
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting service: " + ex.Message);
            }
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceID == id);
        }
    }
}