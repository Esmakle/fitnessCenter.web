using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using fitnessCenter.web.Data;

namespace fitnessCenter.web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------
        // 1) TÜM RANDEVULAR (BASİT GET + LINQ PROJECTION)
        // GET: api/appointmentsapi
        // ----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var data = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Select(a => new
                {
                    a.Id,
                    Member = a.Member.AdSoyad,
                    Trainer = a.Trainer.AdSoyad,
                    Service = a.Service.Ad,
                    a.StartTime,
                    a.EndTime,
                    a.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ----------------------------------------------------
        // 2) BELİRLİ ÜYENİN TÜM RANDEVULARI
        // GET: api/appointmentsapi/member/5
        // ----------------------------------------------------
        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> GetAppointmentsByMember(int memberId)
        {
            var data = await _context.Appointments
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.StartTime)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Select(a => new
                {
                    a.Id,
                    Trainer = a.Trainer.AdSoyad,
                    Service = a.Service.Ad,
                    a.StartTime,
                    a.EndTime,
                    a.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ----------------------------------------------------
        // 3) BELİRLİ TARİHTE UYGUN EĞİTMENLER
        // GET: api/appointmentsapi/available-trainers?date=2025-11-21&serviceId=1
        [HttpGet("available-trainers")]
        public async Task<IActionResult> GetAvailableTrainers(DateTime date, int serviceId)
        {
            // Gelen tarihi sadece gün bazında al ve UTC olarak işaretle
            var targetDateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var nextDateUtc = targetDateUtc.AddDays(1);

            var day = targetDateUtc.DayOfWeek;

            // 1) O gün için çalışma saati tanımlı eğitmenler
            var availableTrainers = await _context.TrainerAvailabilities
                .Where(a => a.DayOfWeek == day)
                .Select(a => a.Trainer)
                .Distinct()
                .ToListAsync();

            // 2) O gün herhangi bir saatte randevusu olan eğitmenler
            var busyTrainerIds = await _context.Appointments
                .Where(a => a.StartTime >= targetDateUtc && a.StartTime < nextDateUtc)
                .Select(a => a.TrainerId)
                .Distinct()
                .ToListAsync();

            // 3) Hem o gün çalışıp hem de boş olanlar
            var result = availableTrainers
                .Where(t => !busyTrainerIds.Contains(t.Id))
                .Select(t => new
                {
                    t.Id,
                    t.AdSoyad
                })
                .ToList();

            return Ok(result);
        }

    }
}
