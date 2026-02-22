namespace Worker_Schedule_Web_Api.Exceptions
{
    public class InvalidWorkingHoursException : DomainException
    {
        public InvalidWorkingHoursException() 
            : base("Provided values wasn't correct",
                  StatusCodes.Status400BadRequest)
        {
        }
    }
}
