using Worker_Schedule_Web_Api.DTOs.ManageAvailability;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IScheduler
    {
        Task<List<ScheduleDto>> SetWorkersIntoSchedule(DateOnly date);
    }
}
