﻿using System.Threading.Tasks;
using System.Web.Mvc;
using Parse;
using System.Web.UI;
using System.Net.Mail;

namespace NietVoorNiets.Controllers
{
    public class GroupController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            ViewBag.Groups = await query.FindAsync();

            if (User.IsInRole("teacher"))
            {
                return View("TeacherIndex");
            }

            return View();
        }

        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(string name)
        {
            ParseObject pushObject = new ParseObject("Klas");
            pushObject["Klasnaam"] = name;
            await pushObject.SaveAsync();

            return RedirectToAction("Index");
        }

        public ActionResult Unsubscribe()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Unsubscribe(string emailadres)
        {
            try
            {
                var query = ParseObject.GetQuery("Subscribers").WhereEqualTo("Email", emailadres);
                ParseObject email = await query.FirstAsync();
                await email.DeleteAsync();

                var mailMan = new MailMan();
                string subject = "Abonnement opgezegd";
                string body = "Uw abonnement is opgezegd.";
                mailMan.Send(subject, body, emailadres);

                return RedirectToAction("Index");
            }
            catch 
            {
                TempData["alertMessage"] = "Error";
                return View();
            }
        }

    }
}
