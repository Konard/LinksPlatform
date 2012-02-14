using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTester.Models;
using NetLibrary;

namespace WebTester.Controllers
{
	public class LinksController : Controller
	{
		//
		// GET: /Links/

		public ActionResult Index(long id = 0)
		{
			// Id может быть со смещением, тогда может произойти повреждение данных

			if (id == 0)
			{
				id = Net.Link.GetPointer().ToInt64();
			}

			LinkModel model;

			try
			{
				Link link = Link.Create(new IntPtr(id));
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
