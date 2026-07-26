using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.DTOs.Schedule
{
    public class SummaryByWorkers
    {
        public Guid Id { get; set; }
        public int WorkerInternalNumber { get; set; }
        public int EmploymentPercentage { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Position { get; set; }
        public double WorkedHours { get; set; }
        public int FullTimeHours { get; set; }
    }
}
