using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IScheduleMonthAlgorithm
    {
        public List<SchedulingResult> Calculate(List<ShiftDemand> demands, List<Availability> workers, List<Schedule> schedules, int year, int month);
    }
}
