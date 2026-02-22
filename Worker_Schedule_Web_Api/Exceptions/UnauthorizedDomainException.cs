namespace Worker_Schedule_Web_Api.Exceptions
{
    public class UnauthorizedDomainException : DomainException
    {
        public UnauthorizedDomainException()
            :base("Unauthorized access",
                 StatusCodes.Status401Unauthorized)
        {
        }
    }
}
