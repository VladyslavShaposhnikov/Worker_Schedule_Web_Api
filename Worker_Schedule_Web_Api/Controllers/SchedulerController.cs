using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = AppRoles.Manager)]
    public class SchedulerController(IScheduler scheduler) : ControllerBase
    {
        [HttpPost]
        [Route("create-day-shedule")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateDaySchedule(DateOnly date)
        {
            var result = await scheduler.CreateDaySchedule(date);
            return StatusCode(201, result);
        }

        [HttpPost]
        [Route("create-month-shedule/{year:int}/{month:int}")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateMonthSchedule(int year, int month)
        {
            var result = await scheduler.CreateMonthSchedule(year, month);
            return StatusCode(201, result);
        }
    }
}
