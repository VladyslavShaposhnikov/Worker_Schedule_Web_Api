using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api/availabilities")]
    [Authorize]
    public class AvailabilityController(IAvailabilityService availabilityService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> Availabilities([FromQuery] AvailabilityFilterDto filters)
        {
            var result = await availabilityService.Availabilities(filters);
            return result;
        }

        [HttpGet]
        [Authorize]
        [Route("user")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> AvailabilitiesUser([FromQuery] string userId)
        {
            var result = await availabilityService.AvailabilitiesUser(userId);
            return result;
        }

        [HttpGet]
        [Route("day/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> GetAvailability([FromRoute] DateOnly date)
        {
            var result = await availabilityService.GetAvailability(date);
            return result;
        }

        [HttpPatch]
        [Route("id/{id}")]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateFinishShiftHour([FromRoute] Guid id, [FromBody] UpdateFinishShiftHourDto dto)
        {
            var result = await availabilityService.UpdateFinishShiftHour(id, dto.FinishShiftHour);
            return result;
        }

        [HttpPut]
        [Route("id/{id}/update")]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateShift([FromRoute] Guid id, [FromBody] CreateUpdateAvailabilityDto dto)
        {
            var result = await availabilityService.UpdateShift(id, dto);
            return result;
        }

        [HttpGet]
        [Route("available-workers/{date}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> GetAvailableWorkers([FromRoute] DateOnly date)
        {
            var result = await availabilityService.GetAvailableWorkers(date);
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
        [Route("id/{id}")]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateAvailability([FromRoute] Guid id, [FromBody] CreateUpdateAvailabilityDto form)
        {
            var result = await availabilityService.UpdateAvailability(id, form);
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

        [HttpDelete]
        [Route("bulk-delete")]
        public async Task<ActionResult> DeleteBulkAvailability([FromBody] DateOnly[] dates)
        {
            await availabilityService.DeleteBulkAvailability(dates);
            return NoContent();
        }
    }
}
