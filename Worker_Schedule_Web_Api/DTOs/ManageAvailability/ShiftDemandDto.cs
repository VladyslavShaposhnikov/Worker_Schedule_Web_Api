namespace Worker_Schedule_Web_Api.DTOs.ManageAvailability
{
    public class ShiftDemandDto
    {
        public DateOnly Date { get; set; }
        public TimeOnly From { get; set; }
        public TimeOnly To { get; set; }
        public int WorkersNeeded { get; set; }
    }
}
