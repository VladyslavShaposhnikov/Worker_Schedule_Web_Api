using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api/availabilities")]
    [Authorize(Roles = $"{AppRoles.Worker},{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
    public class AvailabilityController(IAvailabilityService availabilityService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<GetAvailabilityDto>>> Availabilities([FromQuery] AvailabilityFilterDto filters)
        {
            var result = await availabilityService.Availabilities(filters);
            return result;
        }

        [HttpGet]
        [Route("day/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> GetAvailability([FromRoute] DateOnly date)
        {
            var result = await availabilityService.GetAvailability(date);
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<GetAvailabilityDto>> CreateAvailability(CreateUpdateAvailabilityDto form)
        {
            var result = await availabilityService.CreateAvailability(form);
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpPost]
        [Route("bulk/{year}/{month}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> CreateMonthAvailability(CreateUpdateAvailabilityDto[] form,[FromRoute] int year,[FromRoute] int month)
        {
            var result = await availabilityService.CreateMonthAvailability(form, year, month);
            return result;
        }

        [HttpGet]
        [Route("month/{year}/{month}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> GetMonthAvailability([FromRoute] int year, [FromRoute] int month)
        {
            var result = await availabilityService.GetMonthAvailability(year, month);
            return result;
        }

        [HttpPut]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateAvailability(CreateUpdateAvailabilityDto form)
        {
            var result = await availabilityService.UpdateAvailability(form);
            return result;
        }

        [HttpPut]
        [Route("bulk/{year}/{month}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> UpdateMonthAvailability(CreateUpdateAvailabilityDto[] form,[FromRoute] int year,[FromRoute] int month)
        {
            var result = await availabilityService.UpdateMonthAvailability(form, year, month);
            return result;
        }

        [HttpPost]
        [Route("full-day/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> SetFullAvailability([FromRoute] DateOnly date)
        {
            var result = await availabilityService.SetFullAvailability(date);
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpDelete]
        [Route("day-off/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> DayOffAvailability([FromRoute] DateOnly date)
        {
            await availabilityService.DayOffAvailability(date);
            return NoContent();
        }
    }
}
