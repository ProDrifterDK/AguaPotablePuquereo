using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Controllers {
	public class HomeController : Base.BaseController {
		public ActionResult Index() {
			return View();
		}

		public ActionResult About() {
			return View();
		}
	}
}