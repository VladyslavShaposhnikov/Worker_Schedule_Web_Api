namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Position
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Worker> Workers { get; set; } = new();
    }
}
