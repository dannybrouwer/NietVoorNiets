﻿﻿using System.Threading.Tasks;
using System.Web.Mvc;
using Parse;
using System.Web.UI;
using System.Net.Mail;

namespace NietVoorNiets
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

                MailMessage mail = new MailMessage("dannybrouwertest@hotmail.com", "dannybrouwertest@mailinator.com");
                SmtpClient client = new SmtpClient();
                client.Port = 25;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Host = "localhost";
                mail.Subject = "Abonnement opgezegd";
                mail.Body = "Uw abonnement is opgezegd.";
                mail.To.Add(emailadres);
                client.Send(mail);

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