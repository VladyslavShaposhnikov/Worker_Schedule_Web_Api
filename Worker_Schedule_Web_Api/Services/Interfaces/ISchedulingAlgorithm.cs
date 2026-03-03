using Worker_Schedule_Web_Api.Models.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface ISchedulingAlgorithm
    {
        List<SchedulingResult> Calculate(
        List<SchedulingDemand> demands,
        List<SchedulingWorker> workers,
        HashSet<Guid> alreadyAssignedForDay);
    }
}
