using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fitnessCenter.web.Data;
using fitnessCenter.web.Models;

namespace fitnessCenter.web.Controllers
{
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
        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberId,TrainerId,ServiceId,StartTime,EndTime,Status")] Appointment appointment)
        {
            // Önce temel model doğrulaması
            if (!ModelState.IsValid)
            {
                ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);
                return View(appointment);
            }

            // 1) Tarih mantıklı mı? (Bitiş > Başlangıç)
            if (appointment.EndTime <= appointment.StartTime)
            {
                ModelState.AddModelError("", "Bitiş zamanı, başlangıç zamanından sonra olmalıdır.");
            }

            // Tarihleri UTC olarak işaretleyelim (PostgreSQL hatası için)
            var startUtc = DateTime.SpecifyKind(appointment.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(appointment.EndTime, DateTimeKind.Utc);

            // 2) Eğitmenin o gün çalışma saati içinde mi?
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
                ModelState.AddModelError("", "Seçilen eğitmen bu gün ve saat aralığında çalışmıyor.");
            }

            // 3) Eğitmenin aynı saatte başka randevusu var mı? (Çakışma kontrolü)
            bool hasOverlap = await _context.Appointments
                .AnyAsync(a =>
                    a.TrainerId == appointment.TrainerId &&
                    a.StartTime < endUtc &&      // mevcut randevu bitmeden önce başlıyor
                    a.EndTime > startUtc);     // ve mevcut randevu başladıktan sonra bitiyor

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Bu eğitmenin aynı zaman aralığında başka bir randevusu var.");
            }

            // Eğer yukarıdaki kontroller ModelState'e hata eklediyse, formu tekrar göster
            if (!ModelState.IsValid)
            {
                ViewData["MemberId"] = new SelectList(_context.Members, "Id", "AdSoyad", appointment.MemberId);
                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "AdSoyad", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Ad", appointment.ServiceId);
                return View(appointment);
            }

            // Her şey yolundaysa kaydı ekle
            appointment.StartTime = startUtc;
            appointment.EndTime = endUtc;

            _context.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: TrainerAvailabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var trainerAvailability = await _context.TrainerAvailabilities.FindAsync(id);
            if (trainerAvailability == null)
                return NotFound();

            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "AdSoyad", trainerAvailability.TrainerId);

            return View(trainerAvailability);
        }

        // POST: TrainerAvailabilities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainerId,DayOfWeek,StartTime,EndTime")] TrainerAvailability trainerAvailability)
        {
            if (id != trainerAvailability.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainerAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TrainerAvailabilities.Any(e => e.Id == trainerAvailability.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // ModelState geçersizse dropdown’ı tekrar doldur
            ViewData["TrainerId"] =
                new SelectList(_context.Trainers, "Id", "AdSoyad", trainerAvailability.TrainerId);

            return View(trainerAvailability);
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
