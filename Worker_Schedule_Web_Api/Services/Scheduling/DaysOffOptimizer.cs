using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services.Scheduling
{
    public class DaysOffOptimizer : IDaysOffOptimizer
    {
        public void Fix(
            int year, 
            int month, 
            List<Guid> fullShiftWorkers, 
            List<SchedulingResult> result, 
            List<Availability> workers, 
            Dictionary<Guid, double> hoursSum,
            int daysOffRequired)
        {
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
                var iters = daysOffRequired - worker.Value.Count;
                for (int i = 0; i < iters; i++)
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

                        var newShiftGuid = ReturnLowestHorsWorker(hoursSum, newShiftsList.Select(w => w.WorkerId));
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
                            hoursSum[newShift!.WorkerId] = hoursSum.GetValueOrDefault(newShift.WorkerId, 0) + (newShift.WorkingUnit.To - newShift.WorkingUnit.From).TotalHours;
                        }
                    }
                }
            }
        }

        private static Guid ReturnLowestHorsWorker(Dictionary<Guid, double> hoursSum, IEnumerable<Guid> workerIds)
        {
            var lowestHoursWorker = workerIds.OrderBy(id => hoursSum.GetValueOrDefault(id, 0)).FirstOrDefault();
            return lowestHoursWorker;
        }

        private static (int, int) GetNextDayOff(List<int> daysOff)
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
    }
}
