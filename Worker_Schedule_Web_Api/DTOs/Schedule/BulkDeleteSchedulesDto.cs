namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class BulkDeleteSchedulesDto
    {
        public List<DateOnly> Dates { get; set; }
        public List<int> WorkersIds { get; set; }
    }
}
