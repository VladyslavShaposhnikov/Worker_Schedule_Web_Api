using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Availability
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public Guid WorkingUnitId { get; set; }
        public WorkingUnit? WorkingUnit { get; set; }
        public Guid WorkerId { get; set; }
        public Worker? Worker { get; set; }
    }
}
