namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class WorkingUnit
    {
        public Guid Id { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public List<Schedule> Schedules { get; set; } = new();
        public List<Availability> Availabilities { get; set; } = new();
    }
}
