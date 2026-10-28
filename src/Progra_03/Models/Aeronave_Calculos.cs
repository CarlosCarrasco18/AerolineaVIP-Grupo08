using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Aeronave
    {
        public string Descripcion
        {
            get { return Matricula + " · " + ModeloAeronave.Nombre; }
        }

        public bool EstaDisponible()
        {
            return EstadoAeronaveCodigo == "DISPONIBLE";
        }
    }
}
