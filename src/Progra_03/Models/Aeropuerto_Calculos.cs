using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Aeropuerto
    {
        public string NombreCompleto
        {
            get { return Localidad + " (" + CodigoIATA + ")"; }
        }
    }
}
