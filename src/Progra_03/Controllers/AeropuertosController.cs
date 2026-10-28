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
    public class AeropuertosController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Aeropuertos
        [Authorize(Roles = "view")]
        public ActionResult Index(string buscar)
        {
            if (buscar == null)
            {
                buscar = "";
            }

            var aeropuertos = db.Aeropuertoes
                .Where(a => a.CodigoIATA.Contains(buscar) || a.Nombre.Contains(buscar) || a.Localidad.Contains(buscar))
                .OrderBy(a => a.Localidad)
                .ToList();

            ViewBag.Buscar = buscar;
            return View(aeropuertos);
        }

        // GET: Aeropuertos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }
            return View(aeropuerto);
        }

        // GET: Aeropuertos/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Aeropuertos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "AeropuertoId,CodigoIATA,Nombre,Localidad,Activo")] Aeropuerto aeropuerto)
        {
            if (aeropuerto.CodigoIATA != null)
            {
                aeropuerto.CodigoIATA = aeropuerto.CodigoIATA.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(aeropuerto);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Aeropuertoes.Add(aeropuerto);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            return View(aeropuerto);
        }

        // GET: Aeropuertos/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }

            return View(aeropuerto);
        }

        // POST: Aeropuertos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "AeropuertoId,CodigoIATA,Nombre,Localidad,Activo")] Aeropuerto aeropuerto)
        {
            if (aeropuerto.CodigoIATA != null)
            {
                aeropuerto.CodigoIATA = aeropuerto.CodigoIATA.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(aeropuerto);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(aeropuerto).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            return View(aeropuerto);
        }

        // GET: Aeropuertos/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto == null)
            {
                return HttpNotFound();
            }
            if (aeropuerto.Rutas.Any() || aeropuerto.Rutas1.Any())
            {
                ViewBag.Message = "No se puede eliminar: el aeropuerto tiene rutas registradas.";
            }
            return View(aeropuerto);
        }

        // POST: Aeropuertos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Aeropuerto aeropuerto = db.Aeropuertoes.Find(id);
            if (aeropuerto.Rutas.Any() || aeropuerto.Rutas1.Any())
            {
                ViewBag.Message = "No se puede eliminar: el aeropuerto tiene rutas registradas.";
                return View(aeropuerto);
            }
            try
            {
                db.Aeropuertoes.Remove(aeropuerto);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(aeropuerto);
            }
        }

        private string Validar(Aeropuerto aeropuerto)
        {
            if (db.Aeropuertoes.Any(a => a.CodigoIATA == aeropuerto.CodigoIATA && a.AeropuertoId != aeropuerto.AeropuertoId))
            {
                return "Ya existe un aeropuerto con el código " + aeropuerto.CodigoIATA + ".";
            }
            return null;
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
