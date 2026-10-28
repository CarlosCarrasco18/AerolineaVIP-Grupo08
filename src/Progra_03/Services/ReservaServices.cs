using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Progra_03.Models;

namespace Progra_03.Services
{
    public class ReservaServices
    {
        public void LiberarVencidas(AerolineaVIPEntities db)
        {
            DateTime ahora = DateTime.Now;
            var vencidas = db.Reservas.Where(r => r.EstadoReservaCodigo == "PENDIENTE" && r.FechaExpiracion != null && r.FechaExpiracion < ahora).ToList();

            foreach (var reserva in vencidas)
            {
                reserva.EstadoReservaCodigo = "EXPIRADA";

                foreach (var asignacion in reserva.ReservaPasajeroVueloes)
                {
                    asignacion.EstadoAsignacionCodigo = "LIBERADA";
                }

                foreach (var compra in reserva.Compras.Where(c => c.EstadoCompraCodigo == "PENDIENTE"))
                {
                    compra.EstadoCompraCodigo = "ANULADA";
                }
            }

            if (vencidas.Count > 0)
            {
                db.SaveChanges();
            }
        }

        public List<byte> AsientosOcupados(AerolineaVIPEntities db, List<int> vueloIds, string estado)
        {
            return db.ReservaPasajeroVueloes
                .Where(a => vueloIds.Contains(a.VueloId) && a.EstadoAsignacionCodigo == estado)
                .Select(a => a.NumeroAsiento)
                .Distinct()
                .ToList();
        }
    }
}
