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
    public class ReservasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();

        public ActionResult Disponibilidad(int? id, int? itinerarioId, int? pasajeros)
        {
            if (pasajeros == null || pasajeros < 1 || pasajeros > Vuelo.CapacidadMaxima)
            {
                pasajeros = 1;
            }

            servicioReservas.LiberarVencidas(db);

            List<Vuelo> vuelos = CargarVuelos(id, itinerarioId);
            if (vuelos.Count == 0)
            {
                return HttpNotFound();
            }

            if (vuelos.Any(v => !v.PermiteVenta()))
            {
                ViewBag.Message = "Este vuelo no está disponible para la venta.";
            }

            CargarMapa(vuelos);
            ViewBag.VueloId = id;
            ViewBag.ItinerarioId = itinerarioId;
            ViewBag.Pasajeros = pasajeros;
            return View(vuelos);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult SeleccionAsiento(int? id, int? itinerarioId, int? pasajeros)
        {
            if (pasajeros == null || pasajeros < 1 || pasajeros > Vuelo.CapacidadMaxima)
            {
                pasajeros = 1;
            }

            servicioReservas.LiberarVencidas(db);

            List<Vuelo> vuelos = CargarVuelos(id, itinerarioId);
            if (vuelos.Count == 0)
            {
                return HttpNotFound();
            }

            if (vuelos.Any(v => !v.PermiteVenta()))
            {
                return RedirectToAction("Disponibilidad", new { id = id, itinerarioId = itinerarioId, pasajeros = pasajeros });
            }

            CargarMapa(vuelos);
            ViewBag.VueloId = id;
            ViewBag.ItinerarioId = itinerarioId;
            ViewBag.Pasajeros = pasajeros;
            ViewBag.Elegidos = new List<byte>();
            return View(vuelos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public ActionResult SeleccionAsiento(int? id, int? itinerarioId, int pasajeros, List<byte> asientos)
        {
            if (asientos == null)
            {
                asientos = new List<byte>();
            }

            List<Vuelo> vuelos = CargarVuelos(id, itinerarioId);
            if (vuelos.Count == 0)
            {
                return HttpNotFound();
            }

            List<int> vueloIds = vuelos.Select(v => v.VueloId).ToList();
            List<byte> ocupados = servicioReservas.AsientosOcupados(db, vueloIds, "CONFIRMADA");
            List<byte> reservados = servicioReservas.AsientosOcupados(db, vueloIds, "PENDIENTE");

            string mensaje = null;
            if (vuelos.Any(v => !v.PermiteVenta()))
            {
                mensaje = "Este vuelo ya no está disponible para la venta.";
            }
            else if (asientos.Count != pasajeros)
            {
                mensaje = "Debe elegir " + pasajeros + " asiento(s), uno por pasajero.";
            }
            else if (asientos.Any(a => ocupados.Contains(a) || reservados.Contains(a)))
            {
                mensaje = "Uno de los asientos elegidos acaba de ser tomado por otra reserva. Elija otro asiento.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                CargarMapa(vuelos);
                ViewBag.VueloId = id;
                ViewBag.ItinerarioId = itinerarioId;
                ViewBag.Pasajeros = pasajeros;
                ViewBag.Elegidos = asientos;
                return View(vuelos);
            }

            SeleccionReserva seleccion = new SeleccionReserva();
            seleccion.VueloIds = vueloIds;
            seleccion.ItinerarioId = itinerarioId;
            seleccion.CantidadPasajeros = pasajeros;
            seleccion.Asientos = asientos.OrderBy(a => a).ToList();
            Session["Seleccion"] = seleccion;

            return RedirectToAction("Pasajeros");
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult Pasajeros()
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Home");
            }

            List<Pasajero> pasajeros = new List<Pasajero>();
            for (int i = 0; i < seleccion.CantidadPasajeros; i++)
            {
                Pasajero pasajero = null;
                if (i < seleccion.PasajeroIds.Count)
                {
                    pasajero = db.Pasajeroes.Find(seleccion.PasajeroIds[i]);
                }
                if (pasajero == null)
                {
                    pasajero = new Pasajero { TipoDocumentoCodigo = "CEDULA" };
                }
                pasajeros.Add(pasajero);
            }

            CargarResumen(seleccion);
            return View(pasajeros);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public ActionResult Pasajeros(List<Pasajero> pasajeros)
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Home");
            }

            if (pasajeros == null || pasajeros.Count != seleccion.CantidadPasajeros)
            {
                return RedirectToAction("Pasajeros");
            }

            List<int> vueloIds = seleccion.VueloIds;
            List<string> documentos = new List<string>();
            List<string> mensajes = new List<string>();
            for (int i = 0; i < pasajeros.Count; i++)
            {
                if (pasajeros[i].NumeroDocumento != null)
                {
                    pasajeros[i].NumeroDocumento = pasajeros[i].NumeroDocumento.Trim().ToUpper();
                }

                string error = pasajeros[i].ValidarDocumento();
                if (error != null)
                {
                    ModelState.AddModelError("[" + i + "].NumeroDocumento", error);
                    mensajes.Add("Pasajero " + (i + 1) + ": " + error);
                }

                if (documentos.Contains(pasajeros[i].NumeroDocumento))
                {
                    ModelState.AddModelError("[" + i + "].NumeroDocumento", "Este documento ya está en otro pasajero de la reserva.");
                    mensajes.Add("Pasajero " + (i + 1) + ": este documento ya está en otro pasajero de la reserva.");
                }
                documentos.Add(pasajeros[i].NumeroDocumento);

                if (pasajeros[i].FechaNacimiento != null && pasajeros[i].FechaNacimiento > DateTime.Today)
                {
                    ModelState.AddModelError("[" + i + "].FechaNacimiento", "La fecha de nacimiento no puede ser futura.");
                    mensajes.Add("Pasajero " + (i + 1) + ": la fecha de nacimiento no puede ser futura.");
                }
            }

            List<Pasajero> registrados = new List<Pasajero>();
            if (ModelState.IsValid)
            {
                for (int i = 0; i < pasajeros.Count; i++)
                {
                    string tipo = pasajeros[i].TipoDocumentoCodigo;
                    string numero = pasajeros[i].NumeroDocumento;
                    Pasajero registrado = db.Pasajeroes.FirstOrDefault(p => p.TipoDocumentoCodigo == tipo && p.NumeroDocumento == numero);

                    if (registrado != null)
                    {
                        int pasajeroId = registrado.PasajeroId;
                        bool yaViaja = db.ReservaPasajeroVueloes.Any(a => vueloIds.Contains(a.VueloId) && a.PasajeroId == pasajeroId && (a.EstadoAsignacionCodigo == "PENDIENTE" || a.EstadoAsignacionCodigo == "CONFIRMADA"));
                        if (yaViaja)
                        {
                            ModelState.AddModelError("[" + i + "].NumeroDocumento", "Este pasajero ya tiene un asiento en este vuelo.");
                            mensajes.Add("Pasajero " + (i + 1) + ": ya tiene un asiento en este vuelo.");
                        }

                        registrado.Nombres = pasajeros[i].Nombres;
                        registrado.Apellidos = pasajeros[i].Apellidos;
                        registrado.FechaNacimiento = pasajeros[i].FechaNacimiento;
                        registrado.Correo = pasajeros[i].Correo;
                        registrado.Telefono = pasajeros[i].Telefono;
                    }
                    else
                    {
                        registrado = pasajeros[i];
                        registrado.PasajeroId = 0;
                        registrado.FechaRegistro = DateTime.Now;
                        db.Pasajeroes.Add(registrado);
                    }

                    registrados.Add(registrado);
                }
            }

            if (!ModelState.IsValid)
            {
                if (mensajes.Count > 0)
                {
                    ViewBag.Message = string.Join(" ", mensajes);
                }
                CargarResumen(seleccion);
                return View(pasajeros);
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudieron guardar los pasajeros: " + ex.GetBaseException().Message;
                CargarResumen(seleccion);
                return View(pasajeros);
            }

            seleccion.PasajeroIds = registrados.Select(p => p.PasajeroId).ToList();
            Session["Seleccion"] = seleccion;
            return RedirectToAction("Confirmar");
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult Confirmar()
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Home");
            }

            if (seleccion.PasajeroIds.Count != seleccion.CantidadPasajeros)
            {
                return RedirectToAction("Pasajeros");
            }

            CargarResumen(seleccion);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public ActionResult Confirmar(bool aceptaCondiciones = false)
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Home");
            }

            servicioReservas.LiberarVencidas(db);

            List<int> vueloIds = seleccion.VueloIds;
            List<Vuelo> vuelos = db.Vueloes.Where(v => vueloIds.Contains(v.VueloId)).ToList();
            List<byte> ocupados = servicioReservas.AsientosOcupados(db, vueloIds, "CONFIRMADA");
            List<byte> reservados = servicioReservas.AsientosOcupados(db, vueloIds, "PENDIENTE");

            string mensaje = null;
            if (!aceptaCondiciones)
            {
                mensaje = "Debe confirmar los datos y aceptar las condiciones de transporte.";
            }
            else if (vuelos.Any(v => !v.PermiteVenta()))
            {
                mensaje = "El vuelo ya no está disponible para la venta.";
            }
            else if (seleccion.Asientos.Any(a => ocupados.Contains(a) || reservados.Contains(a)))
            {
                mensaje = "Uno de los asientos elegidos ya fue tomado por otra reserva. Vuelva a elegir los asientos.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                CargarResumen(seleccion);
                return View();
            }

            DateTime ahora = DateTime.Now;
            Reserva reserva = new Reserva();
            reserva.UsuarioId = User.Identity.GetUserId();
            reserva.FechaCreacion = ahora;
            reserva.FechaExpiracion = ahora.AddMinutes(Reserva.MinutosParaPagar);
            reserva.EstadoReservaCodigo = "PENDIENTE";

            for (int i = 0; i < seleccion.PasajeroIds.Count; i++)
            {
                foreach (Vuelo vuelo in vuelos)
                {
                    ReservaPasajeroVuelo asignacion = new ReservaPasajeroVuelo();
                    asignacion.PasajeroId = seleccion.PasajeroIds[i];
                    asignacion.VueloId = vuelo.VueloId;
                    asignacion.NumeroAsiento = seleccion.Asientos[i];
                    asignacion.ImporteTramo = vuelo.Frecuencia.Ruta.TarifaReferencial;
                    asignacion.EstadoAsignacionCodigo = "PENDIENTE";
                    asignacion.FechaAsignacion = ahora;
                    reserva.ReservaPasajeroVueloes.Add(asignacion);
                }
            }

            try
            {
                db.Reservas.Add(reserva);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo registrar la reserva: " + ex.GetBaseException().Message;
                CargarResumen(seleccion);
                return View();
            }

            Session["Seleccion"] = null;
            return RedirectToAction("Pagar", "Compras", new { id = reserva.ReservaId });
        }

        [Authorize(Roles = "Cliente,Pasajero")]
        public ActionResult MisReservas(string estado, DateTime? desde, DateTime? hasta)
        {
            servicioReservas.LiberarVencidas(db);

            if (estado == null)
            {
                estado = "";
            }

            ViewBag.estado = new SelectList(db.EstadoReservas, "Codigo", "Codigo", estado);
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            if (User.IsInRole("Pasajero"))
            {
                var usuario = (ApplicationUser)Session["User"];
                string cedula = usuario.PhoneNumber;
                if (cedula == null)
                {
                    cedula = "";
                }

                var asignaciones = db.ReservaPasajeroVueloes
                    .Include(a => a.Reserva)
                    .Include(a => a.Pasajero)
                    .Include(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto)
                    .Include(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1)
                    .Where(a => a.Pasajero.NumeroDocumento == cedula && a.Reserva.EstadoReservaCodigo.Contains(estado))
                    .OrderBy(a => a.Vuelo.FechaOperacion)
                    .ToList();

                ViewBag.Cedula = cedula;
                return View("MisViajes", asignaciones);
            }

            string usuarioId = User.Identity.GetUserId();
            DateTime? fin = null;
            if (hasta != null)
            {
                fin = hasta.Value.AddDays(1);
            }

            var reservas = db.Reservas
                .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto))
                .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                .Where(r => r.UsuarioId == usuarioId && r.EstadoReservaCodigo.Contains(estado))
                .Where(r => (desde == null || r.FechaCreacion >= desde) && (fin == null || r.FechaCreacion < fin))
                .OrderByDescending(r => r.FechaCreacion)
                .ToList();

            return View(reservas);
        }

        public ActionResult Gestionar(int? id)
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
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id, bool confirmacion = false)
        {
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null || reserva.UsuarioId != User.Identity.GetUserId())
            {
                return HttpNotFound();
            }

            string mensaje = null;
            if (!confirmacion)
            {
                mensaje = "Marque la casilla para confirmar la cancelación.";
            }
            else if (reserva.EstadoReservaCodigo != "PENDIENTE" && reserva.EstadoReservaCodigo != "CONFIRMADA")
            {
                mensaje = "La reserva ya no se puede cancelar porque está " + reserva.EstadoReservaCodigo + ".";
            }
            else if (reserva.ReservaPasajeroVueloes.Any(a => a.Vuelo.FechaHoraSalida <= DateTime.Now))
            {
                mensaje = "La reserva tiene vuelos que ya salieron; no se puede cancelar.";
            }
            else if (reserva.ReservaPasajeroVueloes.Any(a => a.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO")))
            {
                mensaje = "La reserva tiene pasajeros con check-in realizado; no se puede cancelar.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                return View("Gestionar", reserva);
            }

            reserva.EstadoReservaCodigo = "CANCELADA";
            reserva.FechaExpiracion = null;

            foreach (var asignacion in reserva.ReservaPasajeroVueloes)
            {
                asignacion.EstadoAsignacionCodigo = "CANCELADA";
            }

            foreach (var compra in reserva.Compras.Where(c => c.EstadoCompraCodigo != "ANULADA"))
            {
                compra.EstadoCompraCodigo = "ANULADA";

                foreach (var pago in compra.Pagoes.Where(p => p.EstadoPagoCodigo == "APROBADO"))
                {
                    pago.EstadoPagoCodigo = "REVERSADO";
                }

                foreach (var comprobante in compra.Comprobantes)
                {
                    comprobante.EstadoComprobanteCodigo = "ANULADO";
                }
            }

            try
            {
                db.SaveChanges();
                ViewBag.Message = "La reserva " + reserva.CodigoReserva + " fue cancelada. Los asientos quedaron disponibles.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo cancelar la reserva: " + ex.GetBaseException().Message;
            }
            return View("Gestionar", reserva);
        }

        public ActionResult CheckIn(int? id, int? asignacionId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null || !PuedeVer(reserva))
            {
                return HttpNotFound();
            }

            CargarCheckIn(reserva, asignacionId);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RealizarCheckIn(int asignacionId)
        {
            ReservaPasajeroVuelo asignacion = db.ReservaPasajeroVueloes.Find(asignacionId);
            if (asignacion == null || !PuedeVer(asignacion.Reserva))
            {
                return HttpNotFound();
            }

            string mensaje = null;
            if (asignacion.EstadoAsignacionCodigo != "CONFIRMADA")
            {
                mensaje = "El check-in solo se puede realizar con el asiento pagado (CONFIRMADA).";
            }
            else if (!asignacion.Vuelo.CheckInAbierto())
            {
                mensaje = "El check-in del vuelo " + asignacion.Vuelo.CodigoVuelo + " se habilita " + Vuelo.HorasCheckIn + " horas antes de la salida (" + asignacion.Vuelo.FechaHoraSalida.AddHours(-Vuelo.HorasCheckIn).ToString("dd/MM/yyyy HH:mm") + ").";
            }
            else if (asignacion.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO"))
            {
                mensaje = "El pasajero ya realizó el check-in de este vuelo.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                CargarCheckIn(asignacion.Reserva, null);
                return View("CheckIn", asignacion.Reserva);
            }

            CheckIn checkIn = new CheckIn();
            checkIn.AsignacionId = asignacion.AsignacionId;
            checkIn.FechaRegistro = DateTime.Now;
            checkIn.EstadoCheckInCodigo = "COMPLETADO";
            try
            {
                db.CheckIns.Add(checkIn);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                db.CheckIns.Remove(checkIn);
                ViewBag.Message = "No se pudo registrar el check-in: " + ex.GetBaseException().Message;
                CargarCheckIn(asignacion.Reserva, null);
                return View("CheckIn", asignacion.Reserva);
            }

            return RedirectToAction("CheckIn", new { id = asignacion.ReservaId, asignacionId = asignacion.AsignacionId });
        }

        [Authorize(Roles = "view")]
        public ActionResult Index(string buscar, string estado)
        {
            servicioReservas.LiberarVencidas(db);

            if (buscar == null)
            {
                buscar = "";
            }

            if (estado == null)
            {
                estado = "";
            }

            var reservas = db.Reservas
                .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Pasajero))
                .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto))
                .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                .Where(r => (r.CodigoReserva.Contains(buscar) || r.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento.Contains(buscar))) && r.EstadoReservaCodigo.Contains(estado))
                .OrderByDescending(r => r.FechaCreacion)
                .ToList();

            ViewBag.Buscar = buscar;
            ViewBag.estado = new SelectList(db.EstadoReservas, "Codigo", "Codigo", estado);
            return View(reservas);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null || !PuedeVer(reserva))
            {
                return HttpNotFound();
            }
            return View(reserva);
        }

        [Authorize(Roles = "edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit([Bind(Include = "ReservaId,FechaExpiracion")] Reserva reserva)
        {
            Reserva original = db.Reservas.Find(reserva.ReservaId);
            if (original == null)
            {
                return HttpNotFound();
            }

            if (original.EstadoReservaCodigo != "PENDIENTE")
            {
                ViewBag.Message = "Solo se puede cambiar la fecha de expiración de una reserva PENDIENTE.";
                return View(original);
            }

            if (reserva.FechaExpiracion == null || reserva.FechaExpiracion <= DateTime.Now)
            {
                ViewBag.Message = "La nueva fecha de expiración debe ser posterior a la fecha actual.";
                return View(original);
            }

            try
            {
                original.FechaExpiracion = reserva.FechaExpiracion;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo guardar el registro: " + ex.GetBaseException().Message;
                return View(original);
            }
        }

        [Authorize(Roles = "delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }

            if (reserva.ReservaPasajeroVueloes.Any() || reserva.Compras.Any())
            {
                ViewBag.Message = "No se puede eliminar: la reserva tiene asientos o compras registradas. Para liberarla, el cliente debe cancelarla.";
            }
            return View(reserva);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Reserva reserva = db.Reservas.Find(id);
            if (reserva.ReservaPasajeroVueloes.Any() || reserva.Compras.Any())
            {
                ViewBag.Message = "No se puede eliminar: la reserva tiene asientos o compras registradas. Para liberarla, el cliente debe cancelarla.";
                return View(reserva);
            }

            try
            {
                db.Reservas.Remove(reserva);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "No se pudo eliminar el registro: " + ex.GetBaseException().Message;
                return View(reserva);
            }
        }

        private List<Vuelo> CargarVuelos(int? id, int? itinerarioId)
        {
            List<Vuelo> vuelos = new List<Vuelo>();

            if (itinerarioId != null)
            {
                vuelos = db.TramoItinerarios
                    .Where(t => t.ItinerarioId == itinerarioId)
                    .OrderBy(t => t.Orden)
                    .Select(t => t.Vuelo)
                    .ToList();
            }
            else if (id != null)
            {
                Vuelo vuelo = db.Vueloes.Find(id);
                if (vuelo != null)
                {
                    vuelos.Add(vuelo);
                }
            }

            return vuelos;
        }

        private void CargarMapa(List<Vuelo> vuelos)
        {
            List<int> vueloIds = vuelos.Select(v => v.VueloId).ToList();
            int modeloId = 0;
            if (vuelos[0].Aeronave != null)
            {
                modeloId = vuelos[0].Aeronave.ModeloAeronaveId;
            }

            ViewBag.Asientos = db.Asientoes.Where(a => a.ModeloAeronaveId == modeloId).OrderBy(a => a.Fila).ToList();
            ViewBag.Ocupados = servicioReservas.AsientosOcupados(db, vueloIds, "CONFIRMADA");
            ViewBag.Reservados = servicioReservas.AsientosOcupados(db, vueloIds, "PENDIENTE");
        }

        private void CargarResumen(SeleccionReserva seleccion)
        {
            List<int> vueloIds = seleccion.VueloIds;
            List<Vuelo> vuelos = db.Vueloes.Where(v => vueloIds.Contains(v.VueloId)).ToList();
            vuelos = vuelos.OrderBy(v => v.FechaHoraSalida).ToList();

            int modeloId = 0;
            if (vuelos.Count > 0 && vuelos[0].Aeronave != null)
            {
                modeloId = vuelos[0].Aeronave.ModeloAeronaveId;
            }

            var asientos = db.Asientoes.Where(a => a.ModeloAeronaveId == modeloId).ToList();
            List<string> codigos = new List<string>();
            foreach (byte numero in seleccion.Asientos)
            {
                Asiento asiento = asientos.FirstOrDefault(a => a.NumeroAsiento == numero);
                if (asiento != null)
                {
                    codigos.Add(asiento.Codigo);
                }
                else
                {
                    codigos.Add(numero.ToString());
                }
            }

            List<Pasajero> pasajeros = new List<Pasajero>();
            foreach (int pasajeroId in seleccion.PasajeroIds)
            {
                pasajeros.Add(db.Pasajeroes.Find(pasajeroId));
            }

            decimal tarifa = vuelos.Sum(v => v.Frecuencia.Ruta.TarifaReferencial);

            ViewBag.Vuelos = vuelos;
            ViewBag.Codigos = codigos;
            ViewBag.ListaPasajeros = pasajeros;
            ViewBag.Tarifa = tarifa;
            ViewBag.Total = tarifa * seleccion.CantidadPasajeros;
            ViewBag.Seleccion = seleccion;
            ViewBag.TipoDocumentoCodigo = db.TipoDocumentoIdentidads.ToList();
        }

        private void CargarCheckIn(Reserva reserva, int? asignacionId)
        {
            var asignaciones = reserva.ReservaPasajeroVueloes.OrderBy(a => a.Vuelo.FechaOperacion).ThenBy(a => a.Vuelo.Frecuencia.HoraSalida).ToList();

            if (User.IsInRole("Pasajero"))
            {
                var usuario = (ApplicationUser)Session["User"];
                asignaciones = asignaciones.Where(a => a.Pasajero.NumeroDocumento == usuario.PhoneNumber).ToList();
            }

            ViewBag.Asignaciones = asignaciones;
            ViewBag.Tarjeta = null;

            if (asignacionId != null)
            {
                ReservaPasajeroVuelo tarjeta = asignaciones.FirstOrDefault(a => a.AsignacionId == asignacionId);
                if (tarjeta != null && tarjeta.CheckIns.Any(c => c.EstadoCheckInCodigo == "COMPLETADO"))
                {
                    ViewBag.Tarjeta = tarjeta;
                }
            }
        }

        private bool PuedeVer(Reserva reserva)
        {
            if (User.IsInRole("view"))
            {
                return true;
            }

            if (reserva.UsuarioId == User.Identity.GetUserId())
            {
                return true;
            }

            var usuario = (ApplicationUser)Session["User"];
            if (usuario != null && usuario.PhoneNumber != null)
            {
                return reserva.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento == usuario.PhoneNumber);
            }

            return false;
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
