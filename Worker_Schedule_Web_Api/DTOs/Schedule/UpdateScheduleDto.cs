namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class UpdateScheduleDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
    }
}
