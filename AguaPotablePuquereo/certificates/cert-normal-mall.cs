using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/**
 * @author     Allware Ltda. (http://www.allware.cl)
 * @copyright  2015 Transbank S.A. (http://www.tranbank.cl)
 * @date       May 2016
 * @license    GNU LGPL
 * @version    1.0
 */

namespace Transbank.NET.sample.certificates
{
    public class CertNormalMall
    {

        internal static Dictionary<string, string> certificate()
        {

            /** Crea un Dictionary para almacenar los datos de integración pruebas */
            Dictionary<string, string> certificate = new Dictionary<string, string>();

            /** Agregar datos de integración a Dictionary */

            String certFolder = System.Web.HttpContext.Current.Server.MapPath(".");

            /** Modo de Utilización */
            certificate.Add("environment", "INTEGRACION");

            /** Certificado Publico (Dirección fisica de certificado o contenido) */
            certificate.Add("public_cert", certFolder + "\\certificates\\597020000542\\tbk.pem");

            /** Ejemplo de Ruta de Certificado de Salida */
            certificate.Add("webpay_cert", certFolder + "\\certificates\\597020000542\\597020000542.pfx");

            /** Ejemplo de Password de Certificado de Salida */
            certificate.Add("password", "transbank123");

            /** Codigo Comercio */
            certificate.Add("commerce_code", "597020000542");

            return certificate;

        }


        internal static Dictionary<string, string> store_codes()
        {

            /** Crea un Dictionary para almacenar los datos de tiendas */
            Dictionary<string, string> store_codes = new Dictionary<string, string>();

            /** Agregar datos de integración a Dictionary */

            /** Tienda 1 */
            store_codes.Add("tienda1", "597020000543");

            /** Tienda 2 */
            store_codes.Add("tienda2", "597020000544");

            return store_codes;

        }
    }
}