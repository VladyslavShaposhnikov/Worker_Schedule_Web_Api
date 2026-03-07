namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class ScheduleFilterDto
    {
        public DateOnly? startDate { get; set; } = null;
        public DateOnly? endDate { get; set; } = null;
        public int? workerInternalNumber { get; set; } = null;
        public string? workerName { get; set; } = null;
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}
