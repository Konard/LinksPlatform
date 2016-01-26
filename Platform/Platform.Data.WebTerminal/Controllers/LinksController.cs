using System.Web.Mvc;
using Platform.Data.Core.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class LinksController : Controller
    {
        // GET: /Links/
        public ActionResult Index(long id = 0)
        {
            if (id == 0) id = Net.Link;
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Index", model);
        }

        public ActionResult Infinite(long id = 0)
        {
            if (id == 0) id = Net.Link;
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Infinite", model);
        }
    }
}