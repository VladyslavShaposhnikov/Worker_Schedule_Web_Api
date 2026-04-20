using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Worker_Schedule_Web_Api.DTOs.ShiftDemand;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("api/shift-demands")]
    [Authorize(Roles = $"{AppRoles.Manager},{AppRoles.VisualMerchandiser}")]
    public class ShiftDemandController(IShiftDemandService shiftDemandService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the list of shift demands for a specific date.
        /// </summary>
        [HttpGet]
        [Route("{date}")]
        public async Task<ActionResult<List<ShiftDemandDto>>> GetShiftDemand([FromRoute] DateOnly date)
        {
            var result = await shiftDemandService.GetShiftDemand(date);
            return result;
        }

        /// <summary>
        /// Retrieves the list of month shift demands for a specific date.
        /// </summary>
        [HttpGet]
        [Route("{year:int}/{month:int}")]
        public async Task<ActionResult<List<ShiftDemandDto>>> GetMonthShiftDemand([FromRoute] int year, [FromRoute] int month)
        {
            var result = await shiftDemandService.GetMonthShiftDemand(year, month);
            return result;
        }

        [HttpPatch]
        [Route("{id:Guid}/increase")]
        public async Task<ActionResult<List<ShiftDemandDto>>> IncreaseShiftDemandWorker([FromRoute] Guid id)
        {
            await shiftDemandService.IncreaseShiftDemandWorker(id);
            return Ok();
        }

        [HttpPatch]
        [Route("{id:Guid}/decrease")]
        public async Task<ActionResult<List<ShiftDemandDto>>> DecreaseShiftDemandWorker([FromRoute] Guid id)
        {
            await shiftDemandService.DecreaseShiftDemandWorker(id);
            return Ok();
        }

        /// <summary>
        /// Creates/add single shift demands.
        /// </summary>
        [HttpPost]
        [Route("single")]
        public async Task<ActionResult<List<ShiftDemandDto>>> CreateSingleShiftDemand(ShiftDemandDto form)
        {
            var result = await shiftDemandService.CreateSingleShiftDemand(form);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Batch creates/add shift demands.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<List<ShiftDemandDto>>> CreateShiftDemand(List<ShiftDemandDto> form)
        {
            var result = await shiftDemandService.CreateShiftDemand(form);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Batch creates default shift demands for a specific month. The default shift demands are based on the default shift template and are created for each day of the specified month.
        /// </summary>
        [HttpPost]
        [Route("defaults/{year:int}/{month:int}")]
        public async Task<ActionResult<List<ShiftDemandDto>>> SetDefaultShiftsMonth([FromRoute] int year, [FromRoute] int month)
        {
            var result = await shiftDemandService.SetDefaultShiftsMonth(year, month);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Deletes all shift demands for a specific date.
        /// </summary>
        [HttpDelete]
        [Route("{date}")]
        public async Task<ActionResult> DeleteShiftDemand([FromRoute] DateOnly date)
        {
            await shiftDemandService.DeleteShiftDemand(date);
            return NoContent();
        }

        /// <summary>
        /// Deletes all shift demands by Id.
        /// </summary>
        [HttpDelete]
        [Route("{id}/single")]
        public async Task<ActionResult> DeleteShiftDemandById([FromRoute] Guid id)
        {
            await shiftDemandService.DeleteShiftDemandById(id);
            return NoContent();
        }
    }
}
