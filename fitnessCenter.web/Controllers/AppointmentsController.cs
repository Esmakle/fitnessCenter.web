using fitnessCenter.web.Data;
using fitnessCenter.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace fitnessCenter.web.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Türkiye saati (Windows)
        private static readonly TimeZoneInfo TrTz =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsSignedIn() => User?.Identity?.IsAuthenticated == true;

        private async Task<Member?> GetCurrentMemberAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return null;

            return await _context.Members.FirstOrDefaultAsync(m => m.IdentityUserId == userId);
        }

        // ---------------------------------------------------------
        // GET: Appointments
        // Admin: tüm randevular
        // Diğer girişli: sadece kendi randevuları
        // ---------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service);

            if (User.IsInRole("Admin"))
                return View(await query.ToListAsync());

            if (!User.IsInRole("Member"))
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.IdentityUserId == userId);

            if (member == null)
                return Unauthorized();

            return View(await query.Where(a => a.MemberId == member.Id).ToListAsync());
        }

        // ---------------------------------------------------------
        // GET: Appointments/Create
        // Admin: üye seçer
        // Diğer girişli: kendi üyesi sabit gelir
        // ---------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            if (!IsSignedIn())
                return Unauthorized();

            if (IsAdmin())
            {
                ViewBag.MemberList = new SelectList(_context.Members, "Id", "AdSoyad");
            }
            else
            {
                var member = await GetCurrentMemberAsync();
                if (member == null) return Unauthorized();

                ViewBag.CurrentMemberId = member.Id;
                ViewBag.CurrentMemberName = member.AdSoyad;
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
            if (!IsSignedIn())
                return Unauthorized();

            // Admin değilse MemberId formdan gelse bile yok say
            if (!IsAdmin())
            {
                var member = await GetCurrentMemberAsync();
                if (member == null) return Unauthorized();

                model.MemberId = member.Id;
                ModelState.Remove("MemberId");
            }

            if (model.StartTime == default || model.EndTime == default)
                ModelState.AddModelError("StartTime", "Lütfen tarih ve saat seçiniz.");

            // Formdan gelen değerleri TR local kabul et -> UTC’ye çevir
            DateTime startLocal = DateTime.SpecifyKind(model.StartTime, DateTimeKind.Unspecified);
            DateTime endLocal = DateTime.SpecifyKind(model.EndTime, DateTimeKind.Unspecified);

            DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, TrTz);
            DateTime endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, TrTz);

            if (endUtc <= startUtc)
                ModelState.AddModelError(string.Empty, "Bitiş zamanı başlangıç zamanından sonra olmalıdır.");

            // Eğitmen uygun mu? (LOCAL güne göre)
            var dayLocal = startLocal.DayOfWeek;
            var startTimeOnly = TimeOnly.FromDateTime(startLocal);
            var endTimeOnly = TimeOnly.FromDateTime(endLocal);

            bool trainerAvailable = await _context.TrainerAvailabilities.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.DayOfWeek == dayLocal &&
                a.StartTime <= startTimeOnly &&
                a.EndTime >= endTimeOnly);

            if (!trainerAvailable)
                ModelState.AddModelError(string.Empty, "Eğitmen bu gün ve saat aralığında çalışmıyor.");

            // Çakışma kontrolü (UTC)
            bool hasConflict = await _context.Appointments.AnyAsync(a =>
                a.TrainerId == model.TrainerId &&
                a.Status != "Cancelled" &&
                a.StartTime < endUtc &&
                startUtc < a.EndTime);

            if (hasConflict)
                ModelState.AddModelError(string.Empty, "Bu saat aralığında eğitmenin başka randevusu var.");

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

            // dropdownlar tekrar dolsun
            if (IsAdmin())
            {
                ViewBag.MemberList = new SelectList(_context.Members, "Id", "AdSoyad", model.MemberId);
            }
            else
            {
                var member = await GetCurrentMemberAsync();
                if (member != null)
                {
                    ViewBag.CurrentMemberId = member.Id;
                    ViewBag.CurrentMemberName = member.AdSoyad;
                }
            }

            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", model.ServiceId);
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", model.TrainerId);

            return View(model);
        }

        // ---------------------------------------------------------
        // GET: Appointments/GetAvailableSlots (AJAX)
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int trainerId, int serviceId, string date)
        {
            if (trainerId <= 0 || serviceId <= 0 || string.IsNullOrWhiteSpace(date))
                return Json(Array.Empty<string>());

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dateValue))
                return Json(Array.Empty<string>());

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null) return Json(Array.Empty<string>());

            int sessionMinutes = service.DurationMinutes > 0 ? service.DurationMinutes : 90;
            const int breakMinutes = 20;

            var day = dateValue.DayOfWeek;

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == day)
                .ToListAsync();

            if (!availabilities.Any())
                return Json(Array.Empty<string>());

            // O gün local -> UTC aralığı
            var dayStartLocal = DateTime.SpecifyKind(dateValue.Date, DateTimeKind.Unspecified);
            var dayEndLocal = DateTime.SpecifyKind(dateValue.Date.AddDays(1), DateTimeKind.Unspecified);

            var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, TrTz);
            var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, TrTz);

            var appointments = await _context.Appointments
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.Status != "Cancelled" &&
                    a.StartTime < dayEndUtc &&
                    dayStartUtc < a.EndTime)
                .ToListAsync();

            var slots = new List<string>();

            foreach (var av in availabilities)
            {
                var current = av.StartTime;

                while (current.AddMinutes(sessionMinutes) <= av.EndTime)
                {
                    var slotStart = current;
                    var slotEnd = current.AddMinutes(sessionMinutes);

                    var startLocal = DateTime.SpecifyKind(dateValue.Date + slotStart.ToTimeSpan(), DateTimeKind.Unspecified);
                    var endLocal = DateTime.SpecifyKind(dateValue.Date + slotEnd.ToTimeSpan(), DateTimeKind.Unspecified);

                    var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, TrTz);
                    var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, TrTz);

                    bool conflict = appointments.Any(a => a.StartTime < endUtc && startUtc < a.EndTime);

                    if (!conflict)
                        slots.Add($"{slotStart:HH\\:mm}-{slotEnd:HH\\:mm}");

                    current = current.AddMinutes(sessionMinutes + breakMinutes);
                }
            }

            return Json(slots);
        }
    }
}
