using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IDaysOffOptimizer
    {
        void Fix(
            int year, 
            int month, 
            List<Guid> fullShiftWorkers, 
            List<SchedulingResult> result, 
            List<Availability> workers, 
            Dictionary<Guid, double> hoursSum,
            int daysOffRequired);
    }
}
