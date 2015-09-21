using System;
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
            // Id может быть со смещением, тогда может произойти повреждение данных

            if (id == 0)
            {
                id = Net.Link;
            }

            LinkModel model;

            try
            {
                Link link = Link.Restore(id);
                model = LinkModel.CreateLinkModel(link);
            }
            catch (Exception ex)
            {
                throw new Exception("Not found.", ex);
            }

            return View(model);
        }
    }
}