using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class SchedulerService(AppDbContext context, ISchedulingAlgorithm schedulingAlgorithm) : IScheduler
    {
        public async Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date)
        {
            var workingUnits = await context.WorkingUnits.ToListAsync();

            var schedules = await context
                .Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date.Year == date.Year && s.Date.Month == date.Month)
                .ToListAsync();

            var hoursSum = schedules
                .GroupBy(s => s.WorkerId)
                .ToDictionary(d => d.Key, d => d.Sum(s => (s.WorkingUnit.To - s.WorkingUnit.From).TotalHours));

            var demands = await context.ShiftDemands
                .Where(sd => sd.Date == date)
                .Select(sd => new SchedulingDemand
                {
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();

            var workers = await context.Availabilities
                .Where(a => a.Date == date)
                .Select(a => new SchedulingWorker
                {
                    Date = a.Date,
                    From = a.WorkingUnit.From,
                    To = a.WorkingUnit.To,
                    Hours = 160 * a.Worker.EmploymentPercentage / 100 - hoursSum.GetValueOrDefault(a.WorkerId, 0), // magic number 160 is hardcoded temporary
                    WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                    WorkerId = a.WorkerId,
                    FullName = $"{a.Worker.FirstName} {a.Worker.LastName}"
                })
                .ToListAsync();

            var hashWorkersSet = schedules
                .Where(s => s.Date == date)
                .Select(s => s.WorkerId)
                .ToHashSet();

            var result = new List<ScheduleDto>();

            var calculationResult = schedulingAlgorithm.Calculate(demands, workers, hashWorkersSet);

            foreach (var schedule in calculationResult)
            {
                var workingUnit = workingUnits.FirstOrDefault(wu => wu.From == schedule.From && wu.To == schedule.To);
                if (workingUnit == default)
                {
                    workingUnit = new WorkingUnit
                    {
                        From = schedule.From,
                        To = schedule.To
                    };
                    workingUnits.Add(workingUnit);
                    context.WorkingUnits.Add(workingUnit);
                }

                context.Schedules.Add(new Models.Domain.Schedule
                {
                    Date = schedule.Date,
                    WorkerId = schedule.WorkerId,
                    WorkingUnit = workingUnit
                });

                var resultSchedule = new ScheduleDto
                {
                    Date = schedule.Date,
                    From = workingUnit.From,
                    To = workingUnit.To,
                    WorkerInternalNumber = schedule.WorkerInternalNumber,
                    FullName = schedule.FullName
                };

                result.Add(resultSchedule);
            }

            //await context.SaveChangesAsync();
            return result;
        }

        public async Task<List<ScheduleDto>> CreateMonthSchedule(int year, int month)
        {
            var result = new List<ScheduleDto>();

            foreach (var day in Enumerable.Range(1, DateTime.DaysInMonth(year, month)))
            {
                throw new NotImplementedException();
            }

            return result;
        }
    }
}
