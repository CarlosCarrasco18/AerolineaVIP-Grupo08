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
    public class RutasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Rutas
        [Authorize(Roles = "view")]
        public ActionResult Index()
        {
            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).Include(r => r.Frecuencias).ToList();
            return View(rutas);
        }

        // GET: Rutas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            return View(ruta);
        }

        // GET: Rutas/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Rutas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "RutaId,AeropuertoOrigenId,AeropuertoDestinoId,TarifaReferencial,Activa")] Ruta ruta)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(ruta);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Rutas.Add(ruta);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(ruta);

            return View(ruta);
        }

        // GET: Rutas/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            CargarListas(ruta);

            return View(ruta);
        }

        // POST: Rutas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "RutaId,AeropuertoOrigenId,AeropuertoDestinoId,TarifaReferencial,Activa")] Ruta ruta)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(ruta);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(ruta).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(ruta);

            return View(ruta);
        }

        // GET: Rutas/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ruta ruta = db.Rutas.Find(id);
            if (ruta == null)
            {
                return HttpNotFound();
            }
            if (ruta.Frecuencias.Any())
            {
                ViewBag.Message = "No se puede eliminar: la ruta tiene frecuencias registradas.";
            }
            return View(ruta);
        }

        // POST: Rutas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Ruta ruta = db.Rutas.Find(id);
            if (ruta.Frecuencias.Any())
            {
                ViewBag.Message = "No se puede eliminar: la ruta tiene frecuencias registradas.";
                return View(ruta);
            }
            try
            {
                db.Rutas.Remove(ruta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(ruta);
            }
        }

        private string Validar(Ruta ruta)
        {
            if (ruta.AeropuertoOrigenId == ruta.AeropuertoDestinoId)
            {
                return "El aeropuerto de destino debe ser diferente del origen.";
            }

            if (db.Rutas.Any(r => r.AeropuertoOrigenId == ruta.AeropuertoOrigenId && r.AeropuertoDestinoId == ruta.AeropuertoDestinoId && r.RutaId != ruta.RutaId))
            {
                return "Ya existe una ruta con ese origen y destino.";
            }
            return null;
        }

        private void CargarListas(Ruta ruta)
        {
            var aeropuertos = db.Aeropuertoes.OrderBy(a => a.Localidad).ToList();
            if (ruta == null)
            {
                ViewBag.AeropuertoOrigenId = new SelectList(aeropuertos, "AeropuertoId", "NombreCompleto");
                ViewBag.AeropuertoDestinoId = new SelectList(aeropuertos, "AeropuertoId", "NombreCompleto");
            }
            else
            {
                ViewBag.AeropuertoOrigenId = new SelectList(aeropuertos, "AeropuertoId", "NombreCompleto", ruta.AeropuertoOrigenId);
                ViewBag.AeropuertoDestinoId = new SelectList(aeropuertos, "AeropuertoId", "NombreCompleto", ruta.AeropuertoDestinoId);
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
