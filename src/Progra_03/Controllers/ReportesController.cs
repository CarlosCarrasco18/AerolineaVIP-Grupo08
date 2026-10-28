using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();

        [Authorize(Roles = "view")]
        public ActionResult Index(DateTime? desde, DateTime? hasta, int? rutaId, int? aeronaveId)
        {
            servicioReservas.LiberarVencidas(db);

            if (desde == null)
            {
                desde = DateTime.Today.AddDays(-30);
            }

            if (hasta == null)
            {
                hasta = DateTime.Today.AddDays(30);
            }

            if (desde > hasta)
            {
                ViewBag.Message = "La fecha inicial no puede ser mayor que la fecha final.";
            }

            var vuelos = db.Vueloes
                .Include(v => v.Aeronave.ModeloAeronave)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.ReservaPasajeroVueloes)
                .Where(v => v.FechaOperacion >= desde && v.FechaOperacion <= hasta)
                .Where(v => (rutaId == null || v.Frecuencia.RutaId == rutaId) && (aeronaveId == null || v.AeronaveId == aeronaveId))
                .OrderBy(v => v.FechaOperacion)
                .ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.rutaId = new SelectList(db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).ToList(), "RutaId", "Descripcion", rutaId);
            ViewBag.aeronaveId = new SelectList(db.Aeronaves.OrderBy(a => a.Matricula).ToList(), "AeronaveId", "Matricula", aeronaveId);
            return View(vuelos);
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
