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
        public async Task<List<ScheduleDto>> CreateDayAvailability(DateOnly date)
        {
            return await scheduler.SetWorkersIntoSchedule(date);
        }

        public async Task<List<ShiftDemandDto>> GetShiftDemand(DateOnly date)
        {
            return await context.ShiftDemands
                .Where(sd => sd.Date == date)
                .Select(sd => new ShiftDemandDto
                {
                    Date = date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();
        }

        public async Task<List<ShiftDemandDto>> CreateShiftDemand(List<ShiftDemandDto> form)
        {
            var result = new List<ShiftDemandDto>();

            foreach (var formItem in form)
            {
                if (await context.ShiftDemands.AnyAsync(sd => sd.Date == formItem.Date)) throw new DateAlreadyExistsException(formItem.Date);

                var workingUnit = await context.WorkingUnits
                .Where(wu => wu.From == formItem.From && wu.To == formItem.To)
                .FirstOrDefaultAsync();
                if (workingUnit == null)
                {
                    workingUnit = new WorkingUnit
                    {
                        From = formItem.From,
                        To = formItem.To
                    };
                    context.WorkingUnits.Add(workingUnit);

                }
                context.ShiftDemands.Add(new ShiftDemand
                {
                    Date = formItem.Date,
                    WorkingUnit = workingUnit,
                    WorkersNeeded = formItem.WorkersNeeded
                });
            }
            await context.SaveChangesAsync();

            return result;
        }

        public async Task DeleteShiftDemand(DateOnly date)
        {
            await context.ShiftDemands.Where(sd => sd.Date == date).ExecuteDeleteAsync();
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
