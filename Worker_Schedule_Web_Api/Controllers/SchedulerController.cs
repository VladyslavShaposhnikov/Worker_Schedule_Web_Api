using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    public class SchedulerController(IScheduler scheduler) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("{date}")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateDaySchedule([FromRoute] DateOnly date)
        {
            var result = await scheduler.CreateDaySchedule(date);
            return StatusCode(201, result);
        }

        [HttpPost]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("{year:int}/{month:int}")]
        public async Task<ActionResult<List<ScheduleDto>>> CreateMonthSchedule([FromRoute] int year, [FromRoute] int month)
        {
            var result = await scheduler.CreateMonthSchedule(year, month);
            return StatusCode(201, result);
        }

        [HttpDelete]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("dates-range")]
        public async Task<ActionResult> DeleteSchedulesByDaysRange([FromBody] BulkDeleteSchedulesDto dto)
        {
            await scheduler.DeleteSchedulesByDaysRangeAndUsers(dto);
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("add-single-worker")]
        public async Task<ActionResult<List<ScheduleDto>>> AddSingleWorker(ScheduleWorkerDto form)
        {
            var result = await scheduler.AddSingleWorker(form);
            return StatusCode(201, result);
        }

        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        public async Task<List<ScheduleDto>> GetSchedule([FromQuery] ScheduleFilterDto filter)
        {
            var result = await scheduler.GetSchedules(filter);
            return result;
        }
        
        [HttpGet]
        [Authorize]
        [Route("user")]
        public async Task<List<ScheduleDto>> GetUserSchedule([FromQuery] Guid userId)
        {
            var result = await scheduler.GetUserSchedules(userId);
            return result;
        }

        [HttpGet]
        [Authorize]
        [Route("workers")]
        public async Task<List<WorkersLookupDto>> GetWorkersLookup()
        {
            var result = await scheduler.WorkersLookup();
            return result;
        }

        [HttpGet]
        [Authorize]
        [Route("missing-shifts/{date}")]
        public async Task<List<SchedulingDemand>> GetMissingShifts([FromRoute] DateOnly date)
        {
            var result = await scheduler.GetMissingShifts(date);
            return result;
        }

        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("summary/{year:int}/{month:int}")]
        public async Task<List<SummaryByWorkers>> GetWorkerScheduleSummary([FromRoute] int year,[FromRoute] int month)
        {
            var result = await scheduler.WorkersSummary(year, month);
            return result;
        }

        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("month/{year:int}/{month:int}")]
        public async Task<List<ScheduleDto>> GetMonthSchedule([FromRoute] int year, [FromRoute] int month)
        {
            var result = await scheduler.MonthSchedule(year, month);
            return result;
        }

        [HttpDelete]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("{scheduleId:guid}")]
        public async Task<ActionResult> DeleteScheduleById([FromRoute]Guid scheduleId)
        {
            await scheduler.DeleteScheduleShift(scheduleId);
            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("{date}")]
        public async Task<ActionResult> DeleteSchedule([FromRoute] DateOnly date)
        {
            await scheduler.DeleteDaySchedule(date);
            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
        [Route("{year:int}/{month:int}/month")]
        public async Task<ActionResult> DeleteMonthSchedule([FromRoute] int year, [FromRoute] int month)
        {
            await scheduler.DeleteMonthSchedule(year, month);
            return NoContent();
        }
    }
}
