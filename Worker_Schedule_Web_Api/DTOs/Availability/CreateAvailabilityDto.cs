namespace Worker_Schedule_Web_Api.DTOs.Availability
{
    public class CreateAvailabilityDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }
}
