using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.DTOs.ManageAvailability
{
    public class ScheduleDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public int WorkerInternalNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
