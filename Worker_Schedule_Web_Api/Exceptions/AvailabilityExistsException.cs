namespace Worker_Schedule_Web_Api.Exceptions
{
    public class AvailabilityExistsException : DomainException
    {
        public AvailabilityExistsException(DateOnly date)
            : base($"Availability for {date} is already exists", 
                  StatusCodes.Status409Conflict)
        {
        }
    }
}
