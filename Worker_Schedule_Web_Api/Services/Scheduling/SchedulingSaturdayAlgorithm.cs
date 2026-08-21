using Worker_Schedule_Web_Api.Enums;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services.Scheduling
{
    public class SchedulingSaturdayAlgorithm : ISchedulingSaturdayAlgorithm
    {
        public List<SchedulingResult> CalculateSaturday(
            List<SchedulingDemand> demands, 
            List<SchedulingWorker> workers, 
            Dictionary<Guid, int[]> workedSaturdays,
            int ttlMonthSaturdays)
        {
            var result = new List<SchedulingResult>();
            HashSet<Guid> alreadyAssignedForDay = new();

            foreach (var demand in demands)
            {
                var availableWorkers = workers
                    .Where(w => w.Date == demand.Date && w.From <= demand.From && w.To >= demand.To)
                    .OrderBy(w => w.Hours)
                    .ToList();

                var shift = GetShift(demand);

                int counter = 0;
                foreach (var worker in availableWorkers)
                {
                    if (CanWorkToday(worker, alreadyAssignedForDay, workedSaturdays, shift, ttlMonthSaturdays) && counter < demand.WorkersNeeded)
                    {
                        Console.WriteLine($"Worker {worker.FullName} assigned to shift {shift} shift count => {workedSaturdays[worker.WorkerId][(int)shift]}, day => {demand.Date}");
                        result.Add(new SchedulingResult
                        {
                            Date = demand.Date,
                            From = demand.From,
                            To = demand.To,
                            WorkerInternalNumber = worker.WorkerInternalNumber,
                            FullName = worker.FullName ?? string.Empty,
                            WorkerId = worker.WorkerId
                        });
                        alreadyAssignedForDay.Add(worker.WorkerId);
                        workedSaturdays[worker.WorkerId][(int)shift] += 1;
                        counter++;
                    }
                }
            }
            return result;
        }

        private Shift GetShift(SchedulingDemand demand)
        {
            if (demand.From <= new TimeOnly(9, 30))
            {
                return Shift.Morning;
            }
            else if (demand.To >= new TimeOnly(21, 30))
            {
                return Shift.Evening;
            }
            return Shift.Midday;
        }

        private bool CanWorkToday(
            SchedulingWorker worker,
            HashSet<Guid> alreadyAssignedForDay,
            Dictionary<Guid, int[]> workedSaturdays,
            Shift shift,
            int ttlMonthSaturdays
            )
        {
            var shifts = workedSaturdays[worker.WorkerId];
            // work today and have saturdays to work
            if (alreadyAssignedForDay.Contains(worker.WorkerId) || shifts.Sum() >= ttlMonthSaturdays - 1)
            {
                return false;
            }
            // already worked this shift or worked each shift once
            if (shifts[(int)shift] > 0 && !shifts.All(t => t == 1))
            {
                return false;
            }
            return true;
        }
    }
}
