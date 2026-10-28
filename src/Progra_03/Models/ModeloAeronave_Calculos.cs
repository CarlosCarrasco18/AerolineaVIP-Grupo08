using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class ModeloAeronave
    {
        public string NombreCompleto
        {
            get { return Fabricante + " " + Nombre; }
        }
    }
}
