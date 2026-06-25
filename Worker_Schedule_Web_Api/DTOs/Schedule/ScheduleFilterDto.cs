namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class ScheduleFilterDto
    {
        public Guid? UserId { get; set; } = null;
        public DateOnly? startDate { get; set; } = null;
        public DateOnly? endDate { get; set; } = null;
        public string? workerInternalNumbers { get; set; } = null;
        public string? workerName { get; set; } = null;
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}
