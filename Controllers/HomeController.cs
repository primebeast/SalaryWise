using Microsoft.AspNetCore.Mvc;
using SalaryWise.Models;

namespace SalaryWise.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect logged-in users straight to dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
