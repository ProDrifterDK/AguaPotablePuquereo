using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Controllers {
	public class SessionController : Controller {
		// GET: Session
		public ActionResult VistaDeudas() {
			return View();
		}

		public ActionResult Deudas(string rut) {
			return View();
		}
		public ActionResult Error() {
			return View();
		}
	}
}