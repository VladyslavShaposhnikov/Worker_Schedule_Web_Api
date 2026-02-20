using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = AppRoles.Worker)]
    public class AvailabilityController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        [Route("month/{year:int}/{month:int}")]
        public async Task<ActionResult<List<GetAvailabilityDto>>> GetMonthAvailability([FromRoute] int year,[FromRoute] int month)
        {
            var worker = await GetWorker();

            if (worker == null)
            {
                return BadRequest("Worker not found");
            }

            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.WorkerId == worker.Id)
                .ToListAsync();

            List<GetAvailabilityDto> resultList = new();

            foreach (var day in availability)
            {
                resultList.Add(new GetAvailabilityDto
                {
                    WorkerInternalNumber = worker.WorkerInternalNumber,
                    WorkerName = $"{worker.FirstName} {worker.LastName}",
                    WorkerPosition = worker.Position?.Name,
                    From = day.WorkingUnit.From,
                    To = day.WorkingUnit.To,
                    Date = day.Date
                });
            }

            return resultList;
        }

        [HttpGet]
        [Route("day/{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> GetAvailability([FromRoute] DateOnly date)
        {
            var worker = await GetWorker();

            if (worker == null)
            {
                return BadRequest("Worker not found");
            }

            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .FirstOrDefaultAsync(a => a.Date == date && a.WorkerId == worker.Id);

            if (availability == null)
            {
                return NotFound();
            }

            var result = new GetAvailabilityDto
            {
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name,
                From = availability.WorkingUnit.From,
                To = availability.WorkingUnit.To,
                Date = availability.Date
            };
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<GetAvailabilityDto>> CreateAvailability(CreateAvailabilityDto form)
        {
            var result = await SetAvailability(form.Date, form.From, form.To);
            if (result == null)
            {
                return BadRequest("Availability already exists or worker not found");
            }
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpPut]
        [Route("{date}")]
        public async Task<ActionResult<GetAvailabilityDto>> UpdateAvailability([FromRoute] DateOnly date, UpdateAvailabilityDto workingUnit)
        {
            var worker = await GetWorker();

            if (worker == null)
            {
                return BadRequest(ModelState);
            }

            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .FirstOrDefaultAsync(a => a.Date == date && a.WorkerId == worker.Id);
            if (availability == null)
            {
                return NotFound();
            }

            var getWorkingUnit = await context.WorkingUnits
                .FirstOrDefaultAsync(wu => wu.From == workingUnit.From && wu.To == workingUnit.To);
            if (getWorkingUnit == null)
            {
                getWorkingUnit = new WorkingUnit
                {
                    From = workingUnit.From,
                    To = workingUnit.To
                };
                context.WorkingUnits.Add(getWorkingUnit);
                await context.SaveChangesAsync();
            }    

            availability.WorkingUnitId = getWorkingUnit.Id;

            context.Availabilities.Update(availability);
            await context.SaveChangesAsync();

            var result = new GetAvailabilityDto
            {
                Date = availability.Date,
                From = getWorkingUnit.From,
                To = getWorkingUnit.To,
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name
            };
            return result;
        }

        [HttpPost]
        [Route("{date}/full")]
        public async Task<ActionResult<GetAvailabilityDto>> SetFullAvailability([FromRoute] DateOnly date)
        {
            var result = await SetAvailability(date, new TimeOnly(0, 0), new TimeOnly(23, 59));
            if (result == null)
            {
                return BadRequest("Availability already exists or worker not found");
            }
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        [HttpPost]
        [Route("{date}/day-off")]
        public async Task<ActionResult<GetAvailabilityDto>> DayOffAvailability([FromRoute] DateOnly date)
        {
            var result = await SetAvailability(date, new TimeOnly(0, 0), new TimeOnly(0, 0)); // or we can just make sure there is no availability for this worker for this day
            if (result == null)
            {
                return BadRequest("Availability already exists or worker not found");
            }
            return CreatedAtAction(nameof(GetAvailability), new { date = result.Date }, result);
        }

        private async Task<GetAvailabilityDto?> SetAvailability(DateOnly date, TimeOnly from, TimeOnly to)
        {
            var worker = await GetWorker();

            if (worker == null)
            {
                return null;
            }

            var isDateExist = await context.Availabilities.AnyAsync(a => a.Date == date && a.WorkerId == worker.Id);
            if (isDateExist)
            {
                return null;
            }

            var workingUnit = await context.WorkingUnits.FirstOrDefaultAsync(wu => wu.From == from && wu.To == to);
            if (workingUnit == null)
            {
                workingUnit = new WorkingUnit
                {
                    From = from,
                    To = to
                };

                context.WorkingUnits.Add(workingUnit);
            }


            var entity = new Availability
            {
                Date = date,
                WorkingUnit = workingUnit,
                WorkerId = worker.Id
            };

            context.Availabilities.Add(entity);
            await context.SaveChangesAsync();

            var result = new GetAvailabilityDto
            {
                Date = date,
                From = from,
                To = to,
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name
            };
            return result;
        }

        private async Task<Worker?> GetWorker()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var worker = await context.Workers
                .Include(w => w.Position)
                .FirstOrDefaultAsync(w => w.AppUserId == userId);
            if (worker == null)
            {
                return null;
            }

            return worker;
        }
    }
}
