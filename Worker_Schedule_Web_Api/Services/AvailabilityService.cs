using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.Exceptions;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class AvailabilityService(AppDbContext context, ICurrentUserService currentUser) : IAvailabilityService
    {
        public async Task<GetAvailabilityDto> CreateAvailability(CreateUpdateAvailabilityDto form)
        {
            var worker = await GetWorker();
            var isDateExist = await context.Availabilities.AnyAsync(a => a.Date == form.Date && a.WorkerId == worker.Id);

            if (isDateExist) throw new AvailabilityExistsException(form.Date);

            var result = await SetAvailability(worker, form.Date, form.From, form.To);
            await context.SaveChangesAsync();
            return result;
        }

        public async Task DayOffAvailability(DateOnly date)
        {
            var worker = await GetWorker();
            var availability = await context.Availabilities.FirstOrDefaultAsync(a => a.Date == date && a.WorkerId == worker.Id);
            if (availability != null)
            {
                context.Remove(availability);
                await context.SaveChangesAsync();
            }
        }

        public async Task<GetAvailabilityDto> GetAvailability(DateOnly date)
        {
            var worker = await GetWorker();
            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .FirstOrDefaultAsync(a => a.Date == date && a.WorkerId == worker.Id);

            if (availability == null)
            {
                throw new AvailabilityNotFoundException();
            }

            var result = new GetAvailabilityDto
            {
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name ?? "not specified",
                From = availability.WorkingUnit.From,
                To = availability.WorkingUnit.To,
                Date = availability.Date
            };

            return result;
        }

        public async Task<List<GetAvailabilityDto>> GetMonthAvailability(int year, int month)
        {
            var worker = await GetWorker();
            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.WorkerId == worker.Id)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var resultList = availability.Select(a => new GetAvailabilityDto
            {
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name,
                From = a.WorkingUnit.From,
                To = a.WorkingUnit.To,
                Date = a.Date
            }).ToList();

            return resultList;
        }

        public async Task<GetAvailabilityDto> SetFullAvailability(DateOnly date)
        {
            var worker = await GetWorker();

            var isDateExist = await context.Availabilities.AnyAsync(a => a.Date == date && a.WorkerId == worker.Id);
            if (isDateExist) throw new AvailabilityExistsException(date);

            var result = await SetAvailability(worker, date, new TimeOnly(0, 0), new TimeOnly(23, 59));
            await context.SaveChangesAsync();
            return result;
        }

        public async Task<GetAvailabilityDto> UpdateAvailability(CreateUpdateAvailabilityDto form)
        {
            var worker = await GetWorker();
            var result = await UpdateAvailabilityPrivate(worker, form);
            await context.SaveChangesAsync();
            return result;
        }

        private async Task<GetAvailabilityDto> SetAvailability(Worker worker, DateOnly date, TimeOnly from, TimeOnly to)
        {
            if (from >= to) throw new InvalidWorkingHoursException();

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

            var result = new GetAvailabilityDto
            {
                Date = date,
                From = workingUnit.From,
                To = workingUnit.To,
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name ?? "not specified"
            };

            return result;
        }

        private async Task<Worker> GetWorker()
        {
            var userId = currentUser.UserId ?? throw new UnauthorizedDomainException();

            var worker = await context.Workers
                .Include(w => w.Position)
                .FirstOrDefaultAsync(w => w.AppUserId == userId) ?? throw new UnauthorizedDomainException();

            return worker;
        }

        public async Task<List<GetAvailabilityDto>> CreateMonthAvailability(CreateUpdateAvailabilityDto[] form, int year, int month)
        {
            var result = new List<GetAvailabilityDto>();
            var worker = await GetWorker();

            var existingDates = await context.Availabilities
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.WorkerId == worker.Id)
                .Select(a => a.Date)
                .ToListAsync();


            foreach (var availability in form)
            {
                if (availability.Date.Year != year || availability.Date.Month != month) 
                    throw new InvalidAvailabilityDateException(availability.Date);
                if (existingDates.Contains(availability.Date)) 
                    throw new AvailabilityExistsException(availability.Date);

                result.Add(await SetAvailability(worker, availability.Date, availability.From, availability.To));
            }

            await context.SaveChangesAsync();

            return result;
        }

        public async Task<List<GetAvailabilityDto>> UpdateMonthAvailability(CreateUpdateAvailabilityDto[] form, int year, int month)
        {
            var worker = await GetWorker();
            var result = new List<GetAvailabilityDto>();
            foreach (var item in form)
            {
                if (item.Date.Year != year || item.Date.Month != month) throw new InvalidAvailabilityDateException(item.Date);
                var updated = await UpdateAvailabilityPrivate(worker, item);
                result.Add(updated);
            }
            await context.SaveChangesAsync();
            return result;
        }

        private async Task<GetAvailabilityDto> UpdateAvailabilityPrivate(Worker worker, CreateUpdateAvailabilityDto form)
        {
            var availability = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .FirstOrDefaultAsync(a => a.Date == form.Date && a.WorkerId == worker.Id);
            if (availability == null)
            {
                throw new AvailabilityNotFoundException();
            }

            var getWorkingUnit = await context.WorkingUnits
                .FirstOrDefaultAsync(wu => wu.From == form.From && wu.To == form.To);
            if (getWorkingUnit == null)
            {
                getWorkingUnit = new WorkingUnit
                {
                    From = form.From,
                    To = form.To
                };
                context.WorkingUnits.Add(getWorkingUnit);
            }

            availability.WorkingUnit = getWorkingUnit;

            var result = new GetAvailabilityDto
            {
                Date = availability.Date,
                From = getWorkingUnit.From,
                To = getWorkingUnit.To,
                WorkerInternalNumber = worker.WorkerInternalNumber,
                WorkerName = $"{worker.FirstName} {worker.LastName}",
                WorkerPosition = worker.Position?.Name ?? "not specified"
            };
            return result;
        }
    }
}
