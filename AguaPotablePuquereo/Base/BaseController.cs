using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AguaPotablePuquereo.Models.SQL;
using System.Collections;

namespace AguaPotablePuquereo.Base
{
    public class BaseController : Controller
    {
        protected PuquerosBDD BDD;
        protected BaseController()
        {
            BDD = new PuquerosBDD();
        }

        protected TBL_USUARIO UsuarioLogged
        {
            get { return Session["UsuarioLogged"] == null ? null : (TBL_USUARIO)Session["UsuarioLogged"]; }
            set { Session["UsuarioLogged"] = value; }
        }

        protected JsonResult JsonExito(string mensaje = "", object data = null)
        {
            return Json(new { exito = true, mensaje = mensaje, data = data });
        }

        protected JsonResult JsonError(string mensaje = "")
        {
            return Json(new { exito = false, mensaje = mensaje });
        }

        protected void Logger(Exception ex)
        {
            var Log = new TBL_LOG
            {
                LOG_FECHA = DateTime.Now,
                LOG_ERROR = ex.Message,
                LOG_INNER = ex.InnerException?.Message,
            };

            BDD.TBL_LOG.Add(Log);
            BDD.Entry(Log).State = System.Data.Entity.EntityState.Added;

            BDD.SaveChanges();
        }

        [HttpPost]
        public JsonResult JsonValidarRut(string rut)
        {
            if(BDD.TBL_CLIENTE.FirstOrDefault(o=>o.CLI_RUT == rut || o.CLI_CUENTA.ToString() == rut) == null)
                return JsonError("Rut o cuenta no válidos.");
            return JsonExito();
        }

        protected bool ValidarRut(string rut)
        {

            bool validacion = false;
            try
            {
                rut = rut.ToUpper();
                rut = rut.Replace(".", "");
                rut = rut.Replace("-", "");
                int rutAux = int.Parse(rut.Substring(0, rut.Length - 1));

                char dv = char.Parse(rut.Substring(rut.Length - 1, 1));

                int m = 0, s = 1;
                for (; rutAux != 0; rutAux /= 10)
                {
                    s = (s + rutAux % 10 * (9 - m++ % 6)) % 11;
                }
                if (dv == (char)(s != 0 ? s + 47 : 75))
                {
                    validacion = true;
                }
            }
            catch (Exception)
            {
            }
            return validacion;
        }

        public string URLWEBPAY = "";

        protected SelectList SelectMes(PuquerosBDD bdd)
        {
            ArrayList array = new ArrayList();

            var variable = bdd.TBL_MES.ToList().Select(o => new
            {
                Id = o.MES_ID,
                Nombre = o.MES_NOMBRE,
            }).ToArray();

            array.AddRange(variable);

            return new SelectList(array, "Id", "Nombre");
        }
    }
}