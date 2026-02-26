using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IManageAvailabilityService
    {
        Task<List<ScheduleDto>> CreateDayAvailability(DateOnly date);
        Task<List<ShiftDemand>> GetShiftDemand(DateOnly date);
    }
}
