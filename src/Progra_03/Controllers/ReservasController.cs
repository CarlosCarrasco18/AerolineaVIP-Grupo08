using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();

        [Authorize(Roles = "Cliente,Counter")]
        public ActionResult SeleccionAsiento(int? id, int? itinerarioId, int? pasajeros)
        {
            if (User.IsInRole("Counter") && Session["Cliente"] == null)
            {
                return RedirectToAction("Atencion");
            }

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
                return RedirectToAction("Disponibilidad", "Vuelos", new { id = id, itinerarioId = itinerarioId, pasajeros = pasajeros });
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
        [Authorize(Roles = "Cliente,Counter")]
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

        [Authorize(Roles = "Cliente,Counter")]
        public ActionResult Pasajeros()
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Vuelos");
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
        [Authorize(Roles = "Cliente,Counter")]
        public ActionResult Pasajeros(List<Pasajero> pasajeros)
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Vuelos");
            }

            if (pasajeros == null || pasajeros.Count != seleccion.CantidadPasajeros)
            {
                return RedirectToAction("Pasajeros");
            }

            List<int> vueloIds = seleccion.VueloIds;
            List<string> documentos = new List<string>();
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
                }

                if (documentos.Contains(pasajeros[i].NumeroDocumento))
                {
                    ModelState.AddModelError("[" + i + "].NumeroDocumento", "Este documento ya está en otro pasajero de la reserva.");
                }
                documentos.Add(pasajeros[i].NumeroDocumento);

                string errorFecha = pasajeros[i].ValidarFechaNacimiento();
                if (errorFecha != null)
                {
                    ModelState.AddModelError("[" + i + "].FechaNacimiento", errorFecha);
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

        [Authorize(Roles = "Cliente,Counter")]
        public ActionResult Confirmar()
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Vuelos");
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
        [Authorize(Roles = "Cliente,Counter")]
        public ActionResult Confirmar(bool aceptaCondiciones = false, string accion = "reservar")
        {
            var seleccion = (SeleccionReserva)Session["Seleccion"];
            if (seleccion == null)
            {
                return RedirectToAction("Buscar", "Vuelos");
            }

            servicioReservas.LiberarVencidas(db);

            List<int> vueloIds = seleccion.VueloIds;
            List<Vuelo> vuelos = db.Vueloes.Where(v => vueloIds.Contains(v.VueloId)).ToList();
            List<byte> ocupados = servicioReservas.AsientosOcupados(db, vueloIds, "CONFIRMADA");
            List<byte> reservados = servicioReservas.AsientosOcupados(db, vueloIds, "PENDIENTE");

            string usuarioId = User.Identity.GetUserId();
            if (User.IsInRole("Counter"))
            {
                var cliente = (ApplicationUser)Session["Cliente"];
                usuarioId = null;
                if (cliente != null)
                {
                    usuarioId = cliente.Id;
                }
            }

            string mensaje = null;
            if (usuarioId == null)
            {
                mensaje = "Seleccione primero al cliente en Atención al cliente.";
            }
            else if (!aceptaCondiciones)
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
            reserva.UsuarioId = usuarioId;
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
                db.Reservas.Remove(reserva);
                string detalle = ex.GetBaseException().Message;
                if (detalle.Contains("UX_RPV") || detalle.Contains("duplicate") || detalle.Contains("duplicada"))
                {
                    ViewBag.Message = "Otra persona acaba de reservar uno de los asientos elegidos. No se registró nada: vuelva a elegir los asientos.";
                }
                else
                {
                    ViewBag.Message = "No se pudo registrar la reserva: " + detalle;
                }
                CargarResumen(seleccion);
                return View();
            }

            Session["Seleccion"] = null;
            if (accion == "pagar")
            {
                return RedirectToAction("Pagar", "Compras", new { id = reserva.ReservaId });
            }
            return RedirectToAction("Confirmacion", "Compras", new { id = reserva.ReservaId });
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

            var usuarioCliente = (ApplicationUser)Session["User"];
            string cedulaCliente = "";
            if (usuarioCliente != null && usuarioCliente.PhoneNumber != null)
            {
                cedulaCliente = usuarioCliente.PhoneNumber;
            }

            ViewBag.Viajes = db.ReservaPasajeroVueloes
                .Include(a => a.Reserva)
                .Include(a => a.Pasajero)
                .Include(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto)
                .Include(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1)
                .Where(a => a.Pasajero.NumeroDocumento == cedulaCliente && a.Reserva.UsuarioId != usuarioId && a.Reserva.EstadoReservaCodigo.Contains(estado))
                .OrderBy(a => a.Vuelo.FechaOperacion)
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
            if (reserva == null || !PuedeGestionar(reserva))
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
            if (reserva == null || !PuedeCancelar(reserva))
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

        [Authorize(Roles = "Counter,edit")]
        public ActionResult CambiarAsiento(int? id, int? pasajeroId)
        {
            if (id == null || pasajeroId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }

            List<ReservaPasajeroVuelo> asignaciones = reserva.ReservaPasajeroVueloes.Where(a => a.PasajeroId == pasajeroId).ToList();
            if (asignaciones.Count == 0)
            {
                return HttpNotFound();
            }

            string mensaje = ValidarCambioAsiento(reserva, asignaciones);
            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                return View("Gestionar", reserva);
            }

            CargarCambioAsiento(reserva, asignaciones);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Counter,edit")]
        public ActionResult CambiarAsiento(int id, int pasajeroId, List<byte> asientos)
        {
            Reserva reserva = db.Reservas.Find(id);
            if (reserva == null)
            {
                return HttpNotFound();
            }

            List<ReservaPasajeroVuelo> asignaciones = reserva.ReservaPasajeroVueloes.Where(a => a.PasajeroId == pasajeroId).ToList();
            if (asignaciones.Count == 0)
            {
                return HttpNotFound();
            }

            if (asientos == null)
            {
                asientos = new List<byte>();
            }

            string mensaje = ValidarCambioAsiento(reserva, asignaciones);
            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                return View("Gestionar", reserva);
            }

            List<int> vueloIds = asignaciones.Select(a => a.VueloId).ToList();
            List<byte> ocupados = servicioReservas.AsientosOcupados(db, vueloIds, "CONFIRMADA");
            List<byte> reservados = servicioReservas.AsientosOcupados(db, vueloIds, "PENDIENTE");
            int modeloId = asignaciones[0].Vuelo.Aeronave.ModeloAeronaveId;

            if (asientos.Count != 1)
            {
                mensaje = "Elija un solo asiento nuevo.";
            }
            else if (!db.Asientoes.Any(a => a.ModeloAeronaveId == modeloId && a.NumeroAsiento == asientos[0]))
            {
                mensaje = "El asiento elegido no existe en la aeronave.";
            }
            else if (ocupados.Contains(asientos[0]) || reservados.Contains(asientos[0]))
            {
                mensaje = "El asiento elegido acaba de ser tomado. Elija otro asiento.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                CargarCambioAsiento(reserva, asignaciones);
                return View(reserva);
            }

            string anterior = asignaciones[0].CodigoAsiento;
            foreach (var asignacion in asignaciones)
            {
                asignacion.NumeroAsiento = asientos[0];
            }

            try
            {
                db.SaveChanges();
                ViewBag.Message = "Asiento de " + asignaciones[0].Pasajero.NombreCompleto + " cambiado de " + anterior + " a " + asignaciones[0].CodigoAsiento + ".";
            }
            catch (Exception ex)
            {
                foreach (var asignacion in asignaciones)
                {
                    db.Entry(asignacion).Reload();
                }
                ViewBag.Message = "No se pudo cambiar el asiento: " + ex.GetBaseException().Message;
            }

            return View("Gestionar", db.Reservas.Find(id));
        }

        [Authorize(Roles = "Counter")]
        public ActionResult Atencion(string buscar, bool terminar = false)
        {
            if (terminar)
            {
                Session["Cliente"] = null;
                Session["Seleccion"] = null;
            }

            if (buscar != null && buscar.Trim() != "")
            {
                buscar = buscar.Trim();

                ApplicationDbContext contexto = new ApplicationDbContext();
                var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(contexto));
                var encontrado = usermanager.Users.FirstOrDefault(u => u.UserName == buscar || u.PhoneNumber == buscar);

                if (encontrado == null)
                {
                    ViewBag.Message = "No existe un cliente con el usuario o la cédula " + buscar + ".";
                }
                else if (!usermanager.IsInRole(encontrado.Id, "Cliente"))
                {
                    ViewBag.Message = "La cuenta " + encontrado.UserName + " no es de tipo Cliente.";
                }
                else
                {
                    Session["Cliente"] = encontrado;
                    Session["Seleccion"] = null;
                }

                contexto.Dispose();
            }

            servicioReservas.LiberarVencidas(db);

            var cliente = (ApplicationUser)Session["Cliente"];
            List<Reserva> reservas = new List<Reserva>();
            if (cliente != null)
            {
                string clienteId = cliente.Id;
                reservas = db.Reservas
                    .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto))
                    .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                    .Where(r => r.UsuarioId == clienteId)
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToList();
            }

            ViewBag.Cliente = cliente;
            ViewBag.Buscar = buscar;
            return View(reservas);
        }

        [Authorize(Roles = "view,Counter")]
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

        private string ValidarCambioAsiento(Reserva reserva, List<ReservaPasajeroVuelo> asignaciones)
        {
            if (reserva.EstadoReservaCodigo != "PENDIENTE" && reserva.EstadoReservaCodigo != "CONFIRMADA")
            {
                return "Solo se puede cambiar el asiento de una reserva PENDIENTE o CONFIRMADA.";
            }

            if (asignaciones.Any(a => a.Vuelo.FechaHoraSalida <= DateTime.Now))
            {
                return "El vuelo ya salió; no se puede cambiar el asiento.";
            }

            if (asignaciones.Any(a => a.TieneCheckIn()))
            {
                return "El pasajero ya hizo el check-in; no se puede cambiar el asiento.";
            }

            if (asignaciones.Any(a => a.Vuelo.Aeronave == null))
            {
                return "El vuelo no tiene aeronave asignada.";
            }

            return null;
        }

        private void CargarCambioAsiento(Reserva reserva, List<ReservaPasajeroVuelo> asignaciones)
        {
            List<Vuelo> vuelos = asignaciones.Select(a => a.Vuelo).ToList();
            CargarMapa(vuelos);
            ViewBag.Elegidos = new List<byte>();
            ViewBag.Asignaciones = asignaciones;
            ViewBag.PasajeroId = asignaciones[0].PasajeroId;
        }

        private bool PuedeVer(Reserva reserva)
        {
            if (PuedeGestionar(reserva))
            {
                return true;
            }

            string cedula = CedulaUsuario();
            if (cedula != "")
            {
                return reserva.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento == cedula);
            }

            return false;
        }

        private bool PuedeGestionar(Reserva reserva)
        {
            if (User.IsInRole("view") || User.IsInRole("Counter"))
            {
                return true;
            }

            return reserva.UsuarioId == User.Identity.GetUserId();
        }

        private bool PuedeCancelar(Reserva reserva)
        {
            if (User.IsInRole("edit") || User.IsInRole("Counter"))
            {
                return true;
            }

            return reserva.UsuarioId == User.Identity.GetUserId();
        }

        private string CedulaUsuario()
        {
            var usuario = (ApplicationUser)Session["User"];
            if (usuario != null && usuario.PhoneNumber != null)
            {
                return usuario.PhoneNumber;
            }

            return "";
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
