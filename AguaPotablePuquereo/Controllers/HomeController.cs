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
            ViewBag.Html = BDD.TBL_PAGINAS_ADMINISTRABLES.FirstOrDefault(o => o.PADM_ID == 1).PADM_HTML_TEXT;
			return View();
		}

        public ActionResult Payment()
        {
            return View();
        }
    }
}