using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IScheduler
    {
        Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date);
        Task<List<ScheduleDto>> CreateMonthSchedule(int year, int month);
        Task<List<ScheduleDto>> AddSingleWorker(ScheduleWorkerDto form);
        Task<List<ScheduleDto>> GetSchedules(ScheduleFilterDto filter);
        Task<List<ScheduleDto>> GetUserSchedules(Guid userId);
        Task DeleteDaySchedule(DateOnly date);
        Task DeleteScheduleShift(Guid scheduleId);
        Task DeleteMonthSchedule(int year, int month);
        Task<List<SummaryByWorkers>> WorkersSummary(int year, int month);
        Task<List<SchedulingDemand>> GetMissingShifts(DateOnly date);
        Task<List<ScheduleDto>> MonthSchedule(int year, int month);
        Task<List<WorkersLookupDto>> WorkersLookup();
        Task DeleteSchedulesByDaysRangeAndUsers(BulkDeleteSchedulesDto dto);
        Task<ScheduleDto> UpdateSchedule(Guid id, UpdateScheduleDto scheduleDto);
    }
}
