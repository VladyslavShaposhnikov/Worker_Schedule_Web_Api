namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class ShiftDemand
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int WorkersNeeded { get; set; }
        public WorkingUnit? WorkingUnit { get; set; }
        public Guid? WorkingUnitId { get; set; }

    }
}
