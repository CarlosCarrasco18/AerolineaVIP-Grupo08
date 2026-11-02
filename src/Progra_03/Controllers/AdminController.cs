using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    [Authorize(Roles = "view,create,edit,delete")]
    public class AdminController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();
        private ReservaServices servicioReservas = new ReservaServices();
        private VueloServices servicioVuelos = new VueloServices();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "view")]
        public ActionResult Vuelos(DateTime? desde, DateTime? hasta)
        {
            if (desde == null)
            {
                desde = DateTime.Today;
            }

            if (hasta == null)
            {
                hasta = DateTime.Today.AddDays(30);
            }

            if (desde > hasta)
            {
                ViewBag.Message = "La fecha inicial no puede ser mayor que la fecha final.";
                hasta = desde;
            }

            CargarProgramacion(desde.Value, hasta.Value);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult GenerarVuelos(DateTime? desde, DateTime? hasta)
        {
            string mensaje = null;
            if (desde == null || hasta == null)
            {
                mensaje = "Indique la fecha inicial y la fecha final.";
            }
            else if (desde > hasta)
            {
                mensaje = "La fecha inicial no puede ser mayor que la fecha final.";
            }
            else if ((hasta.Value - desde.Value).TotalDays > 365)
            {
                mensaje = "El rango no puede superar 366 días.";
            }
            else if (desde.Value.Date < DateTime.Today)
            {
                mensaje = "No se pueden generar vuelos en fechas pasadas.";
            }

            if (mensaje == null)
            {
                try
                {
                    int creados = servicioVuelos.GenerarVuelos(db, desde.Value, hasta.Value);
                    ViewBag.Exito = "Se generaron " + creados + " vuelo(s) entre el " + desde.Value.ToString("dd/MM/yyyy") + " y el " + hasta.Value.ToString("dd/MM/yyyy") + ", solo en los días de operación de cada frecuencia.";
                }
                catch (Exception ex)
                {
                    mensaje = "No se pudieron generar los vuelos: " + ex.GetBaseException().Message;
                }
            }

            ViewBag.Message = mensaje;
            if (desde == null)
            {
                desde = DateTime.Today;
            }

            if (hasta == null || hasta < desde)
            {
                hasta = desde.Value.AddDays(30);
            }

            CargarProgramacion(desde.Value, hasta.Value);
            return View("Vuelos");
        }

        [Authorize(Roles = "view")]
        public ActionResult Aeronaves()
        {
            DateTime hoy = DateTime.Today;
            DateTime limite = hoy.AddDays(7);

            var aeronaves = db.Aeronaves
                .Include(a => a.ModeloAeronave)
                .Include(a => a.EstadoAeronave)
                .OrderBy(a => a.Matricula)
                .ToList();

            ViewBag.Proximos = db.Vueloes
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.ReservaPasajeroVueloes)
                .Where(v => v.AeronaveId != null && v.FechaOperacion >= hoy && v.FechaOperacion <= limite)
                .ToList()
                .OrderBy(v => v.FechaHoraSalida)
                .ToList();

            ViewBag.SinAeronave = db.Vueloes.Count(v => v.AeronaveId == null && v.FechaOperacion >= hoy && v.EstadoVueloCodigo != "CANCELADO");
            return View(aeronaves);
        }

        [Authorize(Roles = "view")]
        public ActionResult Gestion(string buscar)
        {
            if (buscar == null)
            {
                buscar = "";
            }

            buscar = buscar.Trim();
            List<UsuarioViewModel> usuarios = new List<UsuarioViewModel>();
            List<Pasajero> pasajeros = new List<Pasajero>();
            List<Reserva> reservas = new List<Reserva>();

            if (buscar != "")
            {
                ApplicationDbContext contexto = new ApplicationDbContext();
                var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(contexto));
                var encontrados = usermanager.Users
                    .Where(u => u.UserName.Contains(buscar) || u.Email.Contains(buscar) || u.PhoneNumber.Contains(buscar))
                    .OrderBy(u => u.UserName)
                    .Take(20)
                    .ToList();

                foreach (var usuario in encontrados)
                {
                    usuarios.Add(new UsuarioViewModel
                    {
                        Id = usuario.Id,
                        UserName = usuario.UserName,
                        Email = usuario.Email,
                        Cedula = usuario.PhoneNumber,
                        Roles = string.Join(", ", usermanager.GetRoles(usuario.Id))
                    });
                }
                contexto.Dispose();

                pasajeros = db.Pasajeroes
                    .Where(p => p.NumeroDocumento.Contains(buscar) || p.Nombres.Contains(buscar) || p.Apellidos.Contains(buscar))
                    .OrderBy(p => p.Apellidos)
                    .Take(20)
                    .ToList();

                reservas = db.Reservas
                    .Where(r => r.CodigoReserva.Contains(buscar) || r.ReservaPasajeroVueloes.Any(a => a.Pasajero.NumeroDocumento.Contains(buscar)))
                    .OrderByDescending(r => r.FechaCreacion)
                    .Take(20)
                    .ToList();
            }

            ViewBag.Buscar = buscar;
            ViewBag.Usuarios = usuarios;
            ViewBag.Pasajeros = pasajeros;
            ViewBag.Reservas = reservas;
            return View();
        }

        [Authorize(Roles = "view")]
        public ActionResult Reportes(string consulta, DateTime? desde, DateTime? hasta, int? rutaId, int? aeronaveId, string estado)
        {
            servicioReservas.LiberarVencidas(db);

            if (consulta != "reservas" && consulta != "compras")
            {
                consulta = "ocupacion";
            }

            if (desde == null)
            {
                desde = DateTime.Today.AddDays(-30);
            }

            if (hasta == null)
            {
                hasta = DateTime.Today.AddDays(30);
            }

            if (estado == null)
            {
                estado = "";
            }

            if (desde > hasta)
            {
                ViewBag.Message = "La fecha inicial no puede ser mayor que la fecha final.";
                hasta = desde;
            }
            else if ((hasta.Value - desde.Value).TotalDays > 366)
            {
                ViewBag.Message = "El rango no puede superar 366 días; se ajustó la fecha final.";
                hasta = desde.Value.AddDays(366);
            }

            DateTime inicio = desde.Value.Date;
            DateTime fin = hasta.Value.Date.AddDays(1);

            if (consulta == "ocupacion")
            {
                ViewBag.Vuelos = db.Vueloes
                    .Include(v => v.Aeronave.ModeloAeronave)
                    .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                    .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                    .Include(v => v.ReservaPasajeroVueloes)
                    .Where(v => v.FechaOperacion >= inicio && v.FechaOperacion < fin)
                    .Where(v => (rutaId == null || v.Frecuencia.RutaId == rutaId) && (aeronaveId == null || v.AeronaveId == aeronaveId) && v.EstadoVueloCodigo.Contains(estado))
                    .OrderBy(v => v.FechaOperacion)
                    .ThenBy(v => v.Frecuencia.HoraSalida)
                    .ToList();
                ViewBag.estado = new SelectList(db.EstadoVueloes, "Codigo", "Codigo", estado);
            }
            else if (consulta == "reservas")
            {
                var reservas = db.Reservas
                    .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto))
                    .Include(r => r.ReservaPasajeroVueloes.Select(a => a.Vuelo.Frecuencia.Ruta.Aeropuerto1))
                    .Where(r => r.FechaCreacion >= inicio && r.FechaCreacion < fin && r.EstadoReservaCodigo.Contains(estado))
                    .Where(r => rutaId == null || r.ReservaPasajeroVueloes.Any(a => a.Vuelo.Frecuencia.RutaId == rutaId))
                    .Where(r => aeronaveId == null || r.ReservaPasajeroVueloes.Any(a => a.Vuelo.AeronaveId == aeronaveId))
                    .OrderByDescending(r => r.FechaCreacion)
                    .ToList();
                ViewBag.Reservas = reservas;
                ViewBag.Resumen = reservas.GroupBy(r => r.EstadoReservaCodigo).Select(g => g.Key + ": " + g.Count()).ToList();
                ViewBag.estado = new SelectList(db.EstadoReservas, "Codigo", "Codigo", estado);
            }
            else
            {
                ViewBag.Compras = db.Compras
                    .Include(c => c.Reserva)
                    .Include(c => c.Pagoes)
                    .Where(c => c.FechaCompra >= inicio && c.FechaCompra < fin && c.EstadoCompraCodigo.Contains(estado))
                    .Where(c => rutaId == null || c.Reserva.ReservaPasajeroVueloes.Any(a => a.Vuelo.Frecuencia.RutaId == rutaId))
                    .Where(c => aeronaveId == null || c.Reserva.ReservaPasajeroVueloes.Any(a => a.Vuelo.AeronaveId == aeronaveId))
                    .OrderByDescending(c => c.FechaCompra)
                    .ToList();
                ViewBag.estado = new SelectList(db.EstadoCompras, "Codigo", "Codigo", estado);
            }

            ViewBag.Consulta = consulta;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.Corte = DateTime.Now;
            ViewBag.rutaId = new SelectList(db.Rutas.Include(r => r.Aeropuerto).Include(r => r.Aeropuerto1).ToList(), "RutaId", "Descripcion", rutaId);
            ViewBag.aeronaveId = new SelectList(db.Aeronaves.OrderBy(a => a.Matricula).ToList(), "AeronaveId", "Matricula", aeronaveId);
            return View();
        }

        private void CargarProgramacion(DateTime desde, DateTime hasta)
        {
            DateTime inicio = desde.Date;
            DateTime fin = hasta.Date;

            ViewBag.Desde = inicio;
            ViewBag.Hasta = fin;
            ViewBag.Conflictos = servicioVuelos.ConsultarConflictos(db, inicio, fin);
            ViewBag.Dias = db.Vueloes
                .Include(v => v.Frecuencia.Ruta.Aeropuerto)
                .Include(v => v.Frecuencia.Ruta.Aeropuerto1)
                .Include(v => v.Aeronave)
                .Where(v => v.FechaOperacion >= inicio && v.FechaOperacion <= fin)
                .ToList()
                .OrderBy(v => v.FechaHoraSalida)
                .ToList();
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
