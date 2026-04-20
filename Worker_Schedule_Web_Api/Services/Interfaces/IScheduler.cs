using Worker_Schedule_Web_Api.DTOs.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IScheduler
    {
        Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date);
        Task<List<ScheduleDto>> CreateMonthSchedule(int year, int month);
        Task<List<ScheduleDto>> AddSingleWorker(ScheduleWorkerDto form);
        Task<List<ScheduleDto>> GetSchedules(ScheduleFilterDto filter);
        Task DeleteDaySchedule(DateOnly date);
        Task DeleteScheduleShift(Guid scheduleId);
        Task DeleteMonthSchedule(int year, int month);
    }
}
