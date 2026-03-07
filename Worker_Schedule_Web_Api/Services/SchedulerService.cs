using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class SchedulerService(AppDbContext context, ISchedulingAlgorithm schedulingAlgorithm, IScheduleMonthAlgorithm scheduleMonthAlgorithm) : IScheduler
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
                    Hours = hoursSum.GetValueOrDefault(a.WorkerId, 0) / (160 * (a.Worker.EmploymentPercentage / 100)), // magic number 160 is hardcoded temporary
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
                var workingUnit = CreateWorkingUnitIfNotExists(schedule.From, schedule.To, workingUnits);

                context.Schedules.Add(new Schedule
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
            var workingUnits = await context
                .WorkingUnits
                .ToListAsync();

            var schedules = await context
                .Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .ToListAsync();

            var demands = await context.ShiftDemands
                .Where(sd => sd.Date.Year == year && sd.Date.Month == month)
                .ToListAsync();

            var workers = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Include(a => a.Worker)
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .ToListAsync();

            var result = scheduleMonthAlgorithm.Calculate(demands, workers, schedules, year, month);

            var resultSchedules = new List<ScheduleDto>();

            foreach (var schedule in result)
            {
                var workingUnit = CreateWorkingUnitIfNotExists(schedule.From, schedule.To, workingUnits);

                context.Schedules.Add(new Schedule
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

                resultSchedules.Add(resultSchedule);
            }
            // await context.SaveChangesAsync();
            return resultSchedules;
        }

        private WorkingUnit CreateWorkingUnitIfNotExists(TimeOnly from, TimeOnly to, List<WorkingUnit> workingUnits)
        {
            var workingUnit = workingUnits.FirstOrDefault(wu => wu.From == from && wu.To == to);
            if (workingUnit == default)
            {
                workingUnit = new WorkingUnit
                {
                    From = from,
                    To = to
                };
                workingUnits.Add(workingUnit);
                context.WorkingUnits.Add(workingUnit);
            }
            return workingUnit;
        }
    }
}
