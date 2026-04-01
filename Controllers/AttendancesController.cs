using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentApp.Data;
using StudentApp.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace StudentApp.Controllers
{
    public class AttendancesController : Controller
    {
        private readonly AppDbContext _context;

        public AttendancesController(AppDbContext context)
        {
            _context = context;
        }

        private void SendFailureWarning(string studentEmail, string studentName, string department)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SRM System", "alerts@university.edu"));
            message.To.Add(new MailboxAddress(studentName, studentEmail));
            message.Subject = "FINAL WARNING: Attendance Limit Reached";

            message.Body = new TextPart("plain")
            {
                Text = $@"Dear {studentName},

This is a FINAL WARNING. You have reached 4 total absences in {department}.

Your current Risk Score is 80. You are at high risk of failing this semester. 
Please be careful and do not miss any more classes.

If you have a medical report, submit it to Student Affairs immediately.

Best regards,
Automated Risk Response Team"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.mailtrap.io", 587, false);
                client.Authenticate("username", "password");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        // GET: Attendances
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Attendances.Include(a => a.Student);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Attendances/BulkEntry
        public async Task<IActionResult> BulkEntry(string? department)
        {
            var allDepartments = await _context.Students
                .Select(s => s.Department)
                .Where(d => d != null && d != "")
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var selectedDepartment = department;
            if (string.IsNullOrWhiteSpace(selectedDepartment) && allDepartments.Any())
            {
                selectedDepartment = allDepartments.First();
            }

            var studentQuery = _context.Students.AsQueryable();
            if (!string.IsNullOrWhiteSpace(selectedDepartment))
            {
                studentQuery = studentQuery.Where(s => s.Department == selectedDepartment);
            }

            var students = await studentQuery
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var model = new BulkAttendanceEntryViewModel
            {
                SelectedDepartment = selectedDepartment,
                Departments = allDepartments,
                Rows = students.Select(s => new BulkAttendanceRowViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    Department = s.Department ?? "",
                    Status = "Present"
                }).ToList()
            };

            return View(model);
        }

        // POST: Attendances/BulkEntry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkEntry(BulkAttendanceEntryViewModel model)
        {
            if (model.Rows == null || !model.Rows.Any())
            {
                ModelState.AddModelError(string.Empty, "No students found for attendance submission.");
            }

            if (!ModelState.IsValid)
            {
                model.Departments = await _context.Students
                    .Select(s => s.Department)
                    .Where(d => d != null && d != "")
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();
                return View(model);
            }

            var studentIds = model.Rows.Select(r => r.StudentId).ToList();
            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s);

            foreach (var row in model.Rows)
            {
                if (!students.TryGetValue(row.StudentId, out var student))
                {
                    continue;
                }

                var status = (row.Status ?? "Present").Trim();
                if (!status.Equals("Present", StringComparison.OrdinalIgnoreCase) &&
                    !status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                {
                    status = "Present";
                }

                var attendance = new Attendance
                {
                    StudentId = row.StudentId,
                    Status = status,
                    Date = DateTime.Now
                };

                _context.Attendances.Add(attendance);

                if (status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                {
                    var previousScore = student.RiskScore;
                    student.RiskScore += 20;

                    if (previousScore < 80 && student.RiskScore >= 80)
                    {
                        _context.Interventions.Add(new Intervention
                        {
                            StudentId = student.Id,
                            Type = "Incident",
                            Notes = $"Action Taken: Automated Warning Email Sent to {student.ParentEmail}\nAnalyst Note: Student notified of high failure risk (Score 80).",
                            Date = DateTime.Now
                        });

                        SendFailureWarning(student.ParentEmail, student.FullName, student.Department);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Attendances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // GET: Attendances/Create
        public IActionResult Create()
        {
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName");
            return View();
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Status,StudentId")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                attendance.Date = DateTime.Now;
                _context.Add(attendance);

                // Davranışsal risk ve incident mantığı (basitleştirilmiş):
                // - Her devamsızlık (Absent) için 20 puan
                // - RiskSkoru >= 80 ise öğrenci "riskli" kabul edilir ve Incident (Intervention) oluşturulur
                var studentToUpdate = await _context.Students.FindAsync(attendance.StudentId);

                if (studentToUpdate != null)
                {
                    var isAbsent = attendance.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase);

                    // Her devamsızlık için 20 puan ekle
                    if (isAbsent)
                    {
                        var previousScore = studentToUpdate.RiskScore;
                        studentToUpdate.RiskScore += 20;

                        if (previousScore < 80 && studentToUpdate.RiskScore >= 80)
                        {
                            var incident = new Intervention
                            {
                                StudentId = studentToUpdate.Id,
                                Type = "Incident",
                                Notes = $"Action Taken: Automated Warning Email Sent to {studentToUpdate.ParentEmail}\nAnalyst Note: Student notified of high failure risk (Score 80).",
                                Date = DateTime.Now
                            };

                            _context.Interventions.Add(incident);

                            SendFailureWarning(studentToUpdate.ParentEmail, studentToUpdate.FullName, studentToUpdate.Department);
                        }
                    }

                    _context.Students.Update(studentToUpdate);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty, "Veritabanı hatası. Öğrenci ID kontrol edin.");
                    ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", attendance.StudentId);
                    return View(attendance);
                }
                
                return RedirectToAction(nameof(Index));
            }

            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", attendance.StudentId);
            return View(attendance);
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", attendance.StudentId);
            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Status,StudentId")] Attendance attendance)
        {
            if (id != attendance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", attendance.StudentId);
            return View(attendance);
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}