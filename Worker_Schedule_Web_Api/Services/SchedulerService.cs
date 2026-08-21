using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Availability;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Exceptions;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Schedule;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class SchedulerService(
        AppDbContext context, 
        ISchedulingAlgorithm schedulingAlgorithm, 
        IScheduleMonthAlgorithm scheduleMonthAlgorithm,
        IWeeklyBreakOptimizer weeklyBreakOptimizer,
        IConfiguration configuration,
        ILogger<SchedulerService> _logger) : IScheduler
    {
        public async Task<List<ScheduleDto>> CreateDaySchedule(DateOnly date)
        {
            var schedules = await context
                .Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date.Year == date.Year && s.Date.Month == date.Month)
                .ToListAsync();

            var workedToday = schedules
                .Where(s => s.Date == date)
                .Select(s => s.WorkerId)
                .ToList();

            var hoursSum = schedules
                .GroupBy(s => s.WorkerId)
                .ToDictionary(d => d.Key, d => d.Sum(s => (s.WorkingUnit.To - s.WorkingUnit.From).TotalHours));

            var demands = await context.ShiftDemands
                .Where(sd => sd.Date == date)
                .Select(sd => new SchedulingDemand
                {
                    Date = sd.Date,
                    From = sd.WorkingUnit.From,
                    To = sd.WorkingUnit.To,
                    WorkersNeeded = sd.WorkersNeeded
                })
                .ToListAsync();

            int monthWorkerHours = configuration.GetValue<int>("MonthWorkerHours", 168);

            var workers = await context.Availabilities
                .Where(a => a.Date == date && !workedToday.Contains(a.WorkerId))
                .Select(a => new SchedulingWorker
                {
                    Date = a.Date,
                    From = a.WorkingUnit.From,
                    To = a.WorkingUnit.To,
                    Hours = hoursSum.GetValueOrDefault(a.WorkerId, 0) / (monthWorkerHours * (a.Worker.EmploymentPercentage / 100)),
                    WorkerInternalNumber = a.Worker.WorkerInternalNumber,
                    WorkerId = a.WorkerId,
                    FullName = $"{a.Worker.FirstName} {a.Worker.LastName}",
                    Position = a.Worker.Position.Name,
                    EmploymentPercentage = a.Worker.EmploymentPercentage
                })
                .ToListAsync();

            var workingUnits = await context.WorkingUnits.ToListAsync();

            var result = new List<ScheduleDto>();

            var workedYesterdayEvening = await context.Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date == date.AddDays(-1) && s.WorkingUnit.To >= new TimeOnly(20, 0))
                .ToDictionaryAsync(k => k.WorkerId, v => v.WorkingUnit.To);

            var workedSaturdays = new Dictionary<Guid, int[]>();

            foreach (var worker in workers)
            {
                int firstShift = schedules
                    .Where(w => w.WorkerId == worker.WorkerId
                        && w.Date.DayOfWeek == DayOfWeek.Saturday
                        && w.WorkingUnit.From <= new TimeOnly(9, 30))
                    .Count();

                int secondShift = schedules
                    .Where(w => w.WorkerId == worker.WorkerId
                        && w.Date.DayOfWeek == DayOfWeek.Saturday
                        && w.WorkingUnit.From >= new TimeOnly(12, 0)
                        && w.WorkingUnit.To <= new TimeOnly(20, 0))
                    .Count();

                int thirdShift = schedules
                    .Where(w => w.WorkerId == worker.WorkerId
                        && w.Date.DayOfWeek == DayOfWeek.Saturday
                        && w.WorkingUnit.To >= new TimeOnly(21, 30))
                    .Count();

                workedSaturdays[worker.WorkerId] = new int[3] { firstShift, secondShift, thirdShift };
            }

            int saturdays = GetSaturdaysNumberInMonth(date.Year, date.Month);

            var calculationResult = schedulingAlgorithm.Calculate(demands, workers, workedYesterdayEvening, workedSaturdays, saturdays);

            foreach (var schedule in calculationResult)
            {
                var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, schedule.From, schedule.To);

                context.Schedules.Add(new Schedule
                {
                    Date = schedule.Date,
                    WorkerId = schedule.WorkerId,
                    WorkingUnitId = workingUnit.Id
                });

                var resultSchedule = new ScheduleDto
                {
                    Date = schedule.Date,
                    From = workingUnit.From,
                    To = workingUnit.To,
                    WorkerInternalNumber = schedule.WorkerInternalNumber,
                    FullName = schedule.FullName
                };

                result.Add(resultSchedule);
            }

            await context.SaveChangesAsync();
            return result;
        }

        public async Task<List<ScheduleDto>> CreateMonthSchedule(int year, int month)
        {
            var schedules = await context
                .Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date.Year == year && s.Date.Month == month || s.Date == new DateOnly(year, month, 1).AddDays(-1))
                .ToListAsync();

            var demands = await context.ShiftDemands
                .Where(sd => sd.Date.Year == year && sd.Date.Month == month)
                .ToListAsync();

            var workers = await context.Availabilities
                .Include(a => a.WorkingUnit)
                .Include(a => a.Worker)
                .ThenInclude(w => w.Position)
                .Where(a => a.Date.Year == year && a.Date.Month == month)
                .ToListAsync();

            var workingUnits = await context.WorkingUnits.ToListAsync();

            List<Guid> fullShiftWorkers = await context.Workers
                .Where(w => w.EmploymentPercentage == 100)
                .Select(w => w.Id)
                .ToListAsync();

            var result = scheduleMonthAlgorithm.Calculate(demands, workers, schedules, fullShiftWorkers, year, month);

            var resultSchedules = new List<ScheduleDto>();

            foreach (var schedule in result)
            {
                var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, schedule.From, schedule.To);

                context.Schedules.Add(new Schedule
                {
                    Date = schedule.Date,
                    WorkerId = schedule.WorkerId,
                    WorkingUnitId = workingUnit.Id
                });

                var resultSchedule = new ScheduleDto
                {
                    Date = schedule.Date,
                    From = workingUnit.From,
                    To = workingUnit.To,
                    WorkerInternalNumber = schedule.WorkerInternalNumber,
                    FullName = schedule.FullName
                };

                resultSchedules.Add(resultSchedule);
            }
            
            await context.SaveChangesAsync();
            return resultSchedules;
        }

        public async Task<List<ScheduleDto>> AddSingleWorker(ScheduleWorkerDto form)
        {
            var worker = await context
                .Workers
                .FirstOrDefaultAsync(w => w.WorkerInternalNumber == form.WorkerInternalNumber);

            if (worker == null) throw new WorkerNotFoundException(form.WorkerInternalNumber);

            var workingUnits = await context
                .WorkingUnits
                .Where(wu => wu.From == form.From && wu.To == form.To)
                .ToListAsync();

            var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, form.From, form.To);

            var schedule = new Schedule
            {
                Date = form.Date,
                WorkingUnit = workingUnit,
                Worker = worker
            };

            await context.AddAsync(schedule);
            await context.SaveChangesAsync();

            var result = await context
                .Schedules
                .Where(s => s.Date == form.Date)
                .Select(s => new ScheduleDto
                {
                    Date = s.Date,
                    From = s.WorkingUnit.From,
                    To = s.WorkingUnit.To,
                    WorkerInternalNumber = s.Worker.WorkerInternalNumber,
                    FullName = $"{s.Worker.FirstName} {s.Worker.LastName}"
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<ScheduleDto>> GetSchedules(ScheduleFilterDto filter)
        {
            var schedules = context.Schedules.AsQueryable();

            if (filter.UserId.HasValue)
            {
                schedules = schedules.Where(s => s.Worker.AppUserId == filter.UserId.Value.ToString());
            }
            if (filter.startDate.HasValue)
            {
                schedules = schedules.Where(s => s.Date >= filter.startDate.Value);
            }
            if (filter.endDate.HasValue)
            {
                schedules = schedules.Where(s => s.Date <= filter.endDate.Value);
            }
            if (!string.IsNullOrEmpty(filter.workerInternalNumbers))
            {
                int[] workerInternalIds = filter.workerInternalNumbers.Split(',').Select(item => int.Parse(item)).ToArray();
                schedules = schedules.Where(s => workerInternalIds.Contains(s.Worker.WorkerInternalNumber));
            }
            if (!string.IsNullOrEmpty(filter.workerName))
            {
                schedules = schedules
                    .Where(s => s.Worker.FirstName.Contains(filter.workerName) ||
                    s.Worker.LastName.Contains(filter.workerName));
            }

            var result = await schedules
                .OrderBy(s => s.Date)
                .ThenBy(s => s.WorkerId)
                .Skip((filter.page - 1) * filter.pageSize)
                .Take(filter.pageSize)
                .Select(s => new ScheduleDto
                {
                    Date = s.Date,
                    From = s.WorkingUnit.From,
                    To = s.WorkingUnit.To,
                    WorkerInternalNumber = s.Worker.WorkerInternalNumber,
                    FullName = $"{s.Worker.FirstName} {s.Worker.LastName}",
                    ScheduleId = s.Id
                })
                .ToListAsync();

            // Get unique dates from the result to check for missing shifts
            var dates = await context.ShiftDemands.AsNoTracking().Select(s => s.Date).ToHashSetAsync();

            // For each date, get the missing shifts and add them to the result
            foreach (var date in dates)
            {
                var toAdd = await GetMissingShifts(date);
                foreach (var item in toAdd)
                {
                    for (int i = 0; i < item.WorkersNeeded; i++)
                    {
                        result.Add(new ScheduleDto
                        {
                            ScheduleId = Guid.NewGuid(),
                            Date = item.Date,
                            From = item.From,
                            To = item.To,
                            WorkerInternalNumber = 0,
                            FullName = "Missing shift"
                        });
                    }
                }
            }

            return result.OrderBy(s => s.Date).ThenBy(s => s.From).ToList();
        }

        public async Task<List<ScheduleDto>> GetUserSchedules(Guid userId)
        {
            var result = await context.Schedules
                .Where(s => s.Worker.AppUserId == userId.ToString())
                .OrderBy(s => s.Date)
                .Select(s => new ScheduleDto
                {
                    Date = s.Date,
                    From = s.WorkingUnit.From,
                    To = s.WorkingUnit.To,
                    WorkerInternalNumber = s.Worker.WorkerInternalNumber,
                    FullName = $"{s.Worker.FirstName} {s.Worker.LastName}",
                    ScheduleId = s.Id
                })
                .ToListAsync();

            return result;
        }

        public async Task DeleteScheduleShift(Guid scheduleId)
        {
            await context.Schedules
                .Where(s => s.Id == scheduleId)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteDaySchedule(DateOnly date)
        {
            await context.Schedules
                .Where(s => s.Date == date)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteSchedulesByDaysRangeAndUsers(BulkDeleteSchedulesDto dto)
        {
            foreach (var date in dto.Dates)
            {
                if (dto.WorkersIds != null && dto.WorkersIds.Count > 0)
                {
                    await context.Schedules
                        .Include(s => s.Worker)
                        .Where(s => s.Date == date && dto.WorkersIds.Contains(s.Worker.WorkerInternalNumber))
                        .ExecuteDeleteAsync();
                }
            }
        }

        public async Task DeleteMonthSchedule(int year, int month)
        {
            await context.Schedules
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .ExecuteDeleteAsync();
        }

        // Worked hours summary for each worker for given month and year
        public async Task<List<SummaryByWorkers>> WorkersSummary(int year, int month)
        {
            var schedules = await context
                .Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .ToListAsync();

            var hoursSum = schedules
                .GroupBy(s => s.WorkerId)
                .ToDictionary(
                d => d.Key, 
                d => d.Sum(s => (s.WorkingUnit.To - s.WorkingUnit.From).TotalHours)
                );

            var res = await context.Workers.Include(w => w.Position).Select(w => new SummaryByWorkers
            {
                Id = w.Id,
                WorkerInternalNumber = w.WorkerInternalNumber,
                EmploymentPercentage = w.EmploymentPercentage,
                FirstName = w.FirstName,
                LastName = w.LastName,
                Position = w.Position.Name,
                WorkedHours = hoursSum.GetValueOrDefault(w.Id, 0),
                FullTimeHours = configuration.GetValue<int>("MonthWorkerHours", 168)
            }).ToListAsync();

            return res;
        }

        public async Task<List<ScheduleDto>> MonthSchedule(int year, int month)
        {
            var schedules = await context
                .Schedules
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .Select(s => new ScheduleDto
                {
                    Date = s.Date,
                    From = s.WorkingUnit.From,
                    To = s.WorkingUnit.To,
                    WorkerInternalNumber = s.Worker.WorkerInternalNumber,
                    FullName = $"{s.Worker.FirstName} {s.Worker.LastName}"
                }).ToListAsync();

            return schedules;
        }

        public async Task<List<WorkersLookupDto>> WorkersLookup()
        {
            var workers = await context
                .Workers
                .Select(w => new WorkersLookupDto
                {
                    Id = w.Id,
                    FullName = $"{w.FirstName} {w.LastName}",
                    WorkerInternalNumber = w.WorkerInternalNumber
                }).ToListAsync();
            return workers;
        }

        public async Task<List<SchedulingDemand>> GetMissingShifts(DateOnly date)
        {
            var result = new List<SchedulingDemand>();

            var schedules = await context.Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date == date)
                .AsNoTracking()
                .ToListAsync();
            var demands = await context.ShiftDemands
                .Include(s => s.WorkingUnit)
                .Where(sd => sd.Date == date)
                .AsNoTracking()
                .ToListAsync();

            foreach (var demand in demands)
            {
                var from30 = demand.WorkingUnit.From.AddMinutes(30);
                var to30 = demand.WorkingUnit.To.AddMinutes(-30);

                var scheduledIds = schedules
                    .Where(s => s.WorkingUnit.From <= from30 && s.WorkingUnit.To >= to30)
                    .Select(s => s.Id)
                    .Take(demand.WorkersNeeded)
                    .ToHashSet();

                var scheduledCount = scheduledIds.Count();

                if (scheduledCount < demand.WorkersNeeded)
                {
                    for(int i = 0; i < demand.WorkersNeeded - scheduledCount; i++) // every missing shift is added as a separate entry in the result list
                    {
                        result.Add(new SchedulingDemand
                        {
                            Date = demand.Date,
                            From = demand.WorkingUnit.From,
                            To = demand.WorkingUnit.To,
                            WorkersNeeded = 1
                        });
                    }
                }
                schedules.RemoveAll(s => scheduledIds.Contains(s.Id)); // Remove scheduled shifts from the list
            }
            return result;
        }

        public async Task<ScheduleDto> UpdateSchedule(Guid id, UpdateScheduleDto scheduleDto)
        {
            var schedule = await context.Schedules
                .Include(s => s.WorkingUnit)
                .Include(s => s.Worker)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) throw new Exception($"Schedule with id {id} not found");
            var workingUnits = await context.WorkingUnits.ToListAsync();
            var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, scheduleDto.From, scheduleDto.To);
            schedule.Date = scheduleDto.Date;
            schedule.WorkingUnit = workingUnit;
            await context.SaveChangesAsync();
            return new ScheduleDto
            {
                Date = schedule.Date,
                From = schedule.WorkingUnit.From,
                To = schedule.WorkingUnit.To,
                WorkerInternalNumber = schedule.Worker.WorkerInternalNumber,
                FullName = $"{schedule.Worker.FirstName} {schedule.Worker.LastName}",
                ScheduleId = schedule.Id
            };
        }

        public async Task<ScheduleDto> UpdateFinishTime(Guid id, TimeOnly finishTime)
        {
            var schedule = await context.Schedules
                .Include(s => s.WorkingUnit)
                .Include(s => s.Worker)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null) throw new Exception($"Schedule with id {id} not found");
            var workingUnits = await context.WorkingUnits.ToListAsync();
            var workingUnit = CreateWorkingUnitIfNotExists(workingUnits, schedule.WorkingUnit.From, finishTime);
            schedule.WorkingUnit = workingUnit;
            await context.SaveChangesAsync();
            return new ScheduleDto
            {
                Date = schedule.Date,
                From = schedule.WorkingUnit.From,
                To = schedule.WorkingUnit.To,
                WorkerInternalNumber = schedule.Worker.WorkerInternalNumber,
                FullName = $"{schedule.Worker.FirstName} {schedule.Worker.LastName}",
                ScheduleId = schedule.Id
            };
        }

        public async Task<double> GetTotalScheduledHours(int year, int month)
        {
            var selectedSchedules = await context.Schedules
                .Where(s => s.Date.Year == year && s.Date.Month == month)
                .Select(s => new
                {
                    from = s.WorkingUnit.From,
                    to = s.WorkingUnit.To
                }).ToListAsync();
            return selectedSchedules.Sum(s => (double)(s.to - s.from).TotalHours);
        }

        public async Task<List<WeeklyBreakIssuesDto>> GetWeeklyBreakIssues(int year, int month)
        {
            var workers = await context.Workers.ToListAsync();
            var date = new DateOnly(year, month, 1);
            var lastDayOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
            var schedules = await context.Schedules
                .Include(s => s.WorkingUnit)
                .Where(s => s.Date >= date.AddDays(-7) && s.Date <= lastDayOfMonth)
                .ToListAsync();

            var result = weeklyBreakOptimizer.Fix(year, month, workers, schedules);

            return result;
        }

        private WorkingUnit CreateWorkingUnitIfNotExists(List<WorkingUnit> workingUnits, TimeOnly from, TimeOnly to)
        {
            var workingUnit = workingUnits
                .FirstOrDefault(wu => wu.From == from && wu.To == to);

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

        private int GetSaturdaysNumberInMonth(int year, int month)
        {
            int res = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Count(d => new DateOnly(year, month, d).DayOfWeek == DayOfWeek.Saturday);
            return res;
        }
    }
}
