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
