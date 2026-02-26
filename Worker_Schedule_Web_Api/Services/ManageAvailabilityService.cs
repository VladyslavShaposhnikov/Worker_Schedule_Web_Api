using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class ManageAvailabilityService(AppDbContext context) : IManageAvailabilityService
    {
        public async Task<List<ScheduleDto>> CreateDayAvailability(DateOnly date)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ShiftDemand>> GetShiftDemand(DateOnly date)
        {
            return await context.ShiftDemands.Where(sd => sd.Date == date).ToListAsync();
        }
    }
}
