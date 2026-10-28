using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Progra_03.Models
{
    public partial class Frecuencia
    {
        public string NombreDia
        {
            get { return NombreDelDia(DiaSemana); }
        }

        public string Horario
        {
            get { return HoraSalida.ToString(@"hh\:mm") + " - " + HoraLlegada.ToString(@"hh\:mm"); }
        }

        public string Descripcion
        {
            get { return Ruta.Descripcion + " · " + NombreDia + " " + Horario; }
        }

        public static string NombreDelDia(byte dia)
        {
            string[] nombres = { "", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            if (dia >= 1 && dia <= 7)
            {
                return nombres[dia];
            }
            return "";
        }

        public static byte NumeroDelDia(DateTime fecha)
        {
            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                return 7;
            }
            return (byte)fecha.DayOfWeek;
        }

        public static SelectList ListaDias(object seleccionado)
        {
            List<SelectListItem> dias = new List<SelectListItem>();
            for (byte dia = 1; dia <= 7; dia++)
            {
                dias.Add(new SelectListItem { Value = dia.ToString(), Text = NombreDelDia(dia) });
            }
            return new SelectList(dias, "Value", "Text", seleccionado);
        }
    }
}
