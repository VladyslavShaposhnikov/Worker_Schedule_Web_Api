namespace Worker_Schedule_Web_Api.DTOs.Availability
{
    public class AvailabilityFilterDto
    {
        public DateOnly? startDate { get; set; } = null;
        public DateOnly? endDate { get; set; } = null;
        public int? workerInternalNumber { get; set; } = null;
        public string? workerPosition { get; set; } = null;
        public string? workerName { get; set; } = null;
        public TimeOnly? from { get; set; } = null;
        public TimeOnly? to { get; set; } = null;
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}
