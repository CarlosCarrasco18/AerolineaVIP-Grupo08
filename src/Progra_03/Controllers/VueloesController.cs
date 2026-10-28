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
    public class VueloesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Vueloes
        [Authorize(Roles = "view")]
        public ActionResult Index(DateTime? fecha, int? rutaId, string estado)
        {
            if (estado == null)
            {
                estado = "";
            }

            var vueloes = db.Vueloes
                .Include(v => v.Aeronave)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.ReservaPasajeroVueloes)
                .Where(v => v.EstadoVueloCodigo.Contains(estado) && (fecha == null || v.FechaOperacion == fecha) && (rutaId == null || v.Frecuencia.RutaId == rutaId))
                .OrderBy(v => v.FechaOperacion)
                .ThenBy(v => v.Frecuencia.HoraSalida)
                .ToList();

            ViewBag.Fecha = fecha;
            ViewBag.rutaId = new SelectList(db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).ToList(), "RutaId", "Descripcion", rutaId);
            ViewBag.estado = new SelectList(db.EstadoVueloes, "Codigo", "Codigo", estado);
            return View(vueloes);
        }

        // GET: Vueloes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            return View(vuelo);
        }

        // GET: Vueloes/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Vueloes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "VueloId,FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo")] Vuelo vuelo)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(vuelo);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Vueloes.Add(vuelo);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            CargarListas(vuelo);
            return View(vuelo);
        }

        // GET: Vueloes/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            CargarListas(vuelo);
            return View(vuelo);
        }

        // POST: Vueloes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "VueloId,FrecuenciaId,FechaOperacion,AeronaveId,EstadoVueloCodigo")] Vuelo vuelo)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(vuelo);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(vuelo).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }

            CargarListas(vuelo);
            return View(vuelo);
        }

        // GET: Vueloes/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo == null)
            {
                return HttpNotFound();
            }
            if (vuelo.ReservaPasajeroVueloes.Any() || vuelo.TramoItinerarios.Any())
            {
                ViewBag.Message = "No se puede eliminar: el vuelo tiene reservas o forma parte de un itinerario.";
            }
            return View(vuelo);
        }

        // POST: Vueloes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Vuelo vuelo = db.Vueloes.Find(id);
            if (vuelo.ReservaPasajeroVueloes.Any() || vuelo.TramoItinerarios.Any())
            {
                ViewBag.Message = "No se puede eliminar: el vuelo tiene reservas o forma parte de un itinerario.";
                return View(vuelo);
            }
            try
            {
                db.Vueloes.Remove(vuelo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(vuelo);
            }
        }

        private string Validar(Vuelo vuelo)
        {
            Frecuencia frecuencia = db.Frecuencias.Find(vuelo.FrecuenciaId);
            if (frecuencia == null)
            {
                return "Seleccione la frecuencia.";
            }

            if (Frecuencia.NumeroDelDia(vuelo.FechaOperacion) != frecuencia.DiaSemana)
            {
                return "La fecha del vuelo no corresponde al día de la frecuencia (" + frecuencia.NombreDia + ").";
            }

            if (db.Vueloes.Any(v => v.FrecuenciaId == vuelo.FrecuenciaId && v.FechaOperacion == vuelo.FechaOperacion && v.VueloId != vuelo.VueloId))
            {
                return "Ya existe un vuelo para esa frecuencia en esa fecha.";
            }

            bool tieneReservas = db.ReservaPasajeroVueloes.Any(a => a.VueloId == vuelo.VueloId && (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA"));
            int? aeronaveAnterior = null;
            int frecuenciaAnterior = vuelo.FrecuenciaId;

            if (vuelo.VueloId == 0)
            {
                if (!frecuencia.Activa)
                {
                    return "No se puede programar un vuelo sobre una frecuencia inactiva.";
                }
            }
            else
            {
                aeronaveAnterior = db.Vueloes.Where(v => v.VueloId == vuelo.VueloId).Select(v => v.AeronaveId).FirstOrDefault();
                frecuenciaAnterior = db.Vueloes.Where(v => v.VueloId == vuelo.VueloId).Select(v => v.FrecuenciaId).FirstOrDefault();
            }

            if (tieneReservas && frecuenciaAnterior != vuelo.FrecuenciaId)
            {
                return "No se puede cambiar la frecuencia de un vuelo que ya tiene asientos vendidos.";
            }

            if (tieneReservas && vuelo.AeronaveId == null)
            {
                return "No se puede quitar la aeronave de un vuelo que ya tiene asientos vendidos.";
            }

            if (vuelo.AeronaveId != null)
            {
                Aeronave aeronave = db.Aeronaves.Find(vuelo.AeronaveId);
                if (!aeronave.EstaDisponible() && aeronaveAnterior != vuelo.AeronaveId)
                {
                    return "La aeronave " + aeronave.Matricula + " no está disponible para nuevas asignaciones.";
                }

                if (vuelo.EstadoVueloCodigo != "CANCELADO")
                {
                    var otros = db.Vueloes
                        .Include(v => v.Frecuencia)
                        .Where(v => v.AeronaveId == vuelo.AeronaveId && v.VueloId != vuelo.VueloId && v.FechaOperacion == vuelo.FechaOperacion && v.EstadoVueloCodigo != "CANCELADO")
                        .ToList();

                    foreach (var otro in otros)
                    {
                        if (frecuencia.HoraSalida < otro.Frecuencia.HoraLlegada && otro.Frecuencia.HoraSalida < frecuencia.HoraLlegada)
                        {
                            return "La aeronave " + aeronave.Matricula + " ya tiene el vuelo " + otro.CodigoVuelo + " en un horario que se cruza.";
                        }
                    }
                }
            }

            return null;
        }

        private void CargarListas(Vuelo vuelo)
        {
            var frecuencias = db.Frecuencias
                .Include(f => f.Ruta.Aeropuerto)
                .Include(f => f.Ruta.Aeropuerto1)
                .OrderBy(f => f.RutaId)
                .ThenBy(f => f.DiaSemana)
                .ThenBy(f => f.HoraSalida)
                .ToList();

            var aeronaves = db.Aeronaves.Include(a => a.ModeloAeronave).OrderBy(a => a.Matricula).ToList();

            if (vuelo == null)
            {
                ViewBag.FrecuenciaId = new SelectList(frecuencias, "FrecuenciaId", "Descripcion");
                ViewBag.AeronaveId = new SelectList(aeronaves, "AeronaveId", "Descripcion");
                ViewBag.EstadoVueloCodigo = new SelectList(db.EstadoVueloes, "Codigo", "Codigo", "PROGRAMADO");
            }
            else
            {
                ViewBag.FrecuenciaId = new SelectList(frecuencias, "FrecuenciaId", "Descripcion", vuelo.FrecuenciaId);
                ViewBag.AeronaveId = new SelectList(aeronaves, "AeronaveId", "Descripcion", vuelo.AeronaveId);
                ViewBag.EstadoVueloCodigo = new SelectList(db.EstadoVueloes, "Codigo", "Codigo", vuelo.EstadoVueloCodigo);
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
