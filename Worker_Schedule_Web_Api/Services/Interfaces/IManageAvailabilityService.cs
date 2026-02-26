using Worker_Schedule_Web_Api.DTOs.ManageAvailability;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IManageAvailabilityService
    {
        Task<List<ScheduleDto>> CreateDayAvailability(DateOnly date);
    }
}
