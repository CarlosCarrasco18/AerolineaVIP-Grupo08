using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Progra_03.Models;

namespace Progra_03
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ApplicationDbContext db = new ApplicationDbContext();

            CrearRoles(db);
            CrearSuperUsuario(db);
            CrearAdministrador(db);
            CrearCounter(db);
            CrearCliente(db);
            CrearPasajero(db);
            AsignarPermisos(db);

            db.Dispose();
        }

        private void CrearRoles(ApplicationDbContext db)
        {
            var rolemanager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            if (!rolemanager.RoleExists("view"))
            {
                rolemanager.Create(new IdentityRole("view"));
            }

            if (!rolemanager.RoleExists("create"))
            {
                rolemanager.Create(new IdentityRole("create"));
            }

            if (!rolemanager.RoleExists("edit"))
            {
                rolemanager.Create(new IdentityRole("edit"));
            }

            if (!rolemanager.RoleExists("delete"))
            {
                rolemanager.Create(new IdentityRole("delete"));
            }

            if (!rolemanager.RoleExists("Cliente"))
            {
                rolemanager.Create(new IdentityRole("Cliente"));
            }

            if (!rolemanager.RoleExists("Pasajero"))
            {
                rolemanager.Create(new IdentityRole("Pasajero"));
            }
        }

        private void CrearSuperUsuario(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("superusuario");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "superusuario",
                    Email = "superusuario@aerolineavip.com"
                };
                usermanager.Create(user, "Super2026*");
            }
        }

        private void CrearAdministrador(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("administrador");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "administrador",
                    Email = "administrador@aerolineavip.com"
                };
                usermanager.Create(user, "Admin2026*");
            }
        }

        private void CrearCounter(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("counter.uio");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "counter.uio",
                    Email = "counter.uio@aerolineavip.com"
                };
                usermanager.Create(user, "Counter2026*");
            }
        }

        private void CrearCliente(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("cliente");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = "7c1e4a20-0001-4b6e-9a01-000000000001",
                    UserName = "cliente",
                    Email = "cliente@aerolineavip.com",
                    PhoneNumber = "1710034065"
                };
                usermanager.Create(user, "Cliente2026*");
            }
        }

        private void CrearPasajero(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("pasajero");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "pasajero",
                    Email = "pasajero@aerolineavip.com",
                    PhoneNumber = "0910000008"
                };
                usermanager.Create(user, "Pasajero2026*");
            }
        }

        private void AsignarPermisos(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var superUsuario = usermanager.FindByName("superusuario");
            if (superUsuario != null)
            {
                if (!usermanager.IsInRole(superUsuario.Id, "view"))
                {
                    usermanager.AddToRole(superUsuario.Id, "view");
                }

                if (!usermanager.IsInRole(superUsuario.Id, "create"))
                {
                    usermanager.AddToRole(superUsuario.Id, "create");
                }

                if (!usermanager.IsInRole(superUsuario.Id, "edit"))
                {
                    usermanager.AddToRole(superUsuario.Id, "edit");
                }

                if (!usermanager.IsInRole(superUsuario.Id, "delete"))
                {
                    usermanager.AddToRole(superUsuario.Id, "delete");
                }
            }

            var administrador = usermanager.FindByName("administrador");
            if (administrador != null)
            {
                if (!usermanager.IsInRole(administrador.Id, "edit"))
                {
                    usermanager.AddToRole(administrador.Id, "edit");
                }

                if (!usermanager.IsInRole(administrador.Id, "delete"))
                {
                    usermanager.AddToRole(administrador.Id, "delete");
                }
            }

            var counter = usermanager.FindByName("counter.uio");
            if (counter != null)
            {
                if (!usermanager.IsInRole(counter.Id, "view"))
                {
                    usermanager.AddToRole(counter.Id, "view");
                }
            }

            var cliente = usermanager.FindByName("cliente");
            if (cliente != null)
            {
                if (!usermanager.IsInRole(cliente.Id, "Cliente"))
                {
                    usermanager.AddToRole(cliente.Id, "Cliente");
                }
            }

            var pasajero = usermanager.FindByName("pasajero");
            if (pasajero != null)
            {
                if (!usermanager.IsInRole(pasajero.Id, "Pasajero"))
                {
                    usermanager.AddToRole(pasajero.Id, "Pasajero");
                }
            }
        }
    }
}
