namespace Worker_Schedule_Web_Api.Models.Schedule
{
    public class SchedulingWorker
    {
        public DateOnly Date { get; set; }
        public TimeOnly? From { get; set; }
        public TimeOnly? To { get; set; }
        public double Hours { get; set; }
        public int WorkerInternalNumber { get; set; }
        public Guid WorkerId { get; set; }
        public string? FullName { get; set; }
        public string? Position { get; set; }
    }
}
