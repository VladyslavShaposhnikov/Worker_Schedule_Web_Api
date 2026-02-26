using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class ManageAvailabilityService : IManageAvailabilityService
    {
        public Task<List<ScheduleDto>> CreateDayAvailability(DateOnly date)
        {
            throw new NotImplementedException();
        }
    }
}
