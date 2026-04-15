namespace Worker_Schedule_Web_Api.Models.Schedule
{
    public class SchedulingDemand
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public int WorkersNeeded { get; set; }
    }
}
