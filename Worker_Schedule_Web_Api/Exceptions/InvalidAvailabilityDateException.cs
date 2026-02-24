namespace Worker_Schedule_Web_Api.Exceptions
{
    public class InvalidAvailabilityDateException : DomainException
    {
        public InvalidAvailabilityDateException(DateOnly date) 
            : base($"Date {date} is not from the current month", 
                  StatusCodes.Status400BadRequest)
        {
        }
    }
}
