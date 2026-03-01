using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;

namespace Worker_Schedule_Web_Api.Services.Interfaces
{
    public interface IShiftDemandService
    {
        Task<List<ShiftDemandDto>> GetShiftDemand(DateOnly date);
        Task<List<ShiftDemandDto>> CreateShiftDemand(List<ShiftDemandDto> form);
        Task DeleteShiftDemand(DateOnly date);
        Task<List<ShiftDemandDto>> SetDefaultShiftsMonth(int year, int month);
    }
}
