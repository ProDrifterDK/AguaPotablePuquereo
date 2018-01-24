using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AguaPotablePuquereo.Models.Sitio;
using Webpay.Transbank.Library;
using Webpay.Transbank.Library.Wsdl.Normal;
using Webpay.Transbank.Library.Wsdl.Nullify;

namespace AguaPotablePuquereo.Controllers
{
    public class SessionController : Base.BaseController
    {
        // GET: Session
        #region Vistas
        public ActionResult Deudas(string rut, string aaction, string token)
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

            //WebPay
            Configuration configuration = new Configuration();
            configuration.Environment = certificate["environment"];
            configuration.CommerceCode = certificate["commerce_code"];
            configuration.PublicCert = certificate["public_cert"];
            configuration.WebpayCert = certificate["webpay_cert"];
            configuration.Password = certificate["password"];

            /** Crea Dictionary con descripción */
            Dictionary<string, string> description = new Dictionary<string, string>();

            description.Add("VD", "Venta Deb&iacute;to");
            description.Add("VN", "Venta Normal");
            description.Add("VC", "Venta en cuotas");
            description.Add("SI", "cuotas sin inter&eacute;s");
            description.Add("S2", "2 cuotas sin inter&eacute;s");
            description.Add("NC", "N cuotas sin inter&eacute;s");

            /** Creacion Objeto Webpay */
            Webpay.Transbank.Library.Webpay webpay = new Webpay.Transbank.Library.Webpay(configuration);

            /** Crea Dictionary con codigos de resultado */
            Dictionary<string, string> codes = new Dictionary<string, string>();

            codes.Add("0", "Transacci&oacute;n aprobada");
            codes.Add("-1", "Rechazo de transacci&oacute;n");
            codes.Add("-2", "Transacci&oacute;n debe reintentarse");
            codes.Add("-3", "Error en transacci&oacute;n");
            codes.Add("-4", "Rechazo de transacci&oacute;n");
            codes.Add("-5", "Rechazo por error de tasa");
            codes.Add("-6", "Excede cupo m&aacute;ximo mensual");
            codes.Add("-7", "Excede l&iacute;mite diario por transacci&oacute;n");
            codes.Add("-8", "Rubro no autorizado");

            if (aaction == "Deudas")
                aaction = "result";

            switch (aaction)
            {
                case "result":
                    /** Obtiene Información POST */
                    string[] keysPost = Request.Form.AllKeys;

                    /** Token de la transacción */

                    /** Token de la transacción */
                    token = Request.Form["token_ws"];
                    request.Add("token", token.ToString());

                    transactionResultOutput result = webpay.getNormalTransaction().getTransactionResult(token);

                    if (result.detailOutput[0].responseCode == 0)
                    {
                        ViewBag.Message = "Pago ACEPTADO por webpay (se deben guardar datos para mostrar voucher)";
                        ViewBag.Error = false;

                        ViewBag.AuthorizationCode = result.detailOutput[0].authorizationCode;
                        ViewBag.CommerceCode = result.detailOutput[0].commerceCode;
                        ViewBag.Amount = result.detailOutput[0].amount;
                        ViewBag.BuyOrder = result.detailOutput[0].buyOrder;
                    }
                    else
                    {
                        ViewBag.Message = "Pago RECHAZADO por webpay [Codigo]=> " + result.detailOutput[0].responseCode + " [Descripcion]=> " + codes[result.detailOutput[0].responseCode.ToString()];
                        ViewBag.Error = true;
                    }

                    break;
                case "end":
                    break;
            }

            return View(cliente);
        }

        public ActionResult Error()
        {
            return View();
        }

        #endregion

        #region Json
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

        #endregion

        #region WebPay
        /** Mensaje de Ejecución */
        private string message;

        /** Crea Dictionary con datos Integración Pruebas */
        private Dictionary<string, string> certificate = Transbank.NET.sample.certificates.CertNormal.certificate();

        /** Crea Dictionary con datos de entrada */
        private Dictionary<string, string> request = new Dictionary<string, string>();
        public JsonResult GetToken(int[] Deudas, string rut)
        {
            if(Deudas == null)
            {
                return JsonError("Debe seleccionar al menos un periodo.");
            }
            int deudaTotal = 0;

            foreach (var item in Deudas)
            {
                var deuda = BDD.TBL_DEUDA.FirstOrDefault(o => o.DEU_ID == item);
                deudaTotal += deuda.DEU_DEUDA;
            }

            Configuration configuration = new Configuration();
            configuration.Environment = certificate["environment"];
            configuration.CommerceCode = certificate["commerce_code"];
            configuration.PublicCert = certificate["public_cert"];
            configuration.WebpayCert = certificate["webpay_cert"];
            configuration.Password = certificate["password"];

            /** Creacion Objeto Webpay */
            Webpay.Transbank.Library.Webpay webpay = new Webpay.Transbank.Library.Webpay(configuration);

            /** Información de Host para crear URL */
            String httpHost = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_HOST"].ToString();
            String selfURL = System.Web.HttpContext.Current.Request.ServerVariables["URL"].ToString();


            string sample_baseurl = "http://" + httpHost + selfURL;
            /** Crea Dictionary con descripción */
            Dictionary<string, string> description = new Dictionary<string, string>();

            description.Add("VD", "Venta Deb&iacute;to");
            description.Add("VN", "Venta Normal");
            description.Add("VC", "Venta en cuotas");
            description.Add("SI", "cuotas sin inter&eacute;s");
            description.Add("S2", "2 cuotas sin inter&eacute;s");
            description.Add("NC", "N cuotas sin inter&eacute;s");


            string buyOrder;

            Random random = new Random();

            /** Monto de la transacción */
            decimal amount = (decimal)deudaTotal;

            /** Orden de compra de la tienda */
            buyOrder = random.Next(0, 1000).ToString();

            /** (Opcional) Identificador de sesión, uso interno de comercio */
            string sessionId = random.Next(0, 1000).ToString();

            /** URL Final */
            string urlReturn = "http://" + httpHost + "/Session/Deudas" + "?rut=" + rut + "&aaction=result";

            /** URL Final */
            string urlFinal = "http://" + httpHost + "/Session/Deudas" + "?rut=" + rut + "&aaction=end";

            request.Add("amount", amount.ToString());
            request.Add("buyOrder", buyOrder.ToString());
            request.Add("sessionId", sessionId.ToString());
            request.Add("urlReturn", urlReturn.ToString());
            request.Add("urlFinal", urlFinal.ToString());

            /** Ejecutamos metodo initTransaction desde Libreria */
            wsInitTransactionOutput result = webpay.getNormalTransaction().initTransaction(amount, buyOrder, sessionId, urlReturn, urlFinal);

            if (result.token != null && result.token != "")
            {
                return JsonExito("Sesion iniciada con exito en Webpay", new { request = request, result = result });
            }
            else
            {
                return JsonError("webpay no disponible");
            }
        }
        #endregion
    }
}