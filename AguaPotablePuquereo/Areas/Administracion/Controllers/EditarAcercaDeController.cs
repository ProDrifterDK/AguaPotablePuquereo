using AguaPotablePuquereo.Areas.Administracion.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Areas.Administracion.Controllers
{
    public class EditarAcercaDeController : BaseAdministrar
    {
        // GET: Administracion/EditarAcercaDe
        public ActionResult Index()
        {
            var model = BDD.TBL_PAGINAS_ADMINISTRABLES.FirstOrDefault(o => o.PADM_ID == 1);

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Index(AguaPotablePuquereo.Models.SQL.TBL_PAGINAS_ADMINISTRABLES model)
        {
            var padm = BDD.TBL_PAGINAS_ADMINISTRABLES.FirstOrDefault(o => o.PADM_ID == model.PADM_ID);

            padm.PADM_HTML_TEXT = model.PADM_HTML_TEXT;

            BDD.TBL_PAGINAS_ADMINISTRABLES.Attach(padm);
            BDD.Entry(padm).State = System.Data.Entity.EntityState.Modified;

            BDD.SaveChanges();

            return View(model);
        }
    }
}