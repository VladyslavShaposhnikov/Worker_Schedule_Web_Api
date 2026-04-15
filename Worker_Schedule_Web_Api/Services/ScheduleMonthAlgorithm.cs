using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class ScheduleMonthAlgorithm(ISchedulingAlgorithm schedulingAlgorithm) : IScheduleMonthAlgorithm
    {
        public List<SchedulingResult> Calculate(List<ShiftDemand> demands, List<Availability> workers, List<Schedule> schedules, int year, int month)
        {
            var result = new List<SchedulingResult>();

            var hoursSum = schedules
                .GroupBy(s => s.WorkerId)
                .ToDictionary(d => d.Key, d => d.Sum(s => (s.WorkingUnit.To - s.WorkingUnit.From).TotalHours));

            foreach (var day in Enumerable.Range(1, DateTime.DaysInMonth(year, month)))
            {
                var date = new DateOnly(year, month, day);
                
                if (!demands.Any(d => d.Date == date) || !workers.Any(w => w.Date == date))
                {
                    continue; // nothing to schedule for this day, skip to the next one
                }

                var dayDemands = demands
                    .Where(d => d.Date == date)
                    .Select(sd => new SchedulingDemand
                    {
                        Date = date,
                        From = sd.WorkingUnit.From,
                        To = sd.WorkingUnit.To,
                        WorkersNeeded = sd.WorkersNeeded
                    })
                    .ToList();

                var dayWorkers = workers
                    .Where(a => a.Date == date)
                    .Select(a => new SchedulingWorker
                    {
                        Date = a.Date,
                        From = a?.WorkingUnit?.From,
                        To = a?.WorkingUnit?.To,
                        Hours = CalculateHours(hoursSum, a), // magic number 160 is hardcoded temporary
                        WorkerInternalNumber = a?.Worker?.WorkerInternalNumber ?? 0,
                        WorkerId = a?.WorkerId ?? Guid.Empty,
                        FullName = $"{a?.Worker?.FirstName} {a?.Worker?.LastName}",
                        Position = a?.Worker?.Position?.Name
                    })
                .ToList();

                var workedYesterdayEvening = new Dictionary<Guid, TimeOnly>();

                if (day == 1)
                {
                    foreach (var schedule in schedules
                        .Where(d => d.Date == date.AddDays(-1) && d.WorkingUnit.To >= new TimeOnly(20, 0)))
                    {
                        workedYesterdayEvening.Add(schedule.WorkerId, schedule.WorkingUnit.To);
                    }
                }
                else
                {
                    foreach (var schedule in result
                        .Where(d => d.Date == date.AddDays(-1) && d.To >= new TimeOnly(20, 0)))
                    {
                        workedYesterdayEvening.Add(schedule.WorkerId, schedule.To);
                    }
                }

                var dayResult = schedulingAlgorithm.Calculate(dayDemands, dayWorkers, workedYesterdayEvening);

                result.AddRange(dayResult);

                foreach (var i in dayResult)
                {
                    hoursSum[i.WorkerId] = hoursSum.GetValueOrDefault(i.WorkerId, 0) + (i.To - i.From).TotalHours;
                }
            }
            
            return result;
        }

        private double CalculateHours(Dictionary<Guid, double>? sum, Availability? worker)
        {
            if (worker == null)
                return 0;
            return sum.GetValueOrDefault(worker.WorkerId, 0) / (160 * (worker.Worker.EmploymentPercentage / 100.0));
        }
    }
}
