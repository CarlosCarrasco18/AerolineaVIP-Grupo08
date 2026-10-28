using Progra_03.Controllers;
using Progra_03.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Progra_03.Filters
{
    public class Filtro_VERIFICA_SESSION : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var objUser = (ApplicationUser)HttpContext.Current.Session["User"];
            if (objUser == null)
            {
                if (filterContext.Controller is AccountController == false && filterContext.Controller is HomeController == false)
                {
                    filterContext.Result = new RedirectToRouteResult("Default", new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                }
            }
            else
            {
                if (filterContext.Controller is AccountController == true && filterContext.ActionDescriptor.ActionName != "LogOff")
                {
                    filterContext.Result = new RedirectToRouteResult("Default", new RouteValueDictionary(new { controller = "Home", action = "Index" }));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
