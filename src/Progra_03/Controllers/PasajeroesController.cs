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
    public class PasajeroesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Pasajeroes
        [Authorize(Roles = "view")]
        public ActionResult Index(string buscar)
        {
            if (buscar == null)
            {
                buscar = "";
            }

            var pasajeros = db.Pasajeroes
                .Include(p => p.TipoDocumentoIdentidad)
                .Where(p => p.NumeroDocumento.Contains(buscar) || p.Nombres.Contains(buscar) || p.Apellidos.Contains(buscar))
                .OrderBy(p => p.Apellidos)
                .ToList();

            ViewBag.Buscar = buscar;
            return View(pasajeros);
        }

        // GET: Pasajeroes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            return View(pasajero);
        }

        // GET: Pasajeroes/Create
        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Pasajeroes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create([Bind(Include = "PasajeroId,TipoDocumentoCodigo,NumeroDocumento,Nombres,Apellidos,FechaNacimiento,Correo,Telefono,FechaRegistro")] Pasajero pasajero)
        {
            if (pasajero.NumeroDocumento != null)
            {
                pasajero.NumeroDocumento = pasajero.NumeroDocumento.Trim().ToUpper();
            }

            if (pasajero.PasajeroId == 0)
            {
                pasajero.FechaRegistro = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(pasajero);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Pasajeroes.Add(pasajero);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(pasajero);

            return View(pasajero);
        }

        // GET: Pasajeroes/Edit/5
        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            CargarListas(pasajero);

            return View(pasajero);
        }

        // POST: Pasajeroes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "PasajeroId,TipoDocumentoCodigo,NumeroDocumento,Nombres,Apellidos,FechaNacimiento,Correo,Telefono,FechaRegistro")] Pasajero pasajero)
        {
            if (pasajero.NumeroDocumento != null)
            {
                pasajero.NumeroDocumento = pasajero.NumeroDocumento.Trim().ToUpper();
            }

            if (pasajero.PasajeroId == 0)
            {
                pasajero.FechaRegistro = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = Validar(pasajero);
            }

            if (ModelState.IsValid && ViewBag.Message == null)
            {
                try
                {
                    db.Entry(pasajero).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                }
            }
            CargarListas(pasajero);

            return View(pasajero);
        }

        // GET: Pasajeroes/Delete/5
        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero == null)
            {
                return HttpNotFound();
            }
            if (pasajero.ReservaPasajeroVueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: el pasajero tiene reservas registradas.";
            }
            return View(pasajero);
        }

        // POST: Pasajeroes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Pasajero pasajero = db.Pasajeroes.Find(id);
            if (pasajero.ReservaPasajeroVueloes.Any())
            {
                ViewBag.Message = "No se puede eliminar: el pasajero tiene reservas registradas.";
                return View(pasajero);
            }
            try
            {
                db.Pasajeroes.Remove(pasajero);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(pasajero);
            }
        }

        private string Validar(Pasajero pasajero)
        {
            string documentoAnterior = db.Pasajeroes.Where(p => p.PasajeroId == pasajero.PasajeroId).Select(p => p.NumeroDocumento).FirstOrDefault();
            if (documentoAnterior != pasajero.NumeroDocumento)
            {
                string error = pasajero.ValidarDocumento();
                if (error != null)
                {
                    return error;
                }
            }

            if (db.Pasajeroes.Any(p => p.TipoDocumentoCodigo == pasajero.TipoDocumentoCodigo && p.NumeroDocumento == pasajero.NumeroDocumento && p.PasajeroId != pasajero.PasajeroId))
            {
                return "Ya existe un pasajero con ese documento.";
            }

            if (pasajero.FechaNacimiento != null && pasajero.FechaNacimiento > DateTime.Today)
            {
                return "La fecha de nacimiento no puede ser futura.";
            }
            return null;
        }

        private void CargarListas(Pasajero pasajero)
        {
            if (pasajero == null)
            {
                ViewBag.TipoDocumentoCodigo = new SelectList(db.TipoDocumentoIdentidads, "Codigo", "Descripcion");
            }
            else
            {
                ViewBag.TipoDocumentoCodigo = new SelectList(db.TipoDocumentoIdentidads, "Codigo", "Descripcion", pasajero.TipoDocumentoCodigo);
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
