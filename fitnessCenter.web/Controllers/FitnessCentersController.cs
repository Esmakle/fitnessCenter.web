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
    public class FitnessCentersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FitnessCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FitnessCenters
        public async Task<IActionResult> Index()
        {
            return View(await _context.FitnessCenters.ToListAsync());
        }

        // GET: FitnessCenters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }

            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FitnessCenters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Adres,CalismaBaslangic,CalismaBitis,Aciklama")] FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fitnessCenter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters.FindAsync(id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }
            return View(fitnessCenter);
        }

        // POST: FitnessCenters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Adres,CalismaBaslangic,CalismaBitis,Aciklama")] FitnessCenter fitnessCenter)
        {
            if (id != fitnessCenter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fitnessCenter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FitnessCenterExists(fitnessCenter.Id))
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
            return View(fitnessCenter);
        }

        // GET: FitnessCenters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCenter = await _context.FitnessCenters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }

            return View(fitnessCenter);
        }

        // POST: FitnessCenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fitnessCenter = await _context.FitnessCenters.FindAsync(id);
            if (fitnessCenter != null)
            {
                _context.FitnessCenters.Remove(fitnessCenter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FitnessCenterExists(int id)
        {
            return _context.FitnessCenters.Any(e => e.Id == id);
        }
    }
}
