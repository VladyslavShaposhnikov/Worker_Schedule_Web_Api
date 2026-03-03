using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class SchedulingAlgorithm : ISchedulingAlgorithm
    {
        public List<SchedulingResult> Calculate(List<SchedulingDemand> demands, List<SchedulingWorker> workers, HashSet<Guid> alreadyAssignedForDay)
        {
            var result = new List<SchedulingResult>();
            foreach (var demand in demands)
            {
                var from30 = demand.From.AddMinutes(30);
                var to30 = demand.To.AddMinutes(-30);
                var matchingWorkers = workers
                    .Where(w => w.From <= from30 && w.To >= to30 && !alreadyAssignedForDay.Contains(w.WorkerId))
                    .OrderByDescending(w => w.Hours)
                    .ThenBy(w => w.To - w.From)
                    .Take(demand.WorkersNeeded)
                    .ToList();

                foreach (var worker in matchingWorkers)
                {
                    var localFrom = worker.From;
                    var localTo = worker.To;

                    if (demand.From > worker.From) localFrom = demand.From;
                    if (worker.To > demand.To) localTo = demand.To;

                    alreadyAssignedForDay.Add(worker.WorkerId);

                    result.Add(new SchedulingResult
                    {
                        Date = worker.Date,
                        From = localFrom,
                        To = localTo,
                        WorkerInternalNumber = worker.WorkerInternalNumber,
                        FullName = worker.FullName,
                        WorkerId = worker.WorkerId
                    });
                }
            }
            return result;
        }
    }
}
