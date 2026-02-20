namespace Worker_Schedule_Web_Api.DTOs.Availability
{
    public class UpdateAvailabilityDto
    {
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }
}
