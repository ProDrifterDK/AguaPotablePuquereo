using AguaPotablePuquereo.Areas.Administracion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Areas.Administracion.Controllers
{
    public class AdministradorController : Base.BaseAdministrar
    {
        // GET: Administracion/Administrador
#region Vistas

        public ActionResult Index()
        {
            return View();
        }

#region Clientes
        public ActionResult Cliente()
        {
            return View();
        }

        public ActionResult MantencionCliente(int CliId)
        {
            var modelo = BDD.TBL_CLIENTE.Where(o => o.CLI_ID == CliId).ToList().Select(o => new ModelCliente
            {
                Id = o.CLI_ID,
                Nombre = o.CLI_NOMBRE,
                ApellidoM = o.CLI_APELLIDO_MATERNO,
                ApellidoP = o.CLI_APELLIDO_PATERNO,
                Rut = o.CLI_RUT,
                Cuenta = o.CLI_CUENTA,
            });

            return View(modelo);
        }

#endregion

#endregion
        //Json

        //Cliente
        public JsonResult GetListaCliente()
        {
            try
            {
                var data = BDD.TBL_CLIENTE.ToList().Select(o => new
                {
                    Nombre = o.CLI_COMPLETO,
                    Rut = o.CLI_RUT,
                    Cuenta = o.CLI_CUENTA,
                    TotalDeuda = o.TBL_DEUDA.Select(p => p.DEU_DEUDA).Sum(),
                }).ToList();

                return JsonExito("", data);
            }
            catch (Exception)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}