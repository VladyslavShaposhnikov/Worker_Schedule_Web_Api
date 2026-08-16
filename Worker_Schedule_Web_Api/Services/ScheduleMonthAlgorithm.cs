using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class ScheduleMonthAlgorithm(ISchedulingAlgorithm schedulingAlgorithm, IConfiguration configuration, ILogger<ScheduleMonthAlgorithm> _logger) : IScheduleMonthAlgorithm
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

            // After the initial scheduling, we need to ensure that each worker has at least 9 (hardcoded value) days off in the month.

            var monthDays = new List<int>(Enumerable.Range(1, DateTime.DaysInMonth(year, month)));

            var workersAndDaysOff = new Dictionary<Guid, List<int>>();
            foreach (var workerId in fullShiftWorkers)
            {
                var daysOff = new List<int>();
                foreach (var day in monthDays)
                {
                    if (!result.Any(r => r.WorkerId == workerId && r.Date.Day == day))
                    {
                        daysOff.Add(day);
                    }
                }
                workersAndDaysOff[workerId] = daysOff;
            }

            foreach (var worker in workersAndDaysOff)
            {
                var iters = 9 - worker.Value.Count;
                for (int i = 0; i < iters; i++) // 9 is hardcoded for now, but it should be changed to be more dynamic in the future
                {
                    var indexes = GetNextDayOff(worker.Value);
                    var newDayOff = monthDays.IndexOf(indexes.Item1) + (monthDays.IndexOf(indexes.Item2) - monthDays.IndexOf(indexes.Item1)) / 2;
                    var shiftToRemove = result.FirstOrDefault(r => r.WorkerId == worker.Key && r.Date.Day == newDayOff);
                    if (shiftToRemove != null)
                    {
                        worker.Value.Add(newDayOff);
                        worker.Value.Sort();
                        result.Remove(shiftToRemove);

                        var newShiftsList = workers
                            .Where(w =>
                                w.Date.Day == newDayOff
                                && !fullShiftWorkers.Contains(w.WorkerId)
                                && !result.Any(r => r.WorkerId == w.WorkerId && r.Date.Day == newDayOff)
                                && w.WorkingUnit.From <= shiftToRemove?.From
                                && w.WorkingUnit.To >= shiftToRemove?.To
                                );

                        var newShiftGuid = returnLowestHorsWorker(hoursSum, newShiftsList.Select(w => w.WorkerId));
                        var newShift = newShiftsList.FirstOrDefault(w => w.WorkerId == newShiftGuid);

                        if (newShift != null)
                        {
                            result.Add(new SchedulingResult
                            {
                                Date = new DateOnly(year, month, newDayOff),
                                From = shiftToRemove?.From ?? new TimeOnly(0, 0),
                                To = shiftToRemove?.To ?? new TimeOnly(0, 0),
                                WorkerInternalNumber = newShift?.Worker?.WorkerInternalNumber ?? 0,
                                FullName = $"{newShift?.Worker?.FirstName} {newShift?.Worker?.LastName}",
                                WorkerId = newShift?.WorkerId ?? Guid.Empty
                            });
                            hoursSum[newShift.WorkerId] = hoursSum.GetValueOrDefault(newShift.WorkerId, 0) + (newShift.WorkingUnit.To - newShift.WorkingUnit.From).TotalHours;
                        }
                    }
                }
            }

            return result;
        }

        private Guid returnLowestHorsWorker(Dictionary<Guid, double> hoursSum, IEnumerable<Guid> workerIds)
        {
            var lowestHoursWorker = workerIds.OrderBy(id => hoursSum.GetValueOrDefault(id, 0)).FirstOrDefault();
            return lowestHoursWorker;
        }

        private (int, int) GetNextDayOff(List<int> daysOff)
        {
            (int indexOne, int indexTwo) result = (1, 1);
            if (daysOff.Count < 2)
            {
                return (1, 1);
            }

            var longestStreak = 0;
            var lastDayOff = daysOff[0];
            for (int i = 0; i < daysOff.Count; i++)
            {
                var currentDiff = daysOff[i] - lastDayOff;
                if (longestStreak < currentDiff)
                {
                    result = (lastDayOff, daysOff[i]);
                    longestStreak = currentDiff;
                }
                lastDayOff = daysOff[i];
            }
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
