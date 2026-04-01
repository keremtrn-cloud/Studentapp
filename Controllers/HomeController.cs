using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using StudentApp.Data;

namespace StudentApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Context'i alıyoruz (Veritabanı bağlantısı)
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalStudents = await _context.Students.CountAsync();

            var absentByStudent = await _context.Attendances
                .Where(a => a.Status == "Absent")
                .GroupBy(a => a.StudentId)
                .Select(g => new { StudentId = g.Key, AbsenceCount = g.Count() })
                .ToListAsync();

            var absentLookup = absentByStudent.ToDictionary(x => x.StudentId, x => x.AbsenceCount);

            var warningCount = absentByStudent.Count(x => x.AbsenceCount == 3);
            var highRiskCount = absentByStudent.Count(x => x.AbsenceCount >= 4);
            var mediumRiskCount = absentByStudent.Count(x => x.AbsenceCount >= 2 && x.AbsenceCount <= 3);
            var lowRiskCount = totalStudents - absentByStudent.Count(x => x.AbsenceCount >= 2);

            var criticalCount = await _context.Students.CountAsync(s => s.RiskScore >= 80);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var todaysTotal = await _context.Attendances
                .CountAsync(a => a.Date >= today && a.Date < tomorrow);
            var todaysPresent = await _context.Attendances
                .CountAsync(a => a.Date >= today && a.Date < tomorrow && a.Status == "Present");

            var todayAttendancePercent = todaysTotal == 0
                ? 0
                : (double)todaysPresent * 100 / todaysTotal;

            var departmentRisks = await _context.Students
                .GroupBy(s => s.Department)
                .Select(g => new StudentApp.Models.DepartmentRiskItem
                {
                    Department = g.Key ?? "Unknown",
                    TotalRiskPoints = g.Sum(x => x.RiskScore)
                })
                .OrderByDescending(x => x.TotalRiskPoints)
                .ToListAsync();

            var incidents = await _context.Interventions
                .Include(i => i.Student)
                .Where(i => i.Type == "Incident")
                .OrderByDescending(i => i.Date)
                .Take(5)
                .ToListAsync();

            var model = new StudentApp.Models.DashboardViewModel
            {
                TotalStudents = totalStudents,
                CriticalCount = criticalCount,
                WarningCount = warningCount,
                TodayAttendancePercent = todayAttendancePercent,
                LowRiskCount = lowRiskCount,
                MediumRiskCount = mediumRiskCount,
                HighRiskCount = highRiskCount,
                DepartmentRisks = departmentRisks,
                RecentIncidents = incidents.Select(i => new StudentApp.Models.RecentIncidentItem
                {
                    StudentId = i.StudentId,
                    StudentName = i.Student?.FullName ?? "Unknown Student",
                    TriggerDate = i.Date,
                    RiskScore = i.Student?.RiskScore ?? 0,
                    AbsenceCount = absentLookup.TryGetValue(i.StudentId, out var count) ? count : 0
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}