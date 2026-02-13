namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid AddressId { get; set; }
        public Address? Address { get; set; }
        public List<Worker> Workers { get; set; } = new();
    }
}
