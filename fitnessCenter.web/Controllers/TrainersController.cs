
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


    private static readonly string[] DemoImages =
{
    "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?q=80&w=900&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1550345332-09e3ac987658?q=80&w=900&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1526506118085-60ce8714f8c5?q=80&w=900&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1517963879433-6ad2b056d712?q=80&w=900&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1546483875-ad9014c88eba?q=80&w=900&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1558611848-73f7eb4001a1?q=80&w=900&auto=format&fit=crop",
};



    public async Task<IActionResult> Index()
    {
        // ✅ 1 kez çalışsın diye: ImageUrl boş olanlara otomatik görsel ata
        var trainers = await _context.Trainers.ToListAsync();
        bool changed = false;

        for (int i = 0; i < trainers.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(trainers[i].ImageUrl))
            {
                trainers[i].ImageUrl = DemoImages[i % DemoImages.Length];
                changed = true;
            }
        }

        if (changed)
            await _context.SaveChangesAsync();

        return View(trainers);
    }

    // ✅ herkes görsün
    //[AllowAnonymous]
    //public async Task<IActionResult> Index()
    //    => View(await _context.Trainers.ToListAsync());

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

