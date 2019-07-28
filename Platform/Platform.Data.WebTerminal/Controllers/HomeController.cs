using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View(); //-V3013

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
