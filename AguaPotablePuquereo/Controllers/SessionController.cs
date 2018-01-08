using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AguaPotablePuquereo.Models.Sitio;

namespace AguaPotablePuquereo.Controllers
{
    public class SessionController : Base.BaseController
    {
        // GET: Session
        public ActionResult VistaDeudas()
        {
            return View();
        }

        public ActionResult Deudas(string rut)
        {
            Cliente cliente = BDD.TBL_CLIENTE.Where(o => o.CLI_RUT == rut || o.CLI_CUENTA.ToString() == rut).ToList().Select(o => new Cliente
            {
                CliID = o.CLI_ID,
                Nombre = o.CLI_NOMBRE,
                ApellidoP = o.CLI_APELLIDO_PATERNO,
                ApellidoM = o.CLI_APELLIDO_MATERNO,
                Completo = o.CLI_COMPLETO,
                Cuenta = o.CLI_CUENTA,
                Rut = o.CLI_RUT
            }).FirstOrDefault();
            return View(cliente);
        }


        public JsonResult JsonGetListaDeudas(int CliId)
        {
            try
            {
                var data = BDD.TBL_DEUDA.Where(o => o.CLI_ID == CliId).ToList().Select(o => new
                {
                    Periodo = o.TBL_MES.MES_NOMBRE + " " +o.DEU_PERIODO_ANO,
                    Monto = o.DEU_DEUDA,
                    Vence = o.DEU_PERIODO_VENCE.ToString("dd/MM/yyyy"),
                    CLiId = o.CLI_ID,
                    Id = o.DEU_ID,
                }).ToList();

                return JsonExito("", data);
            }
            catch (Exception ex)
            {
                Logger(ex);
                return JsonError("Ha habido un problema al cargar las deudas.");
            }
        }
    }
		public ActionResult Error() {
			return View();
		}
	}
}