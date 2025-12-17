using fitnessCenter.web.Models.ViewModels;
using fitnessCenter.web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace fitnessCenter.web.Controllers
{
    // [Authorize] kullanıcının giriş yapmasını gerektiriyorsa kalsın.
    [Authorize]
    public class AiController : Controller
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        // GET: /Ai/Index
        [HttpGet]
        public IActionResult Index()
        {
            // İlk açılışta boş bir model gönderiyoruz
            return View(new AiSuggestViewModel());
        }

        // POST: /Ai/Index (Kullanıcı formu gönderdiğinde)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiSuggestViewModel model)
        {
            // Model doğrulamasını kontrol et (Prompt boş mu değil mi?)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // AI Servisini çağır
            model.Result = await _aiService.GenerateAsync(model.Prompt);

            // Sonucu aynı View'a geri gönder
            return View(model);
        }
    }
}