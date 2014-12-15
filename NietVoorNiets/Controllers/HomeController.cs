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
        public async Task<ActionResult> ScheduleChanges(string id)
        {
            string KlasName = id;
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Push").WhereEqualTo("Klasnaam", KlasName);
            var changes = await query.FindAsync();

            int amountOfChanges = 0;

            foreach (var change in changes) {
                amountOfChanges++;
            }

            if (amountOfChanges == 0)
            {
                string noChangesMessage = "Er zijn geen berichten";
                ViewBag.NoChanges = noChangesMessage;
            }

            ViewBag.Klas = KlasName;
            ViewBag.Changes = changes;

            return View();
        }

        
        public async Task<ActionResult> Subscribe()
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Subscribe(string emailadres, string klasnaam)
        {
            ParseObject pushObject = new ParseObject("Subscribers");
            pushObject["Email"] = emailadres;
            pushObject["SubscribedClass"] = klasnaam;
            await pushObject.SaveAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}