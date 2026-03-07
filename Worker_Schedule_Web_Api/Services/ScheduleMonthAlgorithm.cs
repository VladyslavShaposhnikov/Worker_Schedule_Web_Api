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

                HashSet<Guid> alreadyAssignedForDay = schedules
                    .Where(s => s.Date == date)
                    .Select(s => s.WorkerId)
                    .ToHashSet();

                var dayDemands = demands
                    .Where(d => d.Date == date)
                    .Select(sd => new SchedulingDemand
                    {
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
                        From = a.WorkingUnit.From,
                        To = a.WorkingUnit.To,
                        Hours = hoursSum.GetValueOrDefault(a.WorkerId, 0) / (160 * (a.Worker.EmploymentPercentage / 100)), // magic number 160 is hardcoded temporary
                        WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                        WorkerId = a.WorkerId,
                        FullName = $"{a.Worker.FirstName} {a.Worker.LastName}"
                    })
                .ToList();

                var dayResult = schedulingAlgorithm.Calculate(dayDemands, dayWorkers, alreadyAssignedForDay);

                result.AddRange(dayResult);

                foreach (var i in dayResult)
                {
                    hoursSum[i.WorkerId] = hoursSum.GetValueOrDefault(i.WorkerId, 0) + (i.To - i.From).TotalHours;
                }
            }
            
            return result;
        }
    }
}
