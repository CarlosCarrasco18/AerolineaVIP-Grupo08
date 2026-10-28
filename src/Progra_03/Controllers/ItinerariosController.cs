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
    public class ItinerariosController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Itinerarios
        [Authorize(Roles = "view")]
        public ActionResult Index(string buscar)
        {
            if (buscar == null)
            {
                buscar = "";
            }

            var itinerarios = db.Itinerarios
                .Include(i => i.TramoItinerarios)
                .Where(i => i.Referencia.Contains(buscar) || i.Nombre.Contains(buscar))
                .OrderByDescending(i => i.Referencia)
                .ToList();

            ViewBag.Buscar = buscar;
            return View(itinerarios);
        }

        // GET: Itinerarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            return View(itinerario);
        }

        // GET: Itinerarios/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Itinerarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "ItinerarioId,Referencia,Nombre,EstadoItinerarioCodigo")] Itinerario itinerario)
        {
            if (itinerario.Referencia != null)
            {
                itinerario.Referencia = itinerario.Referencia.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(itinerario);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Itinerarios.Add(itinerario);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(itinerario);

            return View(itinerario);
        }

        // GET: Itinerarios/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            CargarListas(itinerario);

            return View(itinerario);
        }

        // POST: Itinerarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "ItinerarioId,Referencia,Nombre,EstadoItinerarioCodigo")] Itinerario itinerario)
        {
            if (itinerario.Referencia != null)
            {
                itinerario.Referencia = itinerario.Referencia.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(itinerario);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(itinerario).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(itinerario);

            return View(itinerario);
        }

        // GET: Itinerarios/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario == null)
            {
                return HttpNotFound();
            }
            if (itinerario.TramoItinerarios.Any())
            {
                ViewBag.Message = "No se puede eliminar: el itinerario tiene tramos. Elimine primero sus tramos.";
            }
            return View(itinerario);
        }

        // POST: Itinerarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Itinerario itinerario = db.Itinerarios.Find(id);
            if (itinerario.TramoItinerarios.Any())
            {
                ViewBag.Message = "No se puede eliminar: el itinerario tiene tramos. Elimine primero sus tramos.";
                return View(itinerario);
            }
            try
            {
                db.Itinerarios.Remove(itinerario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(itinerario);
            }
        }

        private string Validar(Itinerario itinerario)
        {
            if (db.Itinerarios.Any(i => i.Referencia == itinerario.Referencia && i.ItinerarioId != itinerario.ItinerarioId))
            {
                return "Ya existe un itinerario con la referencia " + itinerario.Referencia + ".";
            }
            return null;
        }

        private void CargarListas(Itinerario itinerario)
        {
            if (itinerario == null)
            {
                ViewBag.EstadoItinerarioCodigo = new SelectList(db.EstadoItinerarios, "Codigo", "Codigo", "ACTIVO");
            }
            else
            {
                ViewBag.EstadoItinerarioCodigo = new SelectList(db.EstadoItinerarios, "Codigo", "Codigo", itinerario.EstadoItinerarioCodigo);
            }
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
