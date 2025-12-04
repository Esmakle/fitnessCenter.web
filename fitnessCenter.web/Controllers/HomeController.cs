using System.Diagnostics;
using fitnessCenter.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace fitnessCenter.web.Controllers
{
    // Bu controller'daki tüm action'lar giriþ yapmadan da görülebilsin
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Merkez hakkýnda / ana sayfa
        public IActionResult Index()
        {
            return View();
        }

        // Ýstersen burayý "Hakkýmýzda / KVKK / Gizlilik" gibi kullanýrsýn
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
