using System.Web.Mvc;
using Platform.Links.DataBase.CoreNet.Triplets;
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
            return View("Infinite", model);
        }
    }
}