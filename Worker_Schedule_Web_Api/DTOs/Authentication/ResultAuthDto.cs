namespace Worker_Schedule_Web_Api.DTOs.Authentication
{
    public class ResultAuthDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
