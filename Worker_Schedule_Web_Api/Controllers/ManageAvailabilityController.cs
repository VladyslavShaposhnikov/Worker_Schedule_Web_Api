using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<List<ScheduleDto>>> CreateDayAvailability(DateOnly date)
        {
            var result = await manageAvailabilityService.CreateDayAvailability(date);
            return StatusCode(201, result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ShiftDemand>>> GetShiftDemand(DateOnly date)
        {
            var result = await manageAvailabilityService.GetShiftDemand(date);
            return result;
        }
    }
}
