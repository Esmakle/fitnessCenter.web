using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fitnessCenter.web.Data;
using fitnessCenter.web.Models;
using System.Globalization;
namespace fitnessCenter.web.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // GET: Appointments
        // ---------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service);

            // ADMIN → tüm randevuları görsün
            if (User.IsInRole("Admin"))
            {
                return View(await query.ToListAsync());
            }

            // MEMBER → sadece kendi randevularını görsün
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.IdentityUserId == userId);

            if (member == null)
            {
                return Unauthorized();
            }

            var memberQuery = query.Where(a => a.MemberId == member.Id);

            return View(await memberQuery.ToListAsync());
        }

        // ---------------------------------------------------------
        // GET: Appointments/Details/5
        // ---------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // ---------------------------------------------------------
        // GET: Appointments/Create
        // ---------------------------------------------------------
        public IActionResult Create()
        {
            ViewBag.MemberId = new SelectList(_context.Members, "Id", "AdSoyad");
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad");
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad");

            return View();
        }

        // ---------------------------------------------------------
        // POST: Appointments/Create
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            // 0) Tarihleri UTC olarak işaretle
            var startUtc = DateTime.SpecifyKind(model.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(model.EndTime, DateTimeKind.Utc);

            // 1) Başlangıç < Bitiş kontrolü
            if (endUtc <= startUtc)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş zamanı başlangıç zamanından sonra olmalıdır.");
            }

            // 2) Eğitmen o gün çalışıyor mu?
            var day = startUtc.DayOfWeek;
            var start = TimeOnly.FromDateTime(startUtc);
            var end = TimeOnly.FromDateTime(endUtc);

            bool trainerAvailable = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == day &&
                a.StartTime <= start &&
                a.EndTime >= end);

            if (!trainerAvailable)
            {
                ModelState.AddModelError(string.Empty,
                    "Eğitmen bu gün ve saat aralığında çalışmıyor.");
            }

            // 3) Çakışan randevu var mı?
            bool hasConflict = await _context.Appointments.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.Status != "Cancelled" &&
                a.StartTime < endUtc &&
                startUtc < a.EndTime);

            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu saat aralığında eğitmenin başka randevusu var.");
            }

            if (ModelState.IsValid)
            {
                model.StartTime = startUtc;
                model.EndTime = endUtc;

                if (string.IsNullOrWhiteSpace(model.Status))
                    model.Status = "Pending";

                _context.Appointments.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // dropdown’ları yeniden yükle
            ViewBag.MemberId = new SelectList(_context.Members, "Id", "AdSoyad", model.MemberId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", model.ServiceId);
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", model.TrainerId);

            return View(model);
        }

        // ---------------------------------------------------------
        // GET: Appointments/Edit/5
        // ---------------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            ViewBag.MemberId = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);

            return View(appointment);
        }

        // ---------------------------------------------------------
        // POST: Appointments/Edit/5
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            if (id != model.Id)
                return NotFound();

            var startUtc = DateTime.SpecifyKind(model.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(model.EndTime, DateTimeKind.Utc);

            if (endUtc <= startUtc)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş zamanı başlangıç zamanından sonra olmalıdır.");
            }

            var day = startUtc.DayOfWeek;
            var start = TimeOnly.FromDateTime(startUtc);
            var end = TimeOnly.FromDateTime(endUtc);

            bool trainerAvailable = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == day &&
                a.StartTime <= start &&
                a.EndTime >= end);

            if (!trainerAvailable)
            {
                ModelState.AddModelError(string.Empty,
                    "Eğitmen bu gün ve saat aralığında çalışmıyor.");
            }

            bool hasConflict = await _context.Appointments.AnyAsync(a =>
                a.Id != model.Id &&
                a.TrainerId == model.TrainerId &&
                a.Status != "Cancelled" &&
                a.StartTime < endUtc &&
                startUtc < a.EndTime);

            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu saat aralığında çakışan bir randevu var.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.StartTime = startUtc;
                    model.EndTime = endUtc;

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.Id == model.Id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.MemberId = new SelectList(_context.Members, "Id", "AdSoyad", model.MemberId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", model.ServiceId);
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", model.TrainerId);

            return View(model);
        }

        // ---------------------------------------------------------
        // GET: Appointments/Delete/5
        // ---------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // ---------------------------------------------------------
        // POST: Appointments/Delete/5
        // ---------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------
        // GET: Appointments/GetAvailableSlots  (AJAX için)
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // GET: Appointments/GetAvailableSlots
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int trainerId, int serviceId, DateTime date)
        {
            if (trainerId == 0 || serviceId == 0 || date == default)
                return Json(Array.Empty<string>());

            // 🔹 PostgreSQL timestamptz sadece UTC kabul ettiği için
            // gelen tarihi Utc olarak işaretliyoruz
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            // 1) Hizmet süresi (dakika) -> seans süresi
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return Json(Array.Empty<string>());

            // Hizmette süre tanımlıysa onu kullan, yoksa varsayılan 90 dk
            var sessionMinutes = service.DurationMinutes > 0
                ? service.DurationMinutes
                : 90;

            // Seanslar arası mola süresi (dakika)
            const int breakMinutes = 20;

            // Global çalışma aralığı: 06:00 - 23:00
            var globalStart = new TimeOnly(6, 0);
            var globalEnd = new TimeOnly(23, 0);

            // 2) Eğitmenin o güne ait çalışma aralıkları
            var day = date.DayOfWeek;

            var availabilities = await _context.TrainerAvailabilities
                .AsNoTracking()
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == day)
                .ToListAsync();

            if (!availabilities.Any())
                return Json(Array.Empty<string>());

            // 3) Eğitmenin o günkü randevuları
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.Status != "Cancelled" &&
                    a.StartTime.Date == date.Date)
                .ToListAsync();

            var slots = new List<string>();

            foreach (var av in availabilities)
            {
                // Eğitmenin çalışma aralığını global çalışma saatleriyle kesiştir
                var windowStart = av.StartTime < globalStart ? globalStart : av.StartTime;
                var windowEnd = av.EndTime > globalEnd ? globalEnd : av.EndTime;

                if (windowStart >= windowEnd)
                    continue;

                var current = windowStart;

                while (current.AddMinutes(sessionMinutes) <= windowEnd)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(sessionMinutes);

                    // Tarihle saatleri birleştir
                    var startLocal = date.Date + slotStart.ToTimeSpan();
                    var endLocal = date.Date + slotEnd.ToTimeSpan();

                    // Bunları da Utc olarak işaretle (parametre olarak DB'ye gidecekler)
                    var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Utc);
                    var endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Utc);

                    // Çakışan randevu var mı?
                    bool conflict = appointments.Any(a =>
                        a.StartTime < endUtc &&
                        startUtc < a.EndTime);

                    if (!conflict)
                    {
                        // Örn: "06:00-07:30"
                        slots.Add($"{slotStart:HH\\:mm}-{slotEnd:HH\\:mm}");
                    }

                    // Bir sonraki seans: seans + mola
                    current = current.AddMinutes(sessionMinutes + breakMinutes);
                }
            }

            return Json(slots);
        }

    }
}
