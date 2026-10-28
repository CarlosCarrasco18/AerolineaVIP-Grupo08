using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Reserva
    {
        public const int MinutosParaPagar = 60;

        public decimal CalcularTotal()
        {
            return ReservaPasajeroVueloes.Sum(a => a.ImporteTramo);
        }

        public int CalcularPasajeros()
        {
            return ReservaPasajeroVueloes.Select(a => a.PasajeroId).Distinct().Count();
        }

        public string DescripcionVuelos()
        {
            List<string> vuelos = new List<string>();
            foreach (var asignacion in ReservaPasajeroVueloes.OrderBy(a => a.Vuelo.FechaOperacion).ThenBy(a => a.Vuelo.Frecuencia.HoraSalida))
            {
                string texto = asignacion.Vuelo.Frecuencia.Ruta.Descripcion + " " + asignacion.Vuelo.FechaHoraSalida.ToString("dd/MM HH:mm");
                if (!vuelos.Contains(texto))
                {
                    vuelos.Add(texto);
                }
            }
            return string.Join(" · ", vuelos);
        }

        public bool EstaVencida()
        {
            return EstadoReservaCodigo == "PENDIENTE" && FechaExpiracion != null && FechaExpiracion < DateTime.Now;
        }
    }
}
