using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Progra_03.Models;

namespace Progra_03.Controllers
{
    [Authorize]
    public class CheckInController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        public ActionResult Index(int? id, string codigo, string documento, int? asignacionId)
        {
            Reserva reserva = null;

            if (id != null)
            {
                reserva = db.Reservas.Find(id);
                if (reserva == null || !PuedeVer(reserva))
                {
                    return HttpNotFound();
                }
            }
            else if (codigo != null || documento != null)
            {
                if (codigo == null)
                {
                    codigo = "";
                }

                if (documento == null)
                {
                    documento = "";
                }

                codigo = codigo.Trim().ToUpper();
                documento = documento.Trim().ToUpper();

                if (codigo == "" || documento == "")
                {
                    ViewBag.Message = "Ingrese el código de reserva y el número de documento del pasajero.";
                }
                else
                {
                    reserva = db.Reservas.FirstOrDefault(r => r.CodigoReserva == codigo && r.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento == documento));
                    if (reserva == null || !PuedeVer(reserva))
                    {
                        reserva = null;
                        ViewBag.Message = "No se encontró una reserva con ese código y documento a la que usted tenga acceso.";
                    }
                }
            }

            ViewBag.Codigo = codigo;
            ViewBag.Documento = documento;
            CargarCheckIn(reserva, asignacionId);
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmar(int reservaId, List<int> asignaciones)
        {
            Reserva reserva = db.Reservas.Find(reservaId);
            if (reserva == null || !PuedeVer(reserva))
            {
                return HttpNotFound();
            }

            if (asignaciones == null)
            {
                asignaciones = new List<int>();
            }

            List<string> errores = new List<string>();
            int registrados = 0;
            DateTime ahora = DateTime.Now;
            string cedula = CedulaUsuario();

            if (asignaciones.Count == 0)
            {
                errores.Add("Seleccione al menos un pasajero.");
            }

            foreach (int asignacionId in asignaciones)
            {
                ReservaPasajeroVuelo asignacion = reserva.ReservaPasajeroVueloes.FirstOrDefault(a => a.AsignacionId == asignacionId);
                if (asignacion == null)
                {
                    continue;
                }

                string nombre = asignacion.Pasajero.NombreCompleto + " (" + asignacion.Vuelo.CodigoVuelo + ")";

                if (!PuedeGestionar(reserva) && asignacion.Pasajero.NumeroDocumento != cedula)
                {
                    errores.Add(nombre + ": solo puede hacer el check-in de su propio asiento.");
                }
                else if (asignacion.EstadoAsignacionCodigo != "CONFIRMADA")
                {
                    errores.Add(nombre + ": el asiento no está pagado (CONFIRMADA).");
                }
                else if (!asignacion.Vuelo.CheckInAbierto())
                {
                    errores.Add(nombre + ": el check-in se habilita " + Vuelo.HorasCheckIn + " horas antes de la salida (" + asignacion.Vuelo.FechaHoraSalida.AddHours(-Vuelo.HorasCheckIn).ToString("dd/MM/yyyy HH:mm") + ") y se cierra a la hora de salida.");
                }
                else if (asignacion.TieneCheckIn())
                {
                    errores.Add(nombre + ": ya tiene el check-in realizado.");
                }
                else if (asignacion.Pasajero.NumeroDocumento == null || asignacion.Pasajero.NumeroDocumento.Trim() == "")
                {
                    errores.Add(nombre + ": el pasajero no tiene número de documento registrado; no se puede hacer el check-in.");
                }
                else
                {
                    CheckIn checkIn = new CheckIn();
                    checkIn.AsignacionId = asignacion.AsignacionId;
                    checkIn.FechaRegistro = ahora;
                    checkIn.EstadoCheckInCodigo = "COMPLETADO";
                    db.CheckIns.Add(checkIn);
                    registrados = registrados + 1;
                }
            }

            if (registrados > 0)
            {
                try
                {
                    db.SaveChanges();
                    ViewBag.Exito = "Check-in registrado para " + registrados + " pasajero(s). Ya puede ver la tarjeta de embarque.";
                }
                catch (Exception ex)
                {
                    errores.Add("No se pudo registrar el check-in: " + ex.GetBaseException().Message);
                }
            }

            if (errores.Count > 0)
            {
                ViewBag.Message = string.Join(" ", errores);
            }

            ViewBag.Codigo = reserva.CodigoReserva;
            ViewBag.Documento = "";
            CargarCheckIn(reserva, null);
            return View("Index", reserva);
        }

        private void CargarCheckIn(Reserva reserva, int? asignacionId)
        {
            List<ReservaPasajeroVuelo> asignaciones = new List<ReservaPasajeroVuelo>();
            ViewBag.Tarjeta = null;

            if (reserva != null)
            {
                asignaciones = reserva.ReservaPasajeroVueloes.OrderBy(a => a.Vuelo.FechaOperacion).ThenBy(a => a.Vuelo.Frecuencia.HoraSalida).ToList();

                if (!PuedeGestionar(reserva))
                {
                    string cedula = CedulaUsuario();
                    asignaciones = asignaciones.Where(a => a.Pasajero.NumeroDocumento == cedula).ToList();
                }

                if (asignacionId != null)
                {
                    ReservaPasajeroVuelo tarjeta = asignaciones.FirstOrDefault(a => a.AsignacionId == asignacionId);
                    if (tarjeta != null && tarjeta.TieneCheckIn())
                    {
                        ViewBag.Tarjeta = tarjeta;
                    }
                }
            }

            ViewBag.Asignaciones = asignaciones;
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
