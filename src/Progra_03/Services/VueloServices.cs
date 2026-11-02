using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Progra_03.Models;

namespace Progra_03.Services
{
    public class VueloServices
    {
        public int GenerarVuelos(AerolineaVIPEntities db, DateTime desde, DateTime hasta)
        {
            var frecuencias = db.Frecuencias.Where(f => f.Activa).ToList();
            int creados = 0;

            for (DateTime fecha = desde.Date; fecha <= hasta.Date; fecha = fecha.AddDays(1))
            {
                DateTime dia = fecha;
                byte numeroDia = Frecuencia.NumeroDelDia(dia);

                foreach (var frecuencia in frecuencias.Where(f => f.DiaSemana == numeroDia))
                {
                    int frecuenciaId = frecuencia.FrecuenciaId;
                    bool existe = db.Vueloes.Any(v => v.FrecuenciaId == frecuenciaId && v.FechaOperacion == dia);
                    if (existe)
                    {
                        continue;
                    }

                    Vuelo anterior = db.Vueloes
                        .Include(v => v.Aeronave)
                        .Where(v => v.FrecuenciaId == frecuenciaId && v.AeronaveId != null && v.FechaOperacion < dia)
                        .OrderByDescending(v => v.FechaOperacion)
                        .FirstOrDefault();

                    Vuelo vuelo = new Vuelo();
                    vuelo.FrecuenciaId = frecuenciaId;
                    vuelo.FechaOperacion = dia;
                    vuelo.EstadoVueloCodigo = "PROGRAMADO";
                    if (anterior != null && anterior.Aeronave.EstaDisponible())
                    {
                        vuelo.AeronaveId = anterior.AeronaveId;
                    }

                    db.Vueloes.Add(vuelo);
                    db.SaveChanges();
                    creados = creados + 1;
                }

                CrearItinerario(db, dia, numeroDia);
            }

            return creados;
        }

        private void CrearItinerario(AerolineaVIPEntities db, DateTime dia, byte numeroDia)
        {
            string referencia = "IT-" + dia.ToString("yyyyMMdd");
            if (db.Itinerarios.Any(i => i.Referencia == referencia))
            {
                return;
            }

            TimeSpan manana = new TimeSpan(9, 0, 0);
            TimeSpan tarde = new TimeSpan(14, 0, 0);

            var vuelosDia = db.Vueloes
                .Include(v => v.Frecuencia)
                .Where(v => v.FechaOperacion == dia)
                .ToList()
                .OrderBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            var tramos = vuelosDia.Where(v => numeroDia == 7 || v.Frecuencia.HoraSalida == manana || v.Frecuencia.HoraSalida == tarde).ToList();
            if (tramos.Count < 2)
            {
                return;
            }

            string nombre = "Quito – Galápagos – Quito (escala en Guayaquil)";
            if (numeroDia == 1 || numeroDia == 3 || numeroDia == 5)
            {
                nombre = "Quito – Manta – Quito";
            }
            else if (numeroDia == 2 || numeroDia == 4 || numeroDia == 6)
            {
                nombre = "Quito – Guayaquil – Quito";
            }

            Itinerario itinerario = new Itinerario();
            itinerario.Referencia = referencia;
            itinerario.Nombre = nombre + " · " + dia.ToString("dd/MM/yyyy");
            itinerario.EstadoItinerarioCodigo = "ACTIVO";
            db.Itinerarios.Add(itinerario);
            db.SaveChanges();

            byte orden = 1;
            foreach (var vuelo in tramos)
            {
                TramoItinerario tramo = new TramoItinerario();
                tramo.ItinerarioId = itinerario.ItinerarioId;
                tramo.VueloId = vuelo.VueloId;
                tramo.Orden = orden;
                db.TramoItinerarios.Add(tramo);
                db.SaveChanges();
                orden = (byte)(orden + 1);
            }
        }

        public List<string> ConsultarConflictos(AerolineaVIPEntities db, DateTime desde, DateTime hasta)
        {
            List<string> conflictos = new List<string>();

            var vuelos = db.Vueloes
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.Aeronave)
                .Where(v => v.AeronaveId != null && v.EstadoVueloCodigo != "CANCELADO" && v.FechaOperacion >= desde && v.FechaOperacion <= hasta)
                .ToList()
                .OrderBy(v => v.FechaOperacion)
                .ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            for (int i = 0; i < vuelos.Count; i++)
            {
                for (int j = i + 1; j < vuelos.Count; j++)
                {
                    Vuelo a = vuelos[i];
                    Vuelo b = vuelos[j];
                    if (a.AeronaveId == b.AeronaveId && a.FechaOperacion == b.FechaOperacion && a.Frecuencia.HoraSalida < b.Frecuencia.HoraLlegada && b.Frecuencia.HoraSalida < a.Frecuencia.HoraLlegada)
                    {
                        conflictos.Add(a.Aeronave.Matricula + " · " + a.FechaOperacion.ToString("dd/MM/yyyy") + " · " + a.CodigoVuelo + " (" + a.Frecuencia.Horario + ") se cruza con " + b.CodigoVuelo + " (" + b.Frecuencia.Horario + ")");
                    }
                }
            }

            return conflictos;
        }
    }
}
