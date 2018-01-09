using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Areas.Administracion.Controllers
{
    public class LoginController : Base.BaseController
    {
        // GET: Administracion/Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Index(string nombre, string contrasena)
        {
            try
            {
                var usuario = BDD.TBL_USUARIO.FirstOrDefault(o => o.USU_USUARIO == nombre && o.USU_CONTRASEÑA == contrasena);
                UsuarioLogged = usuario ?? throw new Exception("Nombre de usuario o contraseña incorrectos.");
                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }
    }
}