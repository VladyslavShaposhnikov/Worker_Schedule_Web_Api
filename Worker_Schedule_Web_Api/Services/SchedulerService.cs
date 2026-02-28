using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class SchedulerService(AppDbContext context, ILogger<SchedulerService> logger) : IScheduler
    {
        public async Task<List<ScheduleDto>> SetWorkersIntoSchedule(DateOnly date)
        {
            var schedules = await context
                .Schedules
                .Where(s => s.Date.Year == date.Year && s.Date.Month == date.Month)
                .ToListAsync();
            var demands = await context.ShiftDemands
                .Where(sd => sd.Date == date)
                .Select(sd => new
                {
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();

            var workersAvailable = await context.Availabilities
                .Include(a => a.Worker)
                .Where(a => a.Date == date)
                .Select(a => new
                {
                    Date = a.Date,
                    From = a.WorkingUnit.From,
                    To = a.WorkingUnit.To,
                    Worker = a.Worker,
                    WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                    FullName = $"{a.Worker.FirstName} {a.Worker.LastName}"
                })
                .ToListAsync();

            var workers = workersAvailable
                .Select(w => new
                {
                    Date = w.Date,
                    From = w.From,
                    To = w.To,
                    HoursLeft = HoursLeft(date, w.Worker, schedules),
                    WorkerInternalNumber = w.WorkerInternalNumber,
                    FullName = $"{w.Worker.FirstName} {w.Worker.LastName}"
                })
                .ToList();

            var result = new List<ScheduleDto>();
            foreach (var demand in demands)
            {
                var matchingWorkers = workers
                    .Where(w => w.From < demand.To && w.To > demand.From)
                    .OrderByDescending(w => w.To - w.From)
                    .ThenByDescending(w => w.HoursLeft)
                    .Take(demand.WorkersNeeded)
                    .ToList();

                foreach (var worker in matchingWorkers)
                {
                    logger.LogInformation("Worker has {Hours} for {Date}", worker.HoursLeft, date);

                    var isContain = result.Any(r => r.WorkerInternalNumber == worker.WorkerInternalNumber && r.Date == worker.Date);
                    if (isContain) continue;

                    var localFrom = worker.From;
                    var localTo = worker.To;

                    if (demand.From > worker.From) localFrom = demand.From;
                    if (worker.To > demand.To) localTo = demand.To;

                    result.Add(new ScheduleDto
                    {
                        Date = worker.Date,
                        From = localFrom,
                        To = localTo,
                        WorkerInternalNumber = worker.WorkerInternalNumber,
                        FullName = worker.FullName
                    });
                }
            }
            return result;
        }

        private double HoursLeft(DateOnly date, Worker worker, List<Schedule> schedules)
        {
            var totalHours = 160 * worker.EmploymentPercentage / 100;
            var hoursScheduled = schedules
                .Where(s => s.WorkerId == worker.Id && s.Date.Month == date.Month && s.Date.Year == date.Year)
                .Sum(s => (s.WorkingUnit.To - s.WorkingUnit.From).TotalHours);

            return totalHours - hoursScheduled;
        }
    }
}
