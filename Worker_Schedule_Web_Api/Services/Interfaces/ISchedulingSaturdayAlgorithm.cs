using Worker_Schedule_Web_Api.Models.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface ISchedulingSaturdayAlgorithm
    {
        List<SchedulingResult> CalculateSaturday(
            List<SchedulingDemand> demands, 
            List<SchedulingWorker> workers, 
            Dictionary<Guid, int[]> workedSaturdays, 
            int saturdays);
    }
}
