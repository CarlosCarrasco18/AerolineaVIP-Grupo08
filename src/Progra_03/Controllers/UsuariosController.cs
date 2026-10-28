using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Progra_03.Models;

namespace Progra_03.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private string[] roles = { "view", "create", "edit", "delete", "Cliente", "Pasajero" };

        [Authorize(Roles = "view")]
        public ActionResult Index(string buscar)
        {
            if (buscar == null)
            {
                buscar = "";
            }

            ApplicationDbContext db = new ApplicationDbContext();
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var usuarios = usermanager.Users
                .Where(u => u.UserName.Contains(buscar) || u.Email.Contains(buscar))
                .OrderBy(u => u.UserName)
                .ToList();

            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            foreach (var usuario in usuarios)
            {
                List<string> asignados = new List<string>();
                foreach (string rol in roles)
                {
                    if (usermanager.IsInRole(usuario.Id, rol))
                    {
                        asignados.Add(rol);
                    }
                }

                lista.Add(new UsuarioViewModel
                {
                    UserName = usuario.UserName,
                    Email = usuario.Email,
                    Cedula = usuario.PhoneNumber,
                    Roles = string.Join(", ", asignados)
                });
            }

            db.Dispose();

            ViewBag.Buscar = buscar;
            return View(lista);
        }
    }
}
