namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class WeeklyBreakIssuesDto
    {
        public Guid WorkerId { get; set; }
        public int WorkerInternalNumber { get; set; }
        public string WorkerName { get; set; }
        public DateTime BreakStart { get; set; }
        public DateTime BreakEnd { get; set; }
        public TimeSpan BreakDuration { get; set; }
    }
}
