namespace Worker_Schedule_Web_Api.Models.Schedule
{
    public class SchedulingDemand
    {
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public int WorkersNeeded { get; set; }
    }
}
