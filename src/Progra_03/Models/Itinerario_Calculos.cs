using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Itinerario
    {
        public string DescripcionTramos()
        {
            List<string> tramos = new List<string>();
            foreach (var tramo in TramoItinerarios.OrderBy(t => t.Orden))
            {
                tramos.Add(tramo.Vuelo.Frecuencia.Ruta.Descripcion + " " + tramo.Vuelo.Frecuencia.HoraSalida.ToString(@"hh\:mm"));
            }
            return string.Join(" · ", tramos);
        }

        public decimal CalcularTarifa()
        {
            return TramoItinerarios.Sum(t => t.Vuelo.Frecuencia.Ruta.TarifaReferencial);
        }
    }
}
