using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class MetodoPago
    {
        public string NombreVisible
        {
            get { return Nombre.Split('(')[0].Trim(); }
        }
    }

    public partial class DatosEmisor
    {
        public string NombreVisible
        {
            get { return NombreComercial.Split('(')[0].Trim(); }
        }
    }
}
