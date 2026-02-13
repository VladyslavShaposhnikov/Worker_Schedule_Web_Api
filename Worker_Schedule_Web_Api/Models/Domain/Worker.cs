using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Worker
    {
        public Guid Id { get; set; }
        public int WorkerInternalNumber { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public Store? Store { get; set; }
        public Guid PositionId { get; set; }
        public Position? Position { get; set; }
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;
        public List<Availability> Availabilities { get; set; } = new();
        public List<Schedule> Schedules { get; set; } = new();
    }
}
