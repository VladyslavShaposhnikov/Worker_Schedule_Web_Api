using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IWeeklyBreakOptimizer
    {
        List<WeeklyBreakIssuesDto> Show(
            int year,
            int month,
            List<Worker> workers,
            List<Schedule> schedules);
    }
}
