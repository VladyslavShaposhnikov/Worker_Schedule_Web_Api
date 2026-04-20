using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
    public class SchedulerController(IScheduler scheduler) : ControllerBase
    {
        [HttpPost]
        [Route("{date}")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateDaySchedule([FromRoute] DateOnly date)
        {
            var result = await scheduler.CreateDaySchedule(date);
            return StatusCode(201, result);
        }

        [HttpPost]
        [Route("{year:int}/{month:int}")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateMonthSchedule([FromRoute] int year, [FromRoute] int month)
        {
            var result = await scheduler.CreateMonthSchedule(year, month);
            return StatusCode(201, result);
        }

        [HttpPost]
        [Route("add-single-worker")]
        public async Task<ActionResult<List<ScheduleDto>>> AddSingleWorker(ScheduleWorkerDto form)
        {
            var result = await scheduler.AddSingleWorker(form);
            return StatusCode(201, result);
        }

        [HttpGet]
        public async Task<List<ScheduleDto>> GetSchedule([FromQuery] ScheduleFilterDto filter)
        {
            var result = await scheduler.GetSchedules(filter);
            return result;
        }

        [HttpDelete]
        [Route("{scheduleId:guid}")]
        public async Task<ActionResult> DeleteScheduleById([FromRoute]Guid scheduleId)
        {
            await scheduler.DeleteScheduleShift(scheduleId);
            return NoContent();
        }

        [HttpDelete]
        [Route("{date}")]
        public async Task<ActionResult> DeleteSchedule([FromRoute] DateOnly date)
        {
            await scheduler.DeleteDaySchedule(date);
            return NoContent();
        }

        [HttpDelete]
        [Route("{year:int}/{month:int}/month")]
        public async Task<ActionResult> DeleteMonthSchedule([FromRoute] int year, [FromRoute] int month)
        {
            await scheduler.DeleteMonthSchedule(year, month);
            return NoContent();
        }
    }
}
