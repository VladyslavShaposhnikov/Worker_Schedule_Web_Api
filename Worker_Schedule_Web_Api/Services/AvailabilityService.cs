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
        public async Task<List<GetAvailabilityDto>> Availabilities(AvailabilityFilterDto filter)
        {
            var query = context.Availabilities
                .Include(a => a.Worker)
                .ThenInclude(w => w.Position)
                .Include(a => a.WorkingUnit)
                .AsQueryable();

            if (filter.startDate != null)
            {
                query = query.Where(a => a.Date >= filter.startDate);
            }
            if (filter.endDate != null)
            {
                query = query.Where(a => a.Date <= filter.endDate);
            }

            if (filter.workerInternalNumber != null)
            {
                query = query.Where(a => a.Worker.WorkerInternalNumber == filter.workerInternalNumber);
            }

            if (filter.workerPosition != null)
            {
                query = query.Where(a => a.Worker.Position.Name.ToLower().Contains(filter.workerPosition.ToLower()));
            }

            if (filter.workerName != null)
            {
                query = query.Where(a =>
                a.Worker.FirstName.ToLower().Contains(filter.workerName.ToLower()) ||
                a.Worker.LastName.ToLower().Contains(filter.workerName.ToLower())
                );
            }

            if (filter.from != null)
            {
                query = query.Where(a => a.WorkingUnit.From >= filter.from);
            }
            if (filter.to != null)
            {
                query = query.Where(a => a.WorkingUnit.To <= filter.to);
            }

            return await query.Select(a =>
                new GetAvailabilityDto
                {
                    WorkerName = $"{a.Worker.FirstName} {a.Worker.LastName}",
                    WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                    WorkerPosition = a.Worker.Position.Name,
                    Date = a.Date,
                    From = a.WorkingUnit.From,
                    To = a.WorkingUnit.To
                })
                .OrderBy(a => a.Date)
                .ThenBy(a => a.WorkerInternalNumber)
                .Skip((filter.page - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .ToListAsync();
        }
        public async Task<GetAvailabilityDto> CreateAvailability(CreateUpdateAvailabilityDto form)
        {
            var worker = await GetWorker();
            var workingUnits = await context.WorkingUnits.ToListAsync();
            var isDateExist = await context.Availabilities.AnyAsync(a => a.Date == form.Date && a.WorkerId == worker.Id);

            if (isDateExist) throw new DateAlreadyExistsException(form.Date);

            var result = SetAvailability(worker, form.Date, form.From, form.To, workingUnits);
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

        public async Task<GetAvailabilityDto> SetFullAvailability(DateOnly date)
        {
            var worker = await GetWorker();
            var workingUnits = await context.WorkingUnits.ToListAsync();

            var isDateExist = await context.Availabilities.AnyAsync(a => a.Date == date && a.WorkerId == worker.Id);
            if (isDateExist) throw new DateAlreadyExistsException(date);

            var result = SetAvailability(worker, date, new TimeOnly(0, 0), new TimeOnly(23, 59), workingUnits);
            await context.SaveChangesAsync();
            return result;
        }

        public async Task<GetAvailabilityDto> UpdateAvailability(CreateUpdateAvailabilityDto form)
        {
            var worker = await GetWorker();
            var workingUnits = await context.WorkingUnits.ToListAsync();
            var availabilities = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Where(a => a.WorkerId == worker.Id)
                .ToListAsync();
            var result = UpdateAvailabilityPrivate(worker, workingUnits, availabilities, form);
            await context.SaveChangesAsync();
            return result;
        }

        public async Task<List<GetAvailabilityDto>> CreateMonthAvailability(CreateUpdateAvailabilityDto[] form, int year, int month)
        {
            var result = new List<GetAvailabilityDto>();
            var worker = await GetWorker();
            var workingUnits = await context.WorkingUnits.ToListAsync();

            var existingDates = await context.Availabilities
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.WorkerId == worker.Id)
                .Select(a => a.Date)
                .ToListAsync();


            foreach (var availability in form)
            {
                if (availability.Date.Year != year || availability.Date.Month != month) 
                    throw new InvalidAvailabilityDateException(availability.Date);
                if (existingDates.Contains(availability.Date)) 
                    throw new DateAlreadyExistsException(availability.Date);

                result.Add(SetAvailability(worker, availability.Date, availability.From, availability.To, workingUnits));
            }

            await context.SaveChangesAsync();

            return result;
        }

        public async Task<List<GetAvailabilityDto>> UpdateMonthAvailability(CreateUpdateAvailabilityDto[] form, int year, int month)
        {
            var worker = await GetWorker();
            var workingUnits = await context.WorkingUnits.ToListAsync();
            var availabilities = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Where(a => a.Date.Year == year && a.Date.Month == month && a.WorkerId == worker.Id)
                .ToListAsync();
            var result = new List<GetAvailabilityDto>();
            foreach (var item in form)
            {
                if (item.Date.Year != year || item.Date.Month != month) throw new InvalidAvailabilityDateException(item.Date);
                var updated = UpdateAvailabilityPrivate(worker, workingUnits, availabilities, item);

                result.Add(updated);
            }
            await context.SaveChangesAsync();
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

        private GetAvailabilityDto SetAvailability(Worker worker, DateOnly date, TimeOnly from, TimeOnly to, List<WorkingUnit> workingUnits)
        {
            if (from >= to) throw new InvalidWorkingHoursException();

            var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, from, to);

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

        private GetAvailabilityDto UpdateAvailabilityPrivate(Worker worker, List<WorkingUnit> workingUnits, List<Availability> availabilities, CreateUpdateAvailabilityDto form)
        {
            if (form.From >= form.To) throw new InvalidWorkingHoursException();
            var availability = availabilities
                .FirstOrDefault(a => a.Date == form.Date && a.WorkerId == worker.Id);
            if (availability == null)
            {
                throw new AvailabilityNotFoundException();
            }

            var getWorkingUnit = CreateWorkingUnitIfNotExists(workingUnits, form.From, form.To);

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

        private WorkingUnit CreateWorkingUnitIfNotExists(List<WorkingUnit> workingUnits, TimeOnly from, TimeOnly to)
        {
            var workingUnit = workingUnits.FirstOrDefault(wu => wu.From == from && wu.To == to);
            if (workingUnit == null)
            {
                workingUnit = new WorkingUnit
                {
                    From = from,
                    To = to
                };
                workingUnits.Add(workingUnit);
                context.WorkingUnits.Add(workingUnit);
            }
            return workingUnit;
        }
    }
}
