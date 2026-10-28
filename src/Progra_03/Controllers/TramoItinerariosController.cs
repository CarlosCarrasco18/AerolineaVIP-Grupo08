using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Progra_03.Models;

namespace Progra_03.Controllers
{
    [Authorize]
    public class TramoItinerariosController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: TramoItinerarios
        [Authorize(Roles = "view")]
        public ActionResult Index(int? itinerarioId)
        {
            var tramos = db.TramoItinerarios
                .Include(t => t.Itinerario)
                .Include(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto)
                .Include(t => t.Vuelo.Frecuencia.Ruta.Aeropuerto1)
                .Where(t => itinerarioId == null || t.ItinerarioId == itinerarioId)
                .OrderByDescending(t => t.Itinerario.Referencia)
                .ThenBy(t => t.Orden)
                .ToList();

            ViewBag.itinerarioId = new SelectList(db.Itinerarios.OrderByDescending(i => i.Referencia).ToList(), "ItinerarioId", "Referencia", itinerarioId);
            return View(tramos);
        }

        // GET: TramoItinerarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramoItinerario = db.TramoItinerarios.Find(id);
            if (tramoItinerario == null)
            {
                return HttpNotFound();
            }
            return View(tramoItinerario);
        }

        // GET: TramoItinerarios/Create
        [Authorize(Roles = "create")]
        public ActionResult Create(int? itinerarioId)
        {
            TramoItinerario tramoItinerario = new TramoItinerario();
            if (itinerarioId != null)
            {
                tramoItinerario.ItinerarioId = itinerarioId.Value;
                tramoItinerario.Orden = (byte)(db.TramoItinerarios.Count(t => t.ItinerarioId == itinerarioId) + 1);
            }
            CargarListas(tramoItinerario);
            return View(tramoItinerario);
        }

        // POST: TramoItinerarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "TramoItinerarioId,ItinerarioId,VueloId,Orden")] TramoItinerario tramoItinerario)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(tramoItinerario);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.TramoItinerarios.Add(tramoItinerario);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { itinerarioId = tramoItinerario.ItinerarioId });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(tramoItinerario);

            return View(tramoItinerario);
        }

        // GET: TramoItinerarios/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramoItinerario = db.TramoItinerarios.Find(id);
            if (tramoItinerario == null)
            {
                return HttpNotFound();
            }
            CargarListas(tramoItinerario);

            return View(tramoItinerario);
        }

        // POST: TramoItinerarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "TramoItinerarioId,ItinerarioId,VueloId,Orden")] TramoItinerario tramoItinerario)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(tramoItinerario);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(tramoItinerario).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", new { itinerarioId = tramoItinerario.ItinerarioId });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(tramoItinerario);

            return View(tramoItinerario);
        }

        // GET: TramoItinerarios/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TramoItinerario tramoItinerario = db.TramoItinerarios.Find(id);
            if (tramoItinerario == null)
            {
                return HttpNotFound();
            }
            if (TieneTramosAntesYDespues(tramoItinerario))
            {
                ViewBag.Message = "No se puede eliminar un tramo intermedio porque el itinerario quedaría discontinuo. Elimine primero el último tramo.";
            }
            return View(tramoItinerario);
        }

        // POST: TramoItinerarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            TramoItinerario tramoItinerario = db.TramoItinerarios.Find(id);
            if (TieneTramosAntesYDespues(tramoItinerario))
            {
                ViewBag.Message = "No se puede eliminar un tramo intermedio porque el itinerario quedaría discontinuo. Elimine primero el último tramo.";
                return View(tramoItinerario);
            }
            try
            {
                db.TramoItinerarios.Remove(tramoItinerario);
                db.SaveChanges();
                return RedirectToAction("Index", new { itinerarioId = tramoItinerario.ItinerarioId });
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(tramoItinerario);
            }
        }

        private string Validar(TramoItinerario tramoItinerario)
        {
            var tramos = db.TramoItinerarios
                .Include(t => t.Vuelo.Frecuencia.Ruta)
                .Where(t => t.ItinerarioId == tramoItinerario.ItinerarioId && t.TramoItinerarioId != tramoItinerario.TramoItinerarioId)
                .ToList();

            if (tramos.Any(t => t.Orden == tramoItinerario.Orden))
            {
                return "Ya existe un tramo con el orden " + tramoItinerario.Orden + " en este itinerario.";
            }

            if (tramos.Any(t => t.VueloId == tramoItinerario.VueloId))
            {
                return "El vuelo ya forma parte de este itinerario.";
            }

            Vuelo vuelo = db.Vueloes.Find(tramoItinerario.VueloId);

            TramoItinerario anterior = tramos.Where(t => t.Orden < tramoItinerario.Orden).OrderByDescending(t => t.Orden).FirstOrDefault();
            if (anterior != null)
            {
                if (anterior.Vuelo.Frecuencia.Ruta.AeropuertoDestinoId != vuelo.Frecuencia.Ruta.AeropuertoOrigenId || anterior.Vuelo.FechaHoraLlegada > vuelo.FechaHoraSalida)
                {
                    return "El tramo no continúa desde el tramo anterior (" + anterior.Vuelo.Descripcion + ").";
                }
            }

            TramoItinerario siguiente = tramos.Where(t => t.Orden > tramoItinerario.Orden).OrderBy(t => t.Orden).FirstOrDefault();
            if (siguiente != null)
            {
                if (vuelo.Frecuencia.Ruta.AeropuertoDestinoId != siguiente.Vuelo.Frecuencia.Ruta.AeropuertoOrigenId || vuelo.FechaHoraLlegada > siguiente.Vuelo.FechaHoraSalida)
                {
                    return "El tramo no conecta con el tramo siguiente (" + siguiente.Vuelo.Descripcion + ").";
                }
            }
            return null;
        }

        private bool TieneTramosAntesYDespues(TramoItinerario tramoItinerario)
        {
            bool antes = db.TramoItinerarios.Any(t => t.ItinerarioId == tramoItinerario.ItinerarioId && t.Orden < tramoItinerario.Orden);
            bool despues = db.TramoItinerarios.Any(t => t.ItinerarioId == tramoItinerario.ItinerarioId && t.Orden > tramoItinerario.Orden);
            return antes && despues;
        }

        private void CargarListas(TramoItinerario tramoItinerario)
        {
            DateTime desde = DateTime.Today.AddDays(-30);
            var vuelos = db.Vueloes
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Where(v => v.FechaOperacion >= desde)
                .OrderBy(v => v.FechaOperacion)
                .ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            ViewBag.ItinerarioId = new SelectList(db.Itinerarios.OrderByDescending(i => i.Referencia).ToList(), "ItinerarioId", "Referencia", tramoItinerario.ItinerarioId);
            ViewBag.VueloId = new SelectList(vuelos, "VueloId", "Descripcion", tramoItinerario.VueloId);
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
