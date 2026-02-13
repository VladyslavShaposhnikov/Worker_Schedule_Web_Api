namespace Worker_Schedule_Web_Api.Models.Domain
{
    public class Address
    {
        public Guid Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string BuildingNumber { get; set; } = string.Empty;
        public string? Apartment { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
    }
}
