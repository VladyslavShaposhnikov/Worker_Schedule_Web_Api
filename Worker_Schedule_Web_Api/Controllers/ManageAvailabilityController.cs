using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = AppRoles.Manager)]
    public class ManageAvailabilityController(IManageAvailabilityService manageAvailabilityService) : ControllerBase
    {
        [HttpPost]
        [Route("create-day-shedule")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateDayAvailability(DateOnly date)
        {
            var result = await manageAvailabilityService.CreateDayAvailability(date);
            return StatusCode(201, result);
        }

        [HttpGet]
        [Route("shift-demand")]
        public async Task<ActionResult<List<ShiftDemandDto>>> GetShiftDemand(DateOnly date)
        {
            var result = await manageAvailabilityService.GetShiftDemand(date);
            return result;
        }

        [HttpGet]
        [Route("availabilities")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> GetAllAvailabilities(DateOnly date)
        {
            var result = await manageAvailabilityService.GetAllAvailabilities(date);
            return result;
        }

        [HttpPost]
        [Route("create-shift-demand")]
        public async Task<ActionResult<List<ShiftDemandDto>>> CreateShiftDemand(List<ShiftDemandDto> form)
        {
            var result = await manageAvailabilityService.CreateShiftDemand(form);
            return StatusCode(201, result);
        }

        [HttpDelete]
        [Route("delete-shift-demand")]
        public async Task<ActionResult> DeleteShiftDemand(DateOnly date)
        {
            await manageAvailabilityService.DeleteShiftDemand(date);
            return NoContent();
        }
    }
}
