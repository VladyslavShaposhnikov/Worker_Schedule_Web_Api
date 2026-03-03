namespace Worker_Schedule_Web_Api.Models.Schedule
{
    public class SchedulingResult
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public int WorkerInternalNumber { get; set; }
        public string FullName { get; set; }
        public Guid WorkerId { get; set; }
    }
}
