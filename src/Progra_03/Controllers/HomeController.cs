using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    public class HomeController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();

        public ActionResult Index()
        {
            ViewBag.origen = new SelectList(db.Aeropuertoes.Where(a => a.Activo).OrderBy(a => a.Localidad).ToList(), "AeropuertoId", "NombreCompleto");
            ViewBag.destino = new SelectList(db.Aeropuertoes.Where(a => a.Activo).OrderBy(a => a.Localidad).ToList(), "AeropuertoId", "NombreCompleto");
            ViewBag.pasajeros = new SelectList(Enumerable.Range(1, Vuelo.CapacidadMaxima));
            ViewBag.Rutas = db.Rutas.Where(r => r.Activa).ToList();

            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).Include(r => r.Frecuencias)
                .Where(r => r.Activa)
                .OrderBy(r => r.TarifaReferencial)
                .ToList();
            return View(rutas);
        }

        public ActionResult Buscar(int? origen, int? destino, int? pasajeros)
        {
            if (pasajeros == null)
            {
                pasajeros = 1;
            }

            ViewBag.origen = new SelectList(db.Aeropuertoes.Where(a => a.Activo).OrderBy(a => a.Localidad).ToList(), "AeropuertoId", "NombreCompleto", origen);
            ViewBag.destino = new SelectList(db.Aeropuertoes.Where(a => a.Activo).OrderBy(a => a.Localidad).ToList(), "AeropuertoId", "NombreCompleto", destino);
            ViewBag.pasajeros = new SelectList(Enumerable.Range(1, Vuelo.CapacidadMaxima), pasajeros);
            ViewBag.Rutas = db.Rutas.Where(r => r.Activa).ToList();

            if (origen != null && origen == destino)
            {
                ViewBag.Message = "El destino debe ser diferente del origen.";
            }
            return View();
        }

        public ActionResult Resultados(int? origen, int? destino, DateTime? fecha, int? pasajeros)
        {
            if (origen == null || destino == null || origen == destino)
            {
                return RedirectToAction("Buscar", new { origen = origen, pasajeros = pasajeros });
            }

            if (pasajeros == null || pasajeros < 1 || pasajeros > Vuelo.CapacidadMaxima)
            {
                pasajeros = 1;
            }

            servicioReservas.LiberarVencidas(db);

            DateTime desde = DateTime.Today;
            DateTime hasta = DateTime.Today.AddDays(30);
            if (fecha != null)
            {
                desde = fecha.Value.Date;
                hasta = fecha.Value.Date;
            }

            var vuelos = db.Vueloes
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.Aeronave.ModeloAeronave)
                .Include(v => v.ReservaPasajeroVueloes)
                .Where(v => v.Frecuencia.Ruta.AeropuertoOrigenId == origen && v.Frecuencia.Ruta.AeropuertoDestinoId == destino && v.FechaOperacion >= desde && v.FechaOperacion <= hasta)
                .OrderBy(v => v.FechaOperacion)
                .ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            var itinerarios = db.Itinerarios
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto))
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.ReservaPasajeroVueloes))
                .Include(i => i.TramoItinerarios.Select(t => t.Vuelo.Aeronave.ModeloAeronave))
                .Where(i => i.EstadoItinerarioCodigo == "ACTIVO" && i.TramoItinerarios.Count() > 1)
                .Where(i => i.TramoItinerarios.Any(t => t.Orden == 1 && t.Vuelo.FechaOperacion >= desde && t.Vuelo.FechaOperacion <= hasta && t.Vuelo.Frecuencia.Ruta.AeropuertoOrigenId == origen))
                .Where(i => i.TramoItinerarios.Any(t => t.Vuelo.Frecuencia.Ruta.AeropuertoDestinoId == destino))
                .OrderBy(i => i.Referencia)
                .ToList();

            if (fecha == null)
            {
                vuelos = vuelos.Where(v => v.FechaHoraSalida > DateTime.Now).ToList();
            }

            ViewBag.Origen = db.Aeropuertoes.Find(origen);
            ViewBag.Destino = db.Aeropuertoes.Find(destino);
            ViewBag.Fecha = fecha;
            ViewBag.Pasajeros = pasajeros;
            ViewBag.Itinerarios = itinerarios;
            return View(vuelos);
        }

        public JsonResult FechasDisponibles(int? origen, int? destino, int? pasajeros)
        {
            Dictionary<string, string> fechas = new Dictionary<string, string>();

            if (origen == null || destino == null || origen == destino)
            {
                return Json(fechas, JsonRequestBehavior.AllowGet);
            }

            if (pasajeros == null)
            {
                pasajeros = 1;
            }

            DateTime hoy = DateTime.Today;

            var vuelos = db.Vueloes
                .Include(v => v.Frecuencia)
                .Include(v => v.Aeronave.ModeloAeronave)
                .Include(v => v.ReservaPasajeroVueloes)
                .Where(v => v.FechaOperacion >= hoy && v.Frecuencia.Ruta.AeropuertoOrigenId == origen && v.Frecuencia.Ruta.AeropuertoDestinoId == destino)
                .ToList();

            foreach (var vuelo in vuelos)
            {
                string clave = vuelo.FechaOperacion.ToString("yyyy-MM-dd");
                bool disponible = vuelo.PermiteVenta() && vuelo.CalcularAsientosLibres() >= pasajeros;

                if (disponible)
                {
                    fechas[clave] = "disponible";
                }
                else if (!fechas.ContainsKey(clave))
                {
                    fechas[clave] = "lleno";
                }
            }

            var tramos = db.TramoItinerarios
                .Include(t => t.Vuelo.Frecuencia)
                .Where(t => t.Orden == 1 && t.Vuelo.FechaOperacion >= hoy && t.Vuelo.Frecuencia.Ruta.AeropuertoOrigenId == origen)
                .Where(t => t.Itinerario.EstadoItinerarioCodigo == "ACTIVO" && t.Itinerario.TramoItinerarios.Any(x => x.Vuelo.Frecuencia.Ruta.AeropuertoDestinoId == destino))
                .ToList();

            foreach (var tramo in tramos)
            {
                string clave = tramo.Vuelo.FechaOperacion.ToString("yyyy-MM-dd");
                if (!fechas.ContainsKey(clave))
                {
                    fechas[clave] = "disponible";
                }
            }

            return Json(fechas, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
