using Newtonsoft.Json.Linq;
using Parse;
using System;
using System.Collections;
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

            var cookie = Request.Cookies["userName"];
            if (cookie != null)
            {
                return RedirectToAction("IndexDocent", "Account");
            }
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
            string KlasNaam = id;


            ParseQuery<ParseObject> query = ParseObject.GetQuery("Push").WhereEqualTo("Klasnaam", KlasNaam);
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
                        
            ViewBag.Klas = KlasNaam;
            ViewBag.Changes = changes;

            ParseQuery<ParseObject> query2 = ParseObject.GetQuery("Klas");

            var klassen = await query2.FindAsync();

            ArrayList ListOfClassIds = new ArrayList();
            ArrayList ListOfClassNames = new ArrayList();

            foreach (var klas in klassen)
            {
                ParseObject currentKlas = klas;
                String CurrentObjectId = currentKlas.ObjectId;
                String CurrentKlasName = currentKlas.Get<string>("Klasnaam"); ;

                ListOfClassIds.Add(CurrentObjectId);
                ListOfClassNames.Add(CurrentKlasName);

                ViewBag.KlasName = ListOfClassNames;
                ViewBag.KlasId = ListOfClassIds;
            }

            ViewBag.Message = klassen;

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