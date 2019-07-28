using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class LinksController : Controller
    {
        // GET: /Links/
        public IActionResult Index(long id = 0)
        {
            if (id == 0)
            {
                id = Net.Link;
            }
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Index", model);
        }

        public IActionResult Infinite(long id = 0)
        {
            if (id == 0)
            {
                id = Net.Link;
            }
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Infinite", model);
        }
    }
}