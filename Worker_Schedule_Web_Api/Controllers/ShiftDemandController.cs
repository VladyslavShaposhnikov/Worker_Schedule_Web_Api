using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.ManageAvailability;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ShiftDemandController(IShiftDemandService shiftDemandService) : ControllerBase
    {
        [HttpGet]
        [Route("shift-demand")]
        public async Task<ActionResult<List<ShiftDemandDto>>> GetShiftDemand(DateOnly date)
        {
            var result = await shiftDemandService.GetShiftDemand(date);
            return result;
        }

        [HttpPost]
        [Route("create-shift-demand")]
        public async Task<ActionResult<List<ShiftDemandDto>>> CreateShiftDemand(List<ShiftDemandDto> form)
        {
            var result = await shiftDemandService.CreateShiftDemand(form);
            return StatusCode(201, result);
        }

        [HttpPost]
        [Route("default-month/{year:int}/{month:int}")]
        public async Task<ActionResult<List<ShiftDemandDto>>> SetDefaultShiftsMonth(int year, int month)
        {
            var result = await shiftDemandService.SetDefaultShiftsMonth(year, month);
            return StatusCode(201, result);
        }

        [HttpDelete]
        [Route("delete-shift-demand")]
        public async Task<ActionResult> DeleteShiftDemand(DateOnly date)
        {
            await shiftDemandService.DeleteShiftDemand(date);
            return NoContent();
        }
    }
}
