namespace Worker_Schedule_Web_Api.Exceptions
{
    public class WorkerNotFoundException : DomainException
    {
        public WorkerNotFoundException(int internalNumber) : base($"Worker with internal number {internalNumber} not found.", StatusCodes.Status404NotFound)
        {
        }
    }
}
