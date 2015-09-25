using System.Web.Mvc;
using NetLibrary;
using Web.Models;

namespace Web.Controllers
{
    public class LinksController : Controller
    {
        // GET: /Links/
        public ActionResult Index(long id = 0)
        {
            if (id == 0) id = Net.Link;
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Infinite", model);
        }
    }
}