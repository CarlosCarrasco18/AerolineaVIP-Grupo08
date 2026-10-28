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
    public class AeronavesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Aeronaves
        [Authorize(Roles = "view")]
        public ActionResult Index()
        {
            var aeronaves = db.Aeronaves.Include(a => a.ModeloAeronave).Include(a => a.Vueloes).OrderBy(a => a.Matricula).ToList();
            return View(aeronaves);
        }

        // GET: Aeronaves/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            return View(aeronave);
        }

        // GET: Aeronaves/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Aeronaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "AeronaveId,Matricula,ModeloAeronaveId,EstadoAeronaveCodigo")] Aeronave aeronave)
        {
            if (aeronave.Matricula != null)
            {
                aeronave.Matricula = aeronave.Matricula.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(aeronave);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Aeronaves.Add(aeronave);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(aeronave);

            return View(aeronave);
        }

        // GET: Aeronaves/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            CargarListas(aeronave);

            return View(aeronave);
        }

        // POST: Aeronaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "AeronaveId,Matricula,ModeloAeronaveId,EstadoAeronaveCodigo")] Aeronave aeronave)
        {
            if (aeronave.Matricula != null)
            {
                aeronave.Matricula = aeronave.Matricula.Trim().ToUpper();
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(aeronave);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(aeronave).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(aeronave);

            return View(aeronave);
        }

        // GET: Aeronaves/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave == null)
            {
                return HttpNotFound();
            }
            if (aeronave.Vueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: la aeronave tiene vuelos asignados.";
            }
            return View(aeronave);
        }

        // POST: Aeronaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Aeronave aeronave = db.Aeronaves.Find(id);
            if (aeronave.Vueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: la aeronave tiene vuelos asignados.";
                return View(aeronave);
            }
            try
            {
                db.Aeronaves.Remove(aeronave);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(aeronave);
            }
        }

        private string Validar(Aeronave aeronave)
        {
            if (db.Aeronaves.Any(a => a.Matricula == aeronave.Matricula && a.AeronaveId != aeronave.AeronaveId))
            {
                return "Ya existe una aeronave con la matrícula " + aeronave.Matricula + ".";
            }
            return null;
        }

        private void CargarListas(Aeronave aeronave)
        {
            var modelos = db.ModeloAeronaves.ToList();
            if (aeronave == null)
            {
                ViewBag.ModeloAeronaveId = new SelectList(modelos, "ModeloAeronaveId", "NombreCompleto");
                ViewBag.EstadoAeronaveCodigo = new SelectList(db.EstadoAeronaves, "Codigo", "Codigo", "DISPONIBLE");
            }
            else
            {
                ViewBag.ModeloAeronaveId = new SelectList(modelos, "ModeloAeronaveId", "NombreCompleto", aeronave.ModeloAeronaveId);
                ViewBag.EstadoAeronaveCodigo = new SelectList(db.EstadoAeronaves, "Codigo", "Codigo", aeronave.EstadoAeronaveCodigo);
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
