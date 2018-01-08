using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AguaPotablePuquereo.Models.Sitio
{
    public class Cliente
    {
        public int CliID { get; set; }
        public string Nombre { get; set; }
        public string ApellidoP { get; set; }
        public string ApellidoM { get; set; }
        public string Completo { get; set; }
        public int Cuenta { get; set; }
        public string Rut { get; set; }
    }
}