using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;
using Worker_Schedule_Web_Api.Exceptions;

namespace Worker_Schedule_Web_Api.Services
{
    public class ManageAvailabilityService(AppDbContext context, IScheduler scheduler) : IManageAvailabilityService
    {
        public async Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date)
        {
            return await scheduler.SetWorkersIntoSchedule(date);
        }

        public async Task<List<GetAvailabilityDto>> GetAllAvailabilities(DateOnly date)
        {
            return await context.Availabilities
                .Select(a => 
                new GetAvailabilityDto 
                {
                    WorkerName = $"{a.Worker.FirstName} {a.Worker.LastName}",
                    WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                    WorkerPosition = a.Worker.Position.Name,
                    Date = a.Date,
                    From = a.WorkingUnit.From,
                    To = a.WorkingUnit.To
                })
                .Where(a => a.Date == date)
                .ToListAsync();
        }
    }
}
