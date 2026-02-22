namespace Worker_Schedule_Web_Api.Exceptions
{
    public abstract class DomainException : Exception
    {
        public int StatusCode { get; }
        protected DomainException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
