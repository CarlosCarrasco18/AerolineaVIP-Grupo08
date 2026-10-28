using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    [Authorize]
    public class ComprasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();

        [Authorize(Roles = "Cliente")]
        public ActionResult Pagar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            servicioReservas.LiberarVencidas(db);

            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null || reserva.UsuarioId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            if (reserva.EstadoReservaCodigo != "PENDIENTE")
            {
                ViewBag.Message = "La reserva " + reserva.CodigoReserva + " no está pendiente de pago (estado: " + reserva.EstadoReservaCodigo + ").";
            }

            PagoViewModel modelo = new PagoViewModel();
            modelo.ReservaId = reserva.ReservaId;

            CargarListas(modelo);
            ViewBag.Reserva = reserva;
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public ActionResult Pagar(PagoViewModel modelo)
        {
            Reserva reserva = db.Reservas.Find(modelo.ReservaId);
            if (reserva == null || reserva.UsuarioId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            if (reserva.EstadoReservaCodigo != "PENDIENTE" || reserva.EstaVencida())
            {
                ModelState.AddModelError("", "La reserva ya no está pendiente de pago.");
            }

            MarcaTarjeta marca = null;
            if (modelo.MarcaTarjetaCodigo != null)
            {
                marca = db.MarcaTarjetas.Find(modelo.MarcaTarjetaCodigo);
            }

            if (marca != null && modelo.NumeroTarjeta != null && modelo.NumeroTarjeta.Length != marca.LongitudNumero)
            {
                ModelState.AddModelError("NumeroTarjeta", "La tarjeta " + marca.Nombre + " debe tener " + marca.LongitudNumero + " dígitos.");
            }

            if (marca != null && modelo.CodigoSeguridad != null && modelo.CodigoSeguridad.Length != marca.LongitudCodigoSeguridad)
            {
                ModelState.AddModelError("CodigoSeguridad", "El CVV de " + marca.Nombre + " tiene " + marca.LongitudCodigoSeguridad + " dígitos.");
            }

            if (ModelState.IsValidField("Vencimiento") && modelo.Vencimiento != null)
            {
                int mes = int.Parse(modelo.Vencimiento.Substring(0, 2));
                int anio = 2000 + int.Parse(modelo.Vencimiento.Substring(3, 2));
                DateTime finDeMes = new DateTime(anio, mes, 1).AddMonths(1);
                if (finDeMes <= DateTime.Today)
                {
                    ModelState.AddModelError("Vencimiento", "La tarjeta está vencida.");
                }
            }

            if (!ModelState.IsValid)
            {
                CargarListas(modelo);
                ViewBag.Reserva = reserva;
                return View(modelo);
            }

            DateTime ahora = DateTime.Now;
            decimal total = reserva.CalcularTotal();

            Compra compra = reserva.Compras.FirstOrDefault(c => c.EstadoCompraCodigo == "PENDIENTE");
            if (compra == null)
            {
                compra = new Compra();
                compra.ReservaId = reserva.ReservaId;
                compra.MonedaCodigo = "USD";
                compra.ClaveIdempotencia = Guid.NewGuid();
                db.Compras.Add(compra);
            }

            compra.FechaCompra = ahora;
            compra.Importe = total;
            compra.EstadoCompraCodigo = "PAGADA";

            Pago pago = new Pago();
            pago.FechaPago = ahora;
            pago.Importe = total;
            pago.EstadoPagoCodigo = "APROBADO";
            pago.MetodoPagoCodigo = modelo.MetodoPagoCodigo;
            pago.MarcaTarjetaCodigo = modelo.MarcaTarjetaCodigo;
            pago.UltimosCuatroDigitos = modelo.NumeroTarjeta.Substring(modelo.NumeroTarjeta.Length - 4);
            pago.NombreTitular = modelo.NombreTitular.Trim().ToUpper();
            pago.CodigoOperacion = "OP-" + ahora.ToString("yyyyMMddHHmmss") + "-" + reserva.ReservaId;
            pago.EsSimulado = true;
            pago.MensajeResultado = "Pago aprobado.";
            compra.Pagoes.Add(pago);

            Comprobante comprobante = new Comprobante();
            comprobante.TipoComprobanteCodigo = "FACTURA";
            comprobante.NumeroComprobante = "001-001-" + reserva.ReservaId.ToString("D9");
            comprobante.FechaEmision = ahora;
            comprobante.EstadoComprobanteCodigo = "EMITIDO";
            compra.Comprobantes.Add(comprobante);

            reserva.EstadoReservaCodigo = "CONFIRMADA";
            reserva.FechaExpiracion = null;
            foreach (var asignacion in reserva.ReservaPasajeroVueloes)
            {
                asignacion.EstadoAsignacionCodigo = "CONFIRMADA";
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No se pudo registrar el pago: " + ex.GetBaseException().Message);
                CargarListas(modelo);
                ViewBag.Reserva = reserva;
                return View(modelo);
            }

            return RedirectToAction("Confirmacion", new { id = reserva.ReservaId });
        }

        public ActionResult Confirmacion(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null || reserva.UsuarioId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            Compra compra = reserva.Compras.FirstOrDefault(c => c.EstadoCompraCodigo == "PAGADA");
            if (compra == null)
            {
                return RedirectToAction("Pagar", new { id = reserva.ReservaId });
            }

            ViewBag.Compra = compra;
            ViewBag.Pago = compra.Pagoes.Where(p => p.EstadoPagoCodigo == "APROBADO").OrderByDescending(p => p.FechaPago).FirstOrDefault();
            ViewBag.Comprobante = compra.Comprobantes.Where(c => c.TipoComprobanteCodigo == "FACTURA").FirstOrDefault();
            return View(reserva);
        }

        private void CargarListas(PagoViewModel modelo)
        {
            ViewBag.MetodoPagoCodigo = new SelectList(db.MetodoPagoes.Where(m => m.Activo).ToList(), "Codigo", "NombreVisible", modelo.MetodoPagoCodigo);
            ViewBag.MarcaTarjetaCodigo = new SelectList(db.MarcaTarjetas.Where(m => m.Activa).ToList(), "Codigo", "Nombre", modelo.MarcaTarjetaCodigo);
            ViewBag.Marcas = db.MarcaTarjetas.Where(m => m.Activa).ToList();
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
