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

            codes.Add("0", "Transacción aprobada");
            codes.Add("-1", "Rechazo de transacción");
            codes.Add("-2", "Transacción debe reintentarse");
            codes.Add("-3", "Error en transacción");
            codes.Add("-4", "Rechazo de transacción");
            codes.Add("-5", "Rechazo por error de tasa");
            codes.Add("-6", "Excede cupo máximo mensual");
            codes.Add("-7", "Excede límite diario por transacción");
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

                    var carro = BDD.TBL_CARRO_COMPRA.FirstOrDefault(o => o.CAR_TOKEN == token);

                    var listo = result.detailOutput[0].responseCode == 0;

                    if (listo)
                    {
                        listo = carro.CAR_MONTO == result.detailOutput[0].amount;

                        if (!listo)
                        {
                            carro.CEST_ESTADO = 5;
                            carro.CAS_ERROR = "Pago RECHAZADO los montos difieren";
                            ViewBag.Mensaje = "Pago RECHAZADO los montos difieren";
                            ViewBag.Error = true;

                            BDD.TBL_CARRO_COMPRA.Attach(carro);
                            BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

                            BDD.SaveChanges();

                            return View(cliente);
                        }

                        listo = carro.CAR_ORDEN_COMPRA == result.detailOutput[0].buyOrder;

                        if (!listo)
                        {
                            carro.CEST_ESTADO = 5;
                            carro.CAS_ERROR = "Pago RECHAZADO los orden de compra difieren";
                            ViewBag.Mensaje = "Pago RECHAZADO los orden de compra difieren";
                            ViewBag.Error = true;

                            BDD.TBL_CARRO_COMPRA.Attach(carro);
                            BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

                            BDD.SaveChanges();

                            return View(cliente);
                        }

                        carro.CEST_ESTADO = 2;

                        carro.CAR_CODIGO_AUTORIZACION = result.detailOutput[0].authorizationCode;
                        carro.CAR_CODIGO_COMERCIO = result.detailOutput[0].commerceCode;


                        BDD.TBL_CARRO_COMPRA.Attach(carro);
                        BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

                        BDD.SaveChanges();

                        var deudas = carro.TBL_DEUDA.ToList();

                        var pago = new Models.SQL.TBL_PAGOS
                        {
                            CLI_ID = deudas[0]?.CLI_ID ?? 00,
                            PAG_FECHA = DateTime.Now,
                            PAG_MONTO = carro.CAR_MONTO ?? 0,
                            PAG_VIGENCIA = true,
                        };

                        BDD.TBL_PAGOS.Add(pago);
                        BDD.Entry(pago).State = System.Data.Entity.EntityState.Added;

                        BDD.SaveChanges();

                        foreach (var item in deudas)
                        {
                            item.PAG_ID = pago.PAG_ID;

                            BDD.TBL_DEUDA.Attach(item);
                            BDD.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                        try
                        {
                            //var d = webpay.getNormalTransaction().acknowledgeTransaction(token);

                        }
                        catch (Exception ex)
                        {
                            Logger(ex);
                            carro.CEST_ESTADO = 5;
                            carro.CAS_ERROR = "Error al terminar el pago, itentelo de nuevo.";
                            ViewBag.Mensaje = "Error al terminar el pago, itentelo de nuevo.";
                            ViewBag.Error = true;

                            return View(cliente);
                        }

                        BDD.SaveChanges();

                        ViewBag.Mensaje = "Pago realizado con éxito.";

                        ViewBag.Error = false;

                        return Redirect(result.urlRedirection + "?token_ws=" + token);
                    }
                    else
                    {
                        carro.CEST_ESTADO = 5;
                        carro.CAS_ERROR = "Pago RECHAZADO por webpay [Codigo]=> " + result.detailOutput[0].responseCode + " [Descripcion]=> " + codes[result.detailOutput[0].responseCode.ToString()];
                        ViewBag.Mensaje = "Pago RECHAZADO por webpay [Codigo]=> " + result.detailOutput[0].responseCode + " [Descripcion]=> " + codes[result.detailOutput[0].responseCode.ToString()];
                        ViewBag.Error = true;

                        BDD.TBL_CARRO_COMPRA.Attach(carro);
                        BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

                        BDD.SaveChanges();

                        return View(cliente);
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
                var data = BDD.TBL_DEUDA.Where(o => o.CLI_ID == CliId && o.PAG_ID == null).ToList().Select(o => new
                {
                    Periodo = o.TBL_MES.MES_NOMBRE + " " +o.DEU_PERIODO_ANO,
                    Monto = o.DEU_DEUDA.ToString("C0"),
                    Vence = o.DEU_PERIODO_VENCE.ToString("dd/MM/yyyy"),
                    CLiId = o.CLI_ID,
                    Multa = o.DEU_MULTA?.ToString("C0"),
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

            var carro = new Models.SQL.TBL_CARRO_COMPRA
            {
                CEST_ESTADO = 1,
            };

            BDD.TBL_CARRO_COMPRA.Add(carro);
            BDD.Entry(carro).State = System.Data.Entity.EntityState.Added;

            BDD.SaveChanges();

            foreach (var item in Deudas)
            {
                var deuda = BDD.TBL_DEUDA.FirstOrDefault(o => o.DEU_ID == item);
                deuda.CAR_ID = carro.CAR_ID;

                BDD.TBL_DEUDA.Attach(deuda);
                BDD.Entry(deuda).State = System.Data.Entity.EntityState.Modified;

                deudaTotal += deuda.DEU_DEUDA;
            }

            carro.CAR_MONTO = deudaTotal;

            BDD.TBL_CARRO_COMPRA.Attach(carro);
            BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

            BDD.SaveChanges();

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
            buyOrder = carro.CAR_ID.ToString();

            /** (Opcional) Identificador de sesión, uso interno de comercio */
            string sessionId = Session.SessionID;

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

            carro.CAR_TOKEN = result.token;
            carro.CAR_ORDEN_COMPRA = buyOrder;
            carro.CAR_SESSION_ID = sessionId;

            BDD.TBL_CARRO_COMPRA.Attach(carro);
            BDD.Entry(carro).State = System.Data.Entity.EntityState.Modified;

            BDD.SaveChanges();

            URLWEBPAY = result.url;

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