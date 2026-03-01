namespace Worker_Schedule_Web_Api.Exceptions
{
    public class MonthAlreadyHasShiftDemandsException : DomainException
    {
        public MonthAlreadyHasShiftDemandsException() 
            : base("Month already has at least 1 shift demand", StatusCodes.Status409Conflict)
        {
        }
    }
}
