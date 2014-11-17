using Newtonsoft.Json.Linq;
using Parse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NietVoorNiets.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> createUser(string firstName, string email, string password)
        {
            var user = new ParseUser()
            {
                Username = firstName,
                Password = password,
                Email = email
            };

            await user.SignUpAsync();
            return View();
        }
        public ActionResult createUser()
        {
            return View();
        }
    }
}