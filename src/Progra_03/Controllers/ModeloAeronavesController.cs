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
    public class ModeloAeronavesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        [Authorize(Roles = "view")]
        public ActionResult Index()
        {
            var modelos = db.ModeloAeronaves.Include(m => m.Aeronaves).Include(m => m.Asientoes).OrderBy(m => m.Fabricante).ToList();
            return View(modelos);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloAeronave modeloAeronave = db.ModeloAeronaves.Find(id);
            if (modeloAeronave == null)
            {
                return HttpNotFound();
            }
            return View(modeloAeronave);
        }

        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "ModeloAeronaveId,Fabricante,Nombre,CapacidadAsientos")] ModeloAeronave modeloAeronave)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(modeloAeronave);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.ModeloAeronaves.Add(modeloAeronave);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            return View(modeloAeronave);
        }

        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloAeronave modeloAeronave = db.ModeloAeronaves.Find(id);
            if (modeloAeronave == null)
            {
                return HttpNotFound();
            }

            return View(modeloAeronave);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "ModeloAeronaveId,Fabricante,Nombre,CapacidadAsientos")] ModeloAeronave modeloAeronave)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(modeloAeronave);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(modeloAeronave).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            return View(modeloAeronave);
        }

        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModeloAeronave modeloAeronave = db.ModeloAeronaves.Find(id);
            if (modeloAeronave == null)
            {
                return HttpNotFound();
            }
            if (modeloAeronave.Aeronaves.Any() || modeloAeronave.Asientoes.Any())
            {
                ViewBag.Message = "No se puede eliminar: el modelo tiene aeronaves o asientos registrados.";
            }
            return View(modeloAeronave);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            ModeloAeronave modeloAeronave = db.ModeloAeronaves.Find(id);
            if (modeloAeronave.Aeronaves.Any() || modeloAeronave.Asientoes.Any())
            {
                ViewBag.Message = "No se puede eliminar: el modelo tiene aeronaves o asientos registrados.";
                return View(modeloAeronave);
            }
            try
            {
                db.ModeloAeronaves.Remove(modeloAeronave);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(modeloAeronave);
            }
        }

        private string Validar(ModeloAeronave modeloAeronave)
        {
            if (modeloAeronave.CapacidadAsientos > Vuelo.CapacidadMaxima)
            {
                return "La capacidad máxima permitida es de " + Vuelo.CapacidadMaxima + " asientos.";
            }

            if (db.ModeloAeronaves.Any(m => m.Fabricante == modeloAeronave.Fabricante && m.Nombre == modeloAeronave.Nombre && m.ModeloAeronaveId != modeloAeronave.ModeloAeronaveId))
            {
                return "Ya existe ese modelo de aeronave.";
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
