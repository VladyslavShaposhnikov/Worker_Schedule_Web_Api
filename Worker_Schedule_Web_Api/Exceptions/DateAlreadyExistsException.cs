namespace Worker_Schedule_Web_Api.Exceptions
{
    public class DateAlreadyExistsException : DomainException
    {
        public DateAlreadyExistsException(DateOnly date)
            : base($"Date {date} already exists", 
                  StatusCodes.Status409Conflict)
        {
        }
    }
}
