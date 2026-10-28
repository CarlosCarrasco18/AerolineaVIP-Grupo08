using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Progra_03.Models
{
    public partial class ReservaPasajeroVuelo
    {
        public string CodigoAsiento
        {
            get
            {
                if (Vuelo.Aeronave == null)
                {
                    return NumeroAsiento.ToString();
                }

                Asiento asiento = Vuelo.Aeronave.ModeloAeronave.Asientoes.FirstOrDefault(a => a.NumeroAsiento == NumeroAsiento);
                if (asiento == null)
                {
                    return NumeroAsiento.ToString();
                }
                return asiento.Codigo;
            }
        }

        public bool TieneCheckIn()
        {
            return CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO");
        }
    }
}
