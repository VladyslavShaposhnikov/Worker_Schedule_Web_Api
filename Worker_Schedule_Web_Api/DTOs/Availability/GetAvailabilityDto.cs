namespace Worker_Schedule_Web_Api.DTOs.Availability
{
    public class GetAvailabilityDto
    {
        public string WorkerName { get; set; }
        public int WorkerInternalNumber { get; set; }
        public string WorkerPosition { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }
}
