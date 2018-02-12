using AguaPotablePuquereo.Controllers;
using AguaPotablePuquereo.Models.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AguaPotablePuquereo {
	public class MvcApplication : System.Web.HttpApplication {
		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            //if (httpException == null) return;
            Log(exception);
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Home");
            routeData.Values.Add("action", "Index");
            //if (httpException != null) {
            //    if (httpException.GetHttpCode() == 404) {
            //        routeData.Values["action"] = "HttpError404";
            //    }
            //}
            Server.ClearError();
            Response.Clear();
            IController errorController = new HomeController();
            errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }

        private void Log(Exception ex)
        {
            using (var BDD = new PuquerosBDD())
            {
                var Log = new TBL_LOG
                {
                    LOG_FECHA = DateTime.Now,
                    LOG_ERROR = ex.Message,
                    LOG_INNER = ex.InnerException.Message,
                };

                BDD.TBL_LOG.Add(Log);
                BDD.Entry(Log).State = System.Data.Entity.EntityState.Added;

                BDD.SaveChanges();
            }
        }
    }
}
