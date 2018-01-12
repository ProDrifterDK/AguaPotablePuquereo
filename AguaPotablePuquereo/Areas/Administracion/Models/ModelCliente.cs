using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AguaPotablePuquereo.Areas.Administracion.Models
{
    public class ModelCliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public string NombreCompleto { get; set; }
        public string Rut { get; set; }
        public int Cuenta { get; set; }
    }
}