//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using fitnessCenter.web.Data;
//using fitnessCenter.web.Models;
//using Microsoft.AspNetCore.Authorization;

//namespace fitnessCenter.web.Controllers
//{
//    [Authorize(Roles = "Admin")]


//    public class TrainersController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public TrainersController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Trainers
//        public async Task<IActionResult> Index()
//        {
//            var trainers = _context.Trainers
//                                   .Include(t => t.FitnessCenter);
//            return View(await trainers.ToListAsync());
//        }

//        // GET: Trainers/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//                return NotFound();

//            var trainer = await _context.Trainers
//                .Include(t => t.FitnessCenter)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (trainer == null)
//                return NotFound();

//            return View(trainer);
//        }

//        // GET: Trainers/Create
//        public IActionResult Create()
//        {
//            // Dropdown: Value = Id, görünen metin = Ad
//            ViewData["FitnessCenterId"] =
//                new SelectList(_context.FitnessCenters, "Id", "Ad");
//            return View();
//        }

//        // POST: Trainers/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,FitnessCenterId,AdSoyad,UzmanlikNotu")] Trainer trainer)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(trainer);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["FitnessCenterId"] =
//                new SelectList(_context.FitnessCenters, "Id", "Ad", trainer.FitnessCenterId);
//            return View(trainer);
//        }

//        // GET: Trainers/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//                return NotFound();

//            var trainer = await _context.Trainers.FindAsync(id);
//            if (trainer == null)
//                return NotFound();

//            ViewData["FitnessCenterId"] =
//                new SelectList(_context.FitnessCenters, "Id", "Ad", trainer.FitnessCenterId);
//            return View(trainer);
//        }

//        // POST: Trainers/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,FitnessCenterId,AdSoyad,UzmanlikNotu")] Trainer trainer)
//        {
//            if (id != trainer.Id)
//                return NotFound();

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(trainer);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!TrainerExists(trainer.Id))
//                        return NotFound();
//                    else
//                        throw;
//                }
//                return RedirectToAction(nameof(Index));
//            }

//            ViewData["FitnessCenterId"] =
//                new SelectList(_context.FitnessCenters, "Id", "Ad", trainer.FitnessCenterId);
//            return View(trainer);
//        }

//        // GET: Trainers/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//                return NotFound();

//            var trainer = await _context.Trainers
//                .Include(t => t.FitnessCenter)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (trainer == null)
//                return NotFound();

//            return View(trainer);
//        }

//        // POST: Trainers/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var trainer = await _context.Trainers.FindAsync(id);
//            if (trainer != null)
//            {
//                _context.Trainers.Remove(trainer);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool TrainerExists(int id)
//        {
//            return _context.Trainers.Any(e => e.Id == id);
//        }
//    }
//}
using fitnessCenter.web.Data;
using fitnessCenter.web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize] // default: giriş ister
public class TrainersController : Controller
{
    private readonly ApplicationDbContext _context;
    public TrainersController(ApplicationDbContext context) => _context = context;

    // ✅ herkes görsün
    [AllowAnonymous]
    public async Task<IActionResult> Index()
        => View(await _context.Trainers.ToListAsync());

    // ✅ herkes detay görsün
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var trainer = await _context.Trainers.FirstOrDefaultAsync(x => x.Id == id);
        if (trainer == null) return NotFound();
        return View(trainer);
    }

    // ✅ sadece Admin CRUD
    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Trainer trainer)
    {
        if (!ModelState.IsValid) return View(trainer);
        _context.Add(trainer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();
        return View(trainer);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Trainer trainer)
    {
        if (id != trainer.Id) return BadRequest();
        if (!ModelState.IsValid) return View(trainer);

        _context.Update(trainer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();
        return View(trainer);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null) return NotFound();
        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

