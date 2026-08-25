using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services.Scheduling
{
    public class ScheduleMonthAlgorithm(
        ISchedulingAlgorithm schedulingAlgorithm,
        IDaysOffOptimizer daysOffOptimizer,
        IConfiguration configuration, 
        ILogger<ScheduleMonthAlgorithm> _logger) : IScheduleMonthAlgorithm
    {
        public List<SchedulingResult> Calculate(
            List<ShiftDemand> demands, 
            List<Availability> workers, 
            List<Schedule> schedules, 
            List<Guid> fullShiftWorkers,
            int year, 
            int month)
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
                        Hours = CalculateHours(hoursSum, a),
                        WorkerInternalNumber = a?.Worker?.WorkerInternalNumber ?? 0,
                        WorkerId = a?.WorkerId ?? Guid.Empty,
                        FullName = $"{a?.Worker?.FirstName} {a?.Worker?.LastName}",
                        Position = a?.Worker?.Position?.Name,
                        EmploymentPercentage = a?.Worker?.EmploymentPercentage ?? 0
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

                var workedSaturdays = new Dictionary<Guid, int[]>();

                foreach (var worker in workers)
                {
                    int firstShift = result
                        .Count(w => w.Date.Year == date.Year
                            && w.Date.Month == date.Month
                            && w.WorkerId == worker.WorkerId
                            && w.Date.DayOfWeek == DayOfWeek.Saturday
                            && w.From <= new TimeOnly(9, 30));

                    int secondShift = result
                        .Count(w => w.Date.Year == date.Year
                            && w.Date.Month == date.Month
                            && w.WorkerId == worker.WorkerId
                            && w.Date.DayOfWeek == DayOfWeek.Saturday
                            && w.From >= new TimeOnly(12, 0)
                            && w.To <= new TimeOnly(20, 0));

                    int thirdShift = result
                        .Count(w => w.Date.Year == date.Year
                            && w.Date.Month == date.Month
                            && w.WorkerId == worker.WorkerId
                            && w.Date.DayOfWeek == DayOfWeek.Saturday
                            && w.To >= new TimeOnly(21, 30));

                    workedSaturdays[worker.WorkerId] = new int[3] { firstShift, secondShift, thirdShift };
                }

                int saturdays = 0;

                foreach (var item in Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month)))
                {
                    if (new DateOnly(date.Year, date.Month, item).DayOfWeek == DayOfWeek.Saturday)
                    {
                        saturdays++;
                    }
                }

                var dayResult = schedulingAlgorithm.Calculate(dayDemands, dayWorkers, workedYesterdayEvening, workedSaturdays, saturdays);

                result.AddRange(dayResult);

                foreach (var i in dayResult)
                {
                    hoursSum[i.WorkerId] = hoursSum.GetValueOrDefault(i.WorkerId, 0) + (i.To - i.From).TotalHours;
                }
            }

            // After the initial scheduling, we need to ensure that each worker has at least 8 (hardcoded value) days off in the month.

            daysOffOptimizer.Fix(year, month, fullShiftWorkers, result, workers, hoursSum, 8);

            return result;
        }

        private double CalculateHours(Dictionary<Guid, double>? sum, Availability? worker)
        {
            if (worker == null)
                return 0;
            // now it get the month worker hours from configuration, if not set, it will be 168 (42 hours per week * 4 weeks),
            // but it should be changed to be more dynamic in the future
            int monthWorkerHours = configuration.GetValue<int>("MonthWorkerHours", 168);
            return sum.GetValueOrDefault(worker.WorkerId, 0) / (monthWorkerHours * (worker.Worker.EmploymentPercentage / 100.0));
        }
    }
}
