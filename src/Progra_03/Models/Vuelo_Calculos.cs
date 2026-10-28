using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class Vuelo
    {
        public const int CapacidadMaxima = 7;
        public const int HorasCheckIn = 48;

        public string CodigoVuelo
        {
            get { return "VIP" + VueloId.ToString("D5"); }
        }

        public string Descripcion
        {
            get { return CodigoVuelo + " · " + Frecuencia.Ruta.Descripcion + " · " + FechaHoraSalida.ToString("dd/MM/yyyy HH:mm"); }
        }

        public DateTime FechaHoraSalida
        {
            get { return FechaOperacion.Date.Add(Frecuencia.HoraSalida); }
        }

        public DateTime FechaHoraLlegada
        {
            get { return FechaOperacion.Date.Add(Frecuencia.HoraLlegada); }
        }

        public int CalcularCapacidad()
        {
            if (Aeronave == null)
            {
                return 0;
            }

            int capacidad = Aeronave.ModeloAeronave.CapacidadAsientos;
            if (capacidad > CapacidadMaxima)
            {
                capacidad = CapacidadMaxima;
            }
            return capacidad;
        }

        public int CalcularAsientosOcupados()
        {
            return ReservaPasajeroVueloes.Count(a => a.EstadoAsignacionCodigo == "CONFIRMADA");
        }

        public int CalcularAsientosReservados()
        {
            return ReservaPasajeroVueloes.Count(a => a.EstadoAsignacionCodigo == "PENDIENTE");
        }

        public int CalcularAsientosLibres()
        {
            return CalcularCapacidad() - CalcularAsientosOcupados() - CalcularAsientosReservados();
        }

        public decimal CalcularVentas()
        {
            return ReservaPasajeroVueloes.Where(a => a.EstadoAsignacionCodigo == "CONFIRMADA").Sum(a => a.ImporteTramo);
        }

        public bool PermiteVenta()
        {
            if (AeronaveId == null)
            {
                return false;
            }

            if (EstadoVueloCodigo != "PROGRAMADO" && EstadoVueloCodigo != "CONFIRMADO")
            {
                return false;
            }

            return FechaHoraSalida > DateTime.Now;
        }

        public bool CheckInAbierto()
        {
            DateTime ahora = DateTime.Now;
            return ahora >= FechaHoraSalida.AddHours(-HorasCheckIn) && ahora < FechaHoraSalida;
        }
    }
}
