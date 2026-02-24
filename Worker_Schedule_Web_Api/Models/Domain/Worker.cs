using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Worker
    {
        public Guid Id { get; set; }
        public int WorkerInternalNumber { get; set; }
        public int EmploymentPercentage { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? StoreId { get; set; } = 16614;
        public Store? Store { get; set; }
        public Guid? PositionId { get; set; } = new Guid("65D8D1C7-4DED-44AC-810B-56917F75286E");
        public Position? Position { get; set; }
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
        public List<Availability> Availabilities { get; set; } = new();
        public List<Schedule> Schedules { get; set; } = new();
    }
}
