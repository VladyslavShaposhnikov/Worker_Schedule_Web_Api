using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = AppRoles.Worker)]
    public class AvailabilityController(IAvailabilityService availabilityService) : ControllerBase
    {
        [HttpGet]
        [Route("month/{year:int}/{month:int}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> GetMonthAvailability([FromRoute] int year,[FromRoute] int month)
        {
            var resultList = await availabilityService.GetMonthAvailability(year, month);

            return resultList;
        }

        [HttpGet]
        [Route("day/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> GetAvailability([FromRoute] DateOnly date)
        {
            var result = await availabilityService.GetAvailability(date);
            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<GetAvailabilityDto>> CreateAvailability(CreateAvailabilityDto form)
        {
            var result = await availabilityService.CreateAvailability(form);
            if (result == null)
            {
                return BadRequest("Availability already exists");
            }
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpPut]
        [Route("{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateAvailability([FromRoute] DateOnly date, UpdateAvailabilityDto workingUnit)
        {
            var result = await availabilityService.UpdateAvailability(date, workingUnit);
            if (result == null)
            {
                return Problem();
            }
            return result;
        }

        [HttpPost]
        [Route("{date}/full")]
        public async Task<ActionResult<GetAvailabilityDto>> SetFullAvailability([FromRoute] DateOnly date)
        {
            var result = await availabilityService.SetFullAvailability(date);
            if (result == null)
            {
                return BadRequest("Availability already exists");
            }
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpPost]
        [Route("{date}/day-off")]
        public async Task<ActionResult<GetAvailabilityDto>> DayOffAvailability([FromRoute] DateOnly date)
        {
            await availabilityService.DayOffAvailability(date);
            return NoContent();
        }
    }
}
