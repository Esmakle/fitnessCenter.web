using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fitnessCenter.web.Data;
using fitnessCenter.web.Models;

namespace fitnessCenter.web.Controllers
{
    public class TrainerServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainerServices
        public async Task<IActionResult> Index()
        {
            var query = _context.TrainerServices
                .Include(ts => ts.Trainer)
                .Include(ts => ts.Service);

            return View(await query.ToListAsync());
        }

        // GET: TrainerServices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var trainerService = await _context.TrainerServices
                .Include(ts => ts.Trainer)
                .Include(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainerService == null)
                return NotFound();

            return View(trainerService);
        }

        // GET: TrainerServices/Create
        public IActionResult Create()
        {
            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad");
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad");
            return View();
        }

        // POST: TrainerServices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainerService trainerService)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainerService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", trainerService.TrainerId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", trainerService.ServiceId);
            return View(trainerService);
        }

        // GET: TrainerServices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var trainerService = await _context.TrainerServices.FindAsync(id);
            if (trainerService == null)
                return NotFound();

            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", trainerService.TrainerId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", trainerService.ServiceId);
            return View(trainerService);
        }

        // POST: TrainerServices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainerService trainerService)
        {
            if (id != trainerService.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainerService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TrainerServices.Any(e => e.Id == trainerService.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TrainerId = new SelectList(_context.Trainers, "Id", "AdSoyad", trainerService.TrainerId);
            ViewBag.ServiceId = new SelectList(_context.Services, "Id", "Ad", trainerService.ServiceId);
            return View(trainerService);
        }

        // GET: TrainerServices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var trainerService = await _context.TrainerServices
                .Include(ts => ts.Trainer)
                .Include(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainerService == null)
                return NotFound();

            return View(trainerService);
        }

        // POST: TrainerServices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainerService = await _context.TrainerServices.FindAsync(id);
            if (trainerService != null)
            {
                _context.TrainerServices.Remove(trainerService);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
