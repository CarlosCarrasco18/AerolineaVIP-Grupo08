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

namespace Progra_03.Controllers
{
    [Authorize]
    public class ComprobantesController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        public ActionResult Index(string numero, int? id)
        {
            if (numero == null)
            {
                numero = "";
            }

            string usuarioId = User.Identity.GetUserId();
            bool verTodos = User.IsInRole("view");

            var comprobantes = db.Comprobantes
                .Include(c => c.Compra.Reserva)
                .Where(c => (verTodos || c.Compra.Reserva.UsuarioId == usuarioId) && (c.NumeroComprobante.Contains(numero) || c.Compra.Reserva.CodigoReserva.Contains(numero)))
                .OrderByDescending(c => c.FechaEmision)
                .ToList();

            Comprobante seleccionado = null;
            if (id != null)
            {
                seleccionado = comprobantes.FirstOrDefault(c => c.ComprobanteId == id);
            }
            if (seleccionado == null)
            {
                seleccionado = comprobantes.FirstOrDefault();
            }

            ViewBag.Numero = numero;
            ViewBag.Seleccionado = seleccionado;
            ViewBag.Emisor = db.DatosEmisors.FirstOrDefault();

            if (seleccionado != null)
            {
                ApplicationDbContext contexto = new ApplicationDbContext();
                var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(contexto));
                var cliente = usermanager.FindById(seleccionado.Compra.Reserva.UsuarioId);
                ViewBag.Cliente = cliente != null ? cliente.UserName : seleccionado.Compra.Reserva.UsuarioId;
                ViewBag.Pago = seleccionado.Compra.Pagoes.Where(p => p.EstadoPagoCodigo == "APROBADO" || p.EstadoPagoCodigo == "REVERSADO").OrderByDescending(p => p.FechaPago).FirstOrDefault();
                contexto.Dispose();
            }

            return View(comprobantes);
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
