using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services.Scheduling
{
    public class SchedulingAlgorithm(ISchedulingSaturdayAlgorithm schedulingSaturdayAlgorithm) : ISchedulingAlgorithm
    {

        public List<SchedulingResult> Calculate(List<SchedulingDemand> demands, List<SchedulingWorker> workers, Dictionary<Guid, TimeOnly> closedStoreYesterday, Dictionary<Guid, int[]> workedSaturdays, int saturdays)
        {
            var result = new List<SchedulingResult>();
            HashSet<Guid> alreadyAssignedForDay = new();

            DayOfWeek dayOfWeek = demands.Select(d => d.Date)
                .FirstOrDefault().DayOfWeek;

            if (dayOfWeek == DayOfWeek.Saturday) // todo refactor to be more intuitive and not to use firstordefault
            {
                result = schedulingSaturdayAlgorithm.CalculateSaturday(demands, workers, workedSaturdays, saturdays);
                return result;
            }

            foreach (var demand in demands)
            {
                var from30 = demand.From.AddMinutes(30);
                var to30 = demand.To.AddMinutes(-30);
                var matchingWorkers = workers
                    .Where(w => w.From <= from30 && w.To >= to30 && !alreadyAssignedForDay.Contains(w.WorkerId))
                    .ToList();

                if (demand.From <= new TimeOnly(10, 0)) // do not allow to assign workers who closed the day before if demand is early morning
                {
                    var lessThen11Hours = new List<Guid>();
                    foreach (var key in closedStoreYesterday.Keys)
                    {
                        if (closedStoreYesterday[key].AddHours(11) >= demand.From)
                        {
                            lessThen11Hours.Add(key);
                        }
                    }

                    if (lessThen11Hours != null)
                    {
                        matchingWorkers = matchingWorkers
                            .Where(w => !lessThen11Hours.Contains(w.WorkerId))
                            .ToList();
                    }
                }

                matchingWorkers = matchingWorkers
                    .OrderByDescending(w => w.Position == "Customer advisor") // prioritize customer advisors
                    .ThenBy(w => w.Hours)
                    .ThenBy(w => w.To - w.From)
                    .ToList();

                if (matchingWorkers.Any() && demand.From <= new TimeOnly(9, 30)) // try insert VM to the top of list
                {
                    var visualMerchendiser = matchingWorkers
                        .FirstOrDefault(w => w.Position == "Visual merchandiser");

                    if (visualMerchendiser != null)
                    {
                        matchingWorkers.Remove(visualMerchendiser);
                        matchingWorkers.Insert(0, visualMerchendiser);
                    }
                }
                if (matchingWorkers.Any() && (demand.From <= new TimeOnly(9, 30) || demand.To >= new TimeOnly(21, 0))) // move manager to the front of list early morning or late evening
                {
                    var manager = matchingWorkers
                        .OrderBy(w => w.Hours)
                        .FirstOrDefault(w => w.Position == "Manager");

                    if (manager != null)
                    {
                        matchingWorkers.Remove(manager);
                        matchingWorkers.Insert(0, manager);
                    }
                }

                foreach (var worker in matchingWorkers.Take(demand.WorkersNeeded))
                {
                    var localFrom = worker.From;
                    var localTo = worker.To;

                    if (demand.From > worker.From) localFrom = demand.From;
                    if (worker.To > demand.To) localTo = demand.To;

                    alreadyAssignedForDay.Add(worker.WorkerId);

                    result.Add(new SchedulingResult
                    {
                        Date = worker.Date,
                        From = localFrom ?? throw new ArgumentNullException(),
                        To = localTo ?? throw new ArgumentNullException(),
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
