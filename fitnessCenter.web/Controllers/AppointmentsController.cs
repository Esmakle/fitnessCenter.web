using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fitnessCenter.web.Data;
using fitnessCenter.web.Models;
using Microsoft.AspNetCore.Authorization;

namespace fitnessCenter.web.Controllers
{
    // Bu controller'a sadece giriş yapmış kullanıcılar erişebilir
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer);

            return View(await applicationDbContext.ToListAsync());
        }

        // SADECE ÜYE İÇİN: Kendi randevularım
        [Authorize]
        public async Task<IActionResult> MyAppointments()
        {
            var userEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(userEmail))
                return Challenge();

            // Sistemde bu mail ile kayıtlı Member'ı bul
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == userEmail);

            if (member == null)
            {
                TempData["Error"] = "Sistemde bu e-posta ile kayıtlı bir üye bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Sadece bu üyeye ait randevuları al
            var myAppointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == member.Id)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            return View("Index", myAppointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad");

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,MemberId,TrainerId,ServiceId,StartTime,EndTime,Status")]
            Appointment appointment)
        {
         
            // Eğer TRAINER değilse, onu normal üye kabul edip MemberId'yi mail üzerinden bağla
            if (!User.IsInRole("Trainer"))
            {
                var userEmail = User.Identity?.Name;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var member = await _context.Members
                        .FirstOrDefaultAsync(m => m.Email == userEmail);

                    if (member != null)
                    {
                        appointment.MemberId = member.Id;
                    }
                }
            }


            appointment.Status ??= "Pending";

            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş zamanı, başlangıç zamanından sonra olmalıdır.");
            }

            var startUtc = DateTime.SpecifyKind(appointment.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(appointment.EndTime, DateTimeKind.Utc);

            var day = startUtc.DayOfWeek;
            var startT = TimeOnly.FromDateTime(startUtc);
            var endT = TimeOnly.FromDateTime(endUtc);

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == appointment.TrainerId && a.DayOfWeek == day)
                .ToListAsync();

            bool fitsAvailability = availabilities.Any(a =>
                a.StartTime <= startT && a.EndTime >= endT);

            if (!fitsAvailability)
            {
                ModelState.AddModelError(string.Empty,
                    "Seçilen eğitmen bu gün ve saat aralığında çalışmıyor.");
            }

            bool hasOverlap = await _context.Appointments
                .AnyAsync(a =>
                    a.TrainerId == appointment.TrainerId &&
                    a.StartTime < endUtc &&
                    a.EndTime > startUtc);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu eğitmenin aynı zaman aralığında başka bir randevusu var.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);
                return View(appointment);
            }

            appointment.StartTime = startUtc;
            appointment.EndTime = endUtc;

            _context.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,MemberId,TrainerId,ServiceId,StartTime,EndTime,Status")]
            Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError(string.Empty,
                    "Bitiş zamanı, başlangıç zamanından sonra olmalıdır.");
            }

            var startUtc = DateTime.SpecifyKind(appointment.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(appointment.EndTime, DateTimeKind.Utc);

            var day = startUtc.DayOfWeek;
            var st = TimeOnly.FromDateTime(startUtc);
            var et = TimeOnly.FromDateTime(endUtc);

            var availabilities = await _context.TrainerAvailabilities
                .Where(a => a.TrainerId == appointment.TrainerId && a.DayOfWeek == day)
                .ToListAsync();

            bool fitsAvailability = availabilities.Any(a =>
                a.StartTime <= st && a.EndTime >= et);

            if (!fitsAvailability)
            {
                ModelState.AddModelError(string.Empty,
                    "Seçilen eğitmen bu gün ve saat aralığında çalışmıyor.");
            }

            bool hasOverlap = await _context.Appointments
                .AnyAsync(a =>
                    a.Id != appointment.Id &&
                    a.TrainerId == appointment.TrainerId &&
                    a.StartTime < endUtc &&
                    a.EndTime > startUtc);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "Bu eğitmenin aynı zaman aralığında başka bir randevusu var.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);
                return View(appointment);
            }

            appointment.StartTime = startUtc;
            appointment.EndTime = endUtc;

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
