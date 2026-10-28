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
    public class FrecuenciasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Frecuencias
        [Authorize(Roles = "view")]
        public ActionResult Index(int? rutaId)
        {
            var frecuencias = db.Frecuencias
                .Include(f => f.Ruta.Aeropuerto)
                .Include(f => f.Ruta.Aeropuerto1)
                .Where(f => rutaId == null || f.RutaId == rutaId)
                .OrderBy(f => f.RutaId)
                .ThenBy(f => f.DiaSemana)
                .ThenBy(f => f.HoraSalida)
                .ToList();

            ViewBag.rutaId = new SelectList(db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).ToList(), "RutaId", "Descripcion", rutaId);
            return View(frecuencias);
        }

        // GET: Frecuencias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            return View(frecuencia);
        }

        // GET: Frecuencias/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Frecuencias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(frecuencia);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Frecuencias.Add(frecuencia);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(frecuencia);

            return View(frecuencia);
        }

        // GET: Frecuencias/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            CargarListas(frecuencia);

            return View(frecuencia);
        }

        // POST: Frecuencias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(frecuencia);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(frecuencia).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(frecuencia);

            return View(frecuencia);
        }

        // GET: Frecuencias/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            if (frecuencia.Vueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: la frecuencia tiene vuelos programados.";
            }
            return View(frecuencia);
        }

        // POST: Frecuencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia.Vueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: la frecuencia tiene vuelos programados.";
                return View(frecuencia);
            }
            try
            {
                db.Frecuencias.Remove(frecuencia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(frecuencia);
            }
        }

        private string Validar(Frecuencia frecuencia)
        {
            if (frecuencia.HoraLlegada <= frecuencia.HoraSalida)
            {
                return "La hora de llegada debe ser posterior a la hora de salida.";
            }

            if (db.Frecuencias.Any(f => f.RutaId == frecuencia.RutaId && f.DiaSemana == frecuencia.DiaSemana && f.HoraSalida == frecuencia.HoraSalida && f.FrecuenciaId != frecuencia.FrecuenciaId))
            {
                return "Ya existe una frecuencia para esa ruta, día y hora de salida.";
            }

            if (frecuencia.FrecuenciaId != 0 && db.Vueloes.Any(v => v.FrecuenciaId == frecuencia.FrecuenciaId))
            {
                bool cambioDia = db.Frecuencias.Any(f => f.FrecuenciaId == frecuencia.FrecuenciaId && (f.DiaSemana != frecuencia.DiaSemana || f.RutaId != frecuencia.RutaId));
                if (cambioDia)
                {
                    return "La frecuencia ya tiene vuelos programados; no se puede cambiar la ruta ni el día.";
                }
            }
            return null;
        }

        private void CargarListas(Frecuencia frecuencia)
        {
            var rutas = db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).ToList();
            if (frecuencia == null)
            {
                ViewBag.RutaId = new SelectList(rutas, "RutaId", "Descripcion");
                ViewBag.DiaSemana = Frecuencia.ListaDias(null);
            }
            else
            {
                ViewBag.RutaId = new SelectList(rutas, "RutaId", "Descripcion", frecuencia.RutaId);
                ViewBag.DiaSemana = Frecuencia.ListaDias(frecuencia.DiaSemana);
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
