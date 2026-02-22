namespace Worker_Schedule_Web_Api.Exceptions
{
    public class AvailabilityNotFoundException : DomainException
    {
        public AvailabilityNotFoundException() 
            : base("Availability was not found",
                  StatusCodes.Status404NotFound)
        {
        }
    }
}
