using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.ShiftDemand;
using Worker_Schedule_Web_Api.Exceptions;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class ShiftDemandService(AppDbContext context) : IShiftDemandService
    {
        public async Task<List<ShiftDemandDto>> GetShiftDemand(DateOnly date)
        {
            return await context.ShiftDemands
                .Where(sd => sd.Date == date)
                .Select(sd => new ShiftDemandDto
                {
                    ShiftDemandId = sd.Id,
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();
        }

        public async Task IncreaseShiftDemandWorker(Guid id)
        {
            var shiftDemand = await context.ShiftDemands
                .FindAsync(id);

            if (shiftDemand != null)
            {
                shiftDemand.WorkersNeeded++;
                await context.SaveChangesAsync();
            }
        }

        public async Task DecreaseShiftDemandWorker(Guid id)
        {
            var shiftDemand = await context.ShiftDemands
                .FindAsync(id);

            if (shiftDemand != null)
            {
                if (shiftDemand.WorkersNeeded > 0)
                {
                    shiftDemand.WorkersNeeded--;
                    await context.SaveChangesAsync();
                }
                if (shiftDemand.WorkersNeeded == 0)
                {
                    context.ShiftDemands.Remove(shiftDemand);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<ShiftDemandDto>> GetMonthShiftDemand(int year, int month)
        {
            return await context.ShiftDemands
                .Include(sd => sd.WorkingUnit)
                .Where(sd => sd.Date.Year == year && sd.Date.Month == month)
                .Select(sd => new ShiftDemandDto
                {
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded,
                    ShiftDemandId = sd.Id
                })
                .ToListAsync();
        }

        public async Task<List<ShiftDemandDto>> CreateShiftDemand(List<ShiftDemandDto> form)
        {
            var result = new List<ShiftDemandDto>();

            var units = await context.WorkingUnits.ToListAsync();

            foreach (var formItem in form)
            {
                var workingUnit = CreateWorkingUnitIfNotExists(units, formItem.From, formItem.To);
                Guid shiftDemandId = Guid.NewGuid();

                context.ShiftDemands.Add(new ShiftDemand
                {
                    Id = shiftDemandId,
                    Date = formItem.Date,
                    WorkingUnit = workingUnit,
                    WorkersNeeded = formItem.WorkersNeeded
                });
                result.Add(new ShiftDemandDto
                    {
                        ShiftDemandId = shiftDemandId,
                        Date = formItem.Date,
                        From = formItem.From,
                        To = formItem.To,
                        WorkersNeeded = formItem.WorkersNeeded
                    });
            }
            await context.SaveChangesAsync();

            return result;
        }

        public async Task<ShiftDemandDto> CreateSingleShiftDemand(ShiftDemandDto form)
        {
            var units = await context.WorkingUnits.ToListAsync();

            var workingUnit = CreateWorkingUnitIfNotExists(units, form.From, form.To);

            Guid shiftDemandId = Guid.NewGuid();

            var shiftDemand = new ShiftDemand
            {
                Id = shiftDemandId,
                Date = form.Date,
                WorkingUnit = workingUnit,
                WorkersNeeded = form.WorkersNeeded
            };

            context.ShiftDemands.Add(shiftDemand);
            await context.SaveChangesAsync();

            return new ShiftDemandDto
            {
                ShiftDemandId = shiftDemandId,
                Date = form.Date,
                From = form.From,
                To = form.To,
                WorkersNeeded = form.WorkersNeeded
            };
        }

        public async Task DeleteShiftDemand(DateOnly date)
        {
            await context.ShiftDemands.Where(sd => sd.Date == date).ExecuteDeleteAsync();
        }

        public async Task DeleteShiftDemandById(Guid id)
        {
            await context.ShiftDemands.Where(sd => sd.Id == id).ExecuteDeleteAsync();
        }

        public async Task DeleteShiftDemandsByDays(List<DateOnly> days)
        {
            await context.ShiftDemands.Where(sd => days.Contains(sd.Date)).ExecuteDeleteAsync();
        }

        public async Task<List<ShiftDemandDto>> SetDefaultShiftsMonth(int year, int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var haveShiftDemandList = await context
                .ShiftDemands
                .Where(sd => sd.Date.Year == year && sd.Date.Month == month)
                .Select(sd => sd.Date)
                .ToHashSetAsync();

            var units = await context.WorkingUnits.ToListAsync();

            foreach (var day in Enumerable.Range(1, DateTime.DaysInMonth(year, month)))
            {
                var date = new DateOnly(year, month, day);

                if (haveShiftDemandList.Contains(date))
                {
                    continue; // Skip if shift demand already exists for this date
                }

                if (date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Thursday)
                {
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(6, 0), new TimeOnly(14, 0)),
                        WorkersNeeded = 3
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(14, 0), new TimeOnly(21, 30)),
                        WorkersNeeded = 2
                    });
                }
                else if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(9, 30), new TimeOnly(17, 30)),
                        WorkersNeeded = 1
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(9, 30), new TimeOnly(14, 30)),
                        WorkersNeeded = 1
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(12, 0), new TimeOnly(20, 0)),
                        WorkersNeeded = 2
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(14, 0), new TimeOnly(21, 30)),
                        WorkersNeeded = 3
                    });
                }
                else if (date.DayOfWeek == DayOfWeek.Friday)
                {
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(9, 30), new TimeOnly(17, 30)),
                        WorkersNeeded = 2
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(14, 0), new TimeOnly(21, 30)),
                        WorkersNeeded = 3
                    });
                }
                else
                {
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(9, 30), new TimeOnly(17, 30)),
                        WorkersNeeded = 2
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(17, 30), new TimeOnly(21, 30)),
                        WorkersNeeded = 1
                    });
                    context.ShiftDemands.Add(new ShiftDemand
                    {
                        Id = Guid.NewGuid(),
                        Date = date,
                        WorkingUnit = CreateWorkingUnitIfNotExists(units, new TimeOnly(14, 30), new TimeOnly(21, 30)),
                        WorkersNeeded = 1
                    });
                }
            }
            await context.SaveChangesAsync();

            return await context.ShiftDemands
                .Where(sd => sd.Date >= startDate && sd.Date <= endDate)
                .Select(sd => new ShiftDemandDto
                {
                    ShiftDemandId = sd.Id,
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();
        }

        private WorkingUnit CreateWorkingUnitIfNotExists(List<WorkingUnit> units, TimeOnly from, TimeOnly to)
        {
            var workingUnit = units.FirstOrDefault(wu => wu.From == from && wu.To == to);
            if (workingUnit == null)
            {
                workingUnit = new WorkingUnit
                {
                    From = from,
                    To = to
                };
                context.WorkingUnits.Add(workingUnit);
                units.Add(workingUnit);
            }

            return workingUnit;
        }

        public async Task<List<ShiftDemandDto>> GetAllShiftDemands()
        {
            return await context.ShiftDemands
                .Select(sd => new ShiftDemandDto
                {
                    ShiftDemandId = sd.Id,
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();
        }
    }
}
