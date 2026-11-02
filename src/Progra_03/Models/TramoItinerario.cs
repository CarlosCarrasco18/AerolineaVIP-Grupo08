namespace Progra_03.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TramoItinerario
    {
        public int TramoItinerarioId { get; set; }
        public int ItinerarioId { get; set; }
        public int VueloId { get; set; }
        public byte Orden { get; set; }
    
        public virtual Itinerario Itinerario { get; set; }
        public virtual Vuelo Vuelo { get; set; }
    }
}
