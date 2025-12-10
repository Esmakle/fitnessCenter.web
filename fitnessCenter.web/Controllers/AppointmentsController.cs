using fitnessCenter.web.Data;
using fitnessCenter.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Create()
        {
            // Member ise: kendi bilgisini bul
            if (User.IsInRole("Member"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.IdentityUserId == userId);

                if (member == null)
                    return Unauthorized();

                ViewBag.CurrentMemberId = member.Id;
                ViewBag.CurrentMemberName = member.AdSoyad;
            }
            else
            {
                // Admin vb. ise, tüm üyelerin listesini görsün
                ViewBag.MemberList = new SelectList(_context.Members, "Id", "AdSoyad");
            }

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
            // Member ise, formdan gelen MemberId'yi YOK SAY, kendisi için ata
            if (User.IsInRole("Member"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.IdentityUserId == userId);

                if (member == null)
                    return Unauthorized();

                model.MemberId = member.Id;

                // Eğer MemberId için Required attribute varsa, eski değeri temizle
                ModelState.Remove("MemberId");
            }

            // Saat seçilmemişse daha anlamlı bir hata verelim
            if (model.StartTime == default || model.EndTime == default)
            {
                ModelState.AddModelError("StartTime", "Lütfen tarih ve saat seçiniz.");
            }

            // Buradan sonra senin mevcut kodun (UTC'ye çevirme, eğitmen uygun mu, çakışma kontrolü vs.)
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

            // dropdown’ları yeniden yükle (Admin / Member ayrımına göre)
            if (User.IsInRole("Admin"))
            {
                ViewBag.MemberList = new SelectList(_context.Members, "Id", "AdSoyad", model.MemberId);
            }
            else
            {
                // Member için adını tekrar dolduralım
                var userId2 = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var member2 = await _context.Members
                    .FirstOrDefaultAsync(m => m.IdentityUserId == userId2);

                if (member2 != null)
                {
                    ViewBag.CurrentMemberId = member2.Id;
                    ViewBag.CurrentMemberName = member2.AdSoyad;
                }
            }

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


        [HttpGet]
        // test için istersen şimdilik böyle yap:
        // [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int trainerId, int serviceId, string date)
        {
            // Temel kontroller
            if (trainerId == 0 || serviceId == 0 || string.IsNullOrWhiteSpace(date))
                return Json(Array.Empty<string>());

            // "2025-11-05" formatını parse et
            if (!DateTime.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dateValue))
            {
                return Json(Array.Empty<string>());
            }

            // 1) Hizmet süresi
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return Json(Array.Empty<string>());

            var sessionMinutes = service.DurationMinutes > 0
                ? service.DurationMinutes
                : 90;

            const int breakMinutes = 20;

            var globalStart = new TimeOnly(6, 0);
            var globalEnd = new TimeOnly(23, 0);

            // 2) Eğitmenin o güne ait çalışma saatleri
            var day = dateValue.DayOfWeek;

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == day)
                .ToListAsync();

            if (!availabilities.Any())
                return Json(Array.Empty<string>());

            // 3) O günkü randevular
            var appointments = await _context.Appointments
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.Status != "Cancelled" &&
                    a.StartTime.Date == dateValue.Date)
                .ToListAsync();

            var slots = new List<string>();

            foreach (var av in availabilities)
            {
                var windowStart = av.StartTime < globalStart ? globalStart : av.StartTime;
                var windowEnd = av.EndTime > globalEnd ? globalEnd : av.EndTime;

                if (windowStart >= windowEnd)
                    continue;

                var current = windowStart;

                while (current.AddMinutes(sessionMinutes) <= windowEnd)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(sessionMinutes);

                    var startLocal = dateValue.Date + slotStart.ToTimeSpan();
                    var endLocal = dateValue.Date + slotEnd.ToTimeSpan();

                    var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Utc);
                    var endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Utc);

                    bool conflict = appointments.Any(a =>
                        a.StartTime < endUtc &&
                        startUtc < a.EndTime);

                    if (!conflict)
                    {
                        slots.Add($"{slotStart:HH\\:mm}-{slotEnd:HH\\:mm}");
                    }

                    current = current.AddMinutes(sessionMinutes + breakMinutes);
                }
            }

            return Json(slots);
        }


    }
}
