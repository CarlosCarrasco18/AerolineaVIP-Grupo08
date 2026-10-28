using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Ruta
    {
        public string Descripcion
        {
            get { return Aeropuerto1.CodigoIATA + " → " + Aeropuerto.CodigoIATA; }
        }

        public string DescripcionCompleta
        {
            get { return Aeropuerto1.Localidad + " → " + Aeropuerto.Localidad; }
        }
    }
}
