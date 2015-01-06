using Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NietVoorNiets
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
        }

        protected void Application_AuthenticateRequest()
        {
            var cookie = Request.Cookies["userName"];
            if (cookie != null)
            {
                var identity = new GenericIdentity(cookie.Value);
                var principal = new GenericPrincipal(identity, new string[] { "teacher" });
                HttpContext.Current.User = principal;
            }
        }
    }
}
