using System.Web;
using System.Web.Mvc;

namespace Progra_03
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Filters.Filtro_VERIFICA_SESSION());
        }
    }
}
