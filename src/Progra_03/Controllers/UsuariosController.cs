using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Progra_03.Models;
using Progra_03.Services;

namespace Progra_03.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private string[] roles = { "view", "create", "edit", "delete", "Cliente", "Pasajero", "Counter" };
        private string[] perfiles = { "Administrador", "Counter", "Cliente" };

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
                    Id = usuario.Id,
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

        [Authorize(Roles = "create")]
        public ActionResult Create()
        {
            ViewBag.Rol = new SelectList(perfiles);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult Create(RegisterViewModel model)
        {
            var usermanager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!perfiles.Contains(model.Rol))
            {
                ModelState.AddModelError("Rol", "Seleccione un perfil permitido.");
            }

            if (!Pasajero.CedulaValida(model.Cedula))
            {
                ModelState.AddModelError("Cedula", "La cédula ingresada no es válida.");
            }
            else if (usermanager.Users.Any(u => u.PhoneNumber == model.Cedula))
            {
                ModelState.AddModelError("Cedula", "Ya existe una cuenta registrada con esta cédula.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.Cedula };
                var result = usermanager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    foreach (string rol in RolesDelPerfil(model.Rol))
                    {
                        usermanager.AddToRole(user.Id, rol);
                    }

                    AerolineaVIPEntities db = new AerolineaVIPEntities();
                    ReservaServices servicioReservas = new ReservaServices();
                    servicioReservas.RegistrarPasajeroCuenta(db, model.Cedula, model.Nombres, model.Apellidos, model.Email);
                    db.Dispose();

                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            ViewBag.Rol = new SelectList(perfiles, model.Rol);
            return View(model);
        }

        [Authorize(Roles = "create")]
        public ActionResult AsignarRol(string id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var usuario = usermanager.FindById(id);
            if (usuario == null)
            {
                db.Dispose();
                return HttpNotFound();
            }

            CargarUsuario(usermanager, usuario, null);
            db.Dispose();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "create")]
        public ActionResult AsignarRol(string id, string perfil)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var usuario = usermanager.FindById(id);
            if (usuario == null)
            {
                db.Dispose();
                return HttpNotFound();
            }

            string mensaje = null;
            if (usermanager.IsInRole(usuario.Id, "create"))
            {
                mensaje = "No se pueden cambiar los permisos del superusuario.";
            }
            else if (!perfiles.Contains(perfil))
            {
                mensaje = "Seleccione un perfil permitido.";
            }

            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                CargarUsuario(usermanager, usuario, perfil);
                db.Dispose();
                return View();
            }

            foreach (string rol in roles)
            {
                if (usermanager.IsInRole(usuario.Id, rol))
                {
                    usermanager.RemoveFromRole(usuario.Id, rol);
                }
            }

            foreach (string rol in RolesDelPerfil(perfil))
            {
                usermanager.AddToRole(usuario.Id, rol);
            }

            db.Dispose();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "edit")]
        public ActionResult Edit(string id)
        {
            var usermanager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var usuario = usermanager.FindById(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            if (usermanager.IsInRole(usuario.Id, "create") && !User.IsInRole("create"))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            UsuarioEditarViewModel modelo = new UsuarioEditarViewModel();
            modelo.Id = usuario.Id;
            modelo.UserName = usuario.UserName;
            modelo.Email = usuario.Email;
            modelo.Cedula = usuario.PhoneNumber;
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "edit")]
        public ActionResult Edit(UsuarioEditarViewModel modelo)
        {
            var usermanager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var usuario = usermanager.FindById(modelo.Id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            if (usermanager.IsInRole(usuario.Id, "create") && !User.IsInRole("create"))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            modelo.UserName = usuario.UserName;

            if (!Pasajero.CedulaValida(modelo.Cedula))
            {
                ModelState.AddModelError("Cedula", "La cédula ingresada no es válida.");
            }
            else if (usermanager.Users.Any(u => u.PhoneNumber == modelo.Cedula && u.Id != modelo.Id))
            {
                ModelState.AddModelError("Cedula", "Ya existe otra cuenta registrada con esta cédula.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.Email = modelo.Email;
            usuario.PhoneNumber = modelo.Cedula;
            var result = usermanager.Update(usuario);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
            return View(modelo);
        }

        [Authorize(Roles = "delete")]
        public ActionResult Delete(string id)
        {
            var usermanager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var usuario = usermanager.FindById(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            ViewBag.Message = MotivoNoEliminar(usermanager, usuario);
            return View(CrearVista(usermanager, usuario));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            var usermanager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var usuario = usermanager.FindById(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            string mensaje = MotivoNoEliminar(usermanager, usuario);
            if (mensaje != null)
            {
                ViewBag.Message = mensaje;
                return View(CrearVista(usermanager, usuario));
            }

            var result = usermanager.Delete(usuario);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Message = "No se pudo eliminar el usuario: " + string.Join(" ", result.Errors);
            return View(CrearVista(usermanager, usuario));
        }

        private string MotivoNoEliminar(UserManager<ApplicationUser> usermanager, ApplicationUser usuario)
        {
            if (usermanager.IsInRole(usuario.Id, "create"))
            {
                return "No se puede eliminar al superusuario.";
            }

            if (usuario.Id == User.Identity.GetUserId())
            {
                return "No puede eliminar su propia cuenta.";
            }

            AerolineaVIPEntities db = new AerolineaVIPEntities();
            string usuarioId = usuario.Id;
            bool tieneReservas = db.Reservas.Any(r => r.UsuarioId == usuarioId);
            db.Dispose();

            if (tieneReservas)
            {
                return "No se puede eliminar: el usuario tiene reservas registradas.";
            }

            return null;
        }

        private UsuarioViewModel CrearVista(UserManager<ApplicationUser> usermanager, ApplicationUser usuario)
        {
            UsuarioViewModel modelo = new UsuarioViewModel();
            modelo.Id = usuario.Id;
            modelo.UserName = usuario.UserName;
            modelo.Email = usuario.Email;
            modelo.Cedula = usuario.PhoneNumber;
            modelo.Roles = string.Join(", ", usermanager.GetRoles(usuario.Id));
            return modelo;
        }

        private string[] RolesDelPerfil(string perfil)
        {
            if (perfil == "Administrador")
            {
                return new string[] { "view", "edit", "delete" };
            }

            if (perfil == "Counter")
            {
                return new string[] { "Counter" };
            }

            return new string[] { "Cliente" };
        }

        private void CargarUsuario(UserManager<ApplicationUser> usermanager, ApplicationUser usuario, string perfil)
        {
            List<string> asignados = new List<string>();
            foreach (string rol in roles)
            {
                if (usermanager.IsInRole(usuario.Id, rol))
                {
                    asignados.Add(rol);
                }
            }

            ViewBag.Id = usuario.Id;
            ViewBag.UserName = usuario.UserName;
            ViewBag.Email = usuario.Email;
            ViewBag.Roles = string.Join(", ", asignados);
            ViewBag.EsSuperUsuario = asignados.Contains("create");
            ViewBag.perfil = new SelectList(perfiles, perfil);
        }
    }
}
