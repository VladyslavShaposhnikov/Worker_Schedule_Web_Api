using Microsoft.EntityFrameworkCore;
using Worker_Schedule_Web_Api.DTOs.Schedule;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services.Scheduling
{
    // todo : refactor this class to modify the schedules instead of returning a list of issues
    public class WeeklyBreakOptimizer : IWeeklyBreakOptimizer
    {
        public List<WeeklyBreakIssuesDto> Show(
            int year,
            int month,
            List<Worker> workers,
            List<Schedule> schedules)
        {
            var result = new List<WeeklyBreakIssuesDto>();
            var date = new DateOnly(year, month, 1);
            var lastDayOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            // Ensure schedules to only include those within the last 7 days of the month and the current month
            schedules = schedules
                .Where(s => s.Date >= date.AddDays(-7) && s.Date <= lastDayOfMonth).ToList(); 

            var datesToCheck = new List<DateOnly>();
            for (DateOnly d = date.AddDays(-7); d <= lastDayOfMonth; d = d.AddDays(1))
            {
                datesToCheck.Add(d);
            }

            foreach (var worker in workers)
            {
                var workerSchedules = schedules.Where(s => s.WorkerId == worker.Id).ToList();
                List<(DateTime from, DateTime to)?> breaksInfoList = new();
                foreach (var d in datesToCheck)
                {
                    if (!workerSchedules.Any(s => s.Date == d))
                    {
                        var breakInfo = VerifyBreak(workerSchedules, d);
                        if (breakInfo != null)
                        {
                            breaksInfoList.Add(breakInfo);
                        }
                    }
                }

                var breaksMoreThan7Days = FindDifferenceMoreThen7Days(breaksInfoList);
                if (breaksMoreThan7Days != null)
                {
                    foreach (var breakInfo in breaksMoreThan7Days)
                    {
                        if (breakInfo.HasValue)
                        {
                            result.Add(new WeeklyBreakIssuesDto
                            {
                                WorkerId = worker.Id,
                                WorkerInternalNumber = worker.WorkerInternalNumber,
                                WorkerName = $"{worker.FirstName} {worker.LastName}",
                                BreakStart = breakInfo.Value.From,
                                BreakEnd = breakInfo.Value.To,
                                WorkStreakDuration = breakInfo.Value.To - breakInfo.Value.From
                            });
                        }
                    }
                }
            }


            return result;
        }

        // This method finds the gaps between breaks that are more than 7 days apart.
        private List<(DateTime From, DateTime To)?> FindDifferenceMoreThen7Days(List<(DateTime from, DateTime to)?> breaksInfoList)
        {
            var result = new List<(DateTime From, DateTime To)?>();
            for (int i = 0; i < breaksInfoList.Count - 1; i++)
            {
                var currentBreak = breaksInfoList[i];
                var nextBreak = breaksInfoList[i + 1];
                if (currentBreak.HasValue && nextBreak.HasValue)
                {
                    var difference = nextBreak.Value.from - currentBreak.Value.to;
                    if (difference.TotalDays > 7)
                    {
                        result.Add((currentBreak.Value.to, nextBreak.Value.from)); // Add the gap between the two breaks
                    }
                }
            }
            return result;
        }

        // This method checks if there is a break of at least 35 hours between the last schedule
        // before the given date and the first schedule after the given date.
        private static (DateTime From, DateTime To)? VerifyBreak(List<Schedule> schedules, DateOnly date)
        {
            var breakStart = schedules
                .Where(s => s.Date < date)
                .OrderByDescending(s => s.Date)
                .Select(s => s.Date.ToDateTime(s.WorkingUnit.To))
                .FirstOrDefault();
            var breakEnd = schedules
                .Where(s => s.Date > date)
                .OrderBy(s => s.Date)
                .Select(s => s.Date.ToDateTime(s.WorkingUnit.From))
                .FirstOrDefault();
            if (breakStart == default || breakEnd == default || (breakEnd - breakStart).TotalHours < 35) return null;
            return (breakStart, breakEnd);
        }
    }
}
