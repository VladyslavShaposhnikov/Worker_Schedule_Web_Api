using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Models.Domain;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IAvailabilityService
    {
        Task<List<GetAvailabilityDto>> GetMonthAvailability(int year, int month);
        Task<GetAvailabilityDto?> GetAvailability(DateOnly date);
        Task<GetAvailabilityDto> CreateAvailability(CreateAvailabilityDto form);
        Task<GetAvailabilityDto> UpdateAvailability(DateOnly date, UpdateAvailabilityDto workingUnit);
        Task<GetAvailabilityDto> SetFullAvailability(DateOnly date);
        Task DayOffAvailability(DateOnly date);
    }
}
