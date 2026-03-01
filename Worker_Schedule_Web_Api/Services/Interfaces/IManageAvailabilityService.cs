using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IManageAvailabilityService
    {
        Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date);
        Task<List<GetAvailabilityDto>> GetAllAvailabilities(DateOnly date);
    }
}
