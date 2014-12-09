using Newtonsoft.Json.Linq;
using Parse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NietVoorNiets.Controllers
{
    public class AccountController : Controller
    {
        public bool login;
        public bool loggedin;
        
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            try
            {
                await ParseUser.LogInAsync(username, password);

                login = true;
                Session["loggedin"] = login.ToString();
                return RedirectToAction("IndexDocent","Account");
                // Login was successful.
            }
            catch (Exception e)
            {
                login = false;
                return View();
                // The login failed
            }
        }
        
        public async Task<ActionResult> Create()
        {
            if (Session["loggedin"] != null && (Session["loggedin"].ToString() == "True"))
            {
                    ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
                    var klassen = await query.FindAsync();
                    return RedirectToAction("IndexDocent", "Home");
            }
            return RedirectToAction("IndexDocent", "Home");
        }

        [HttpPost]
        public async Task<ActionResult>Push(string message, string klasnaam)
        {
            bool isPushMessageSend = false;
            string postString = "";
            string urlpath = "https://api.parse.com/1/push";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlpath);
            postString = "{ \"channels\": [ \"class_" + klasnaam + "\"  ], " +
                             "\"data\" : {\"alert\":\"" + message + "\"}" +
                             "}";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = postString.Length;
            httpWebRequest.Headers.Add("X-Parse-Application-Id", "QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa");
            httpWebRequest.Headers.Add("X-Parse-REST-API-KEY", "QkXb7wOPqOt1P21M58xb3ODS2LlaXdTiDXmG2E8g");
            httpWebRequest.Method = "POST";

            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            requestWriter.Write(postString);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (Convert.ToString(jObjRes).IndexOf("true") != -1)
                {
                    isPushMessageSend = true;
                }
            }

            ParseObject pushObject = new ParseObject("Push");
            pushObject["Pushnotification"] = message;
            pushObject["Klasnaam"] = klasnaam;
            await pushObject.SaveAsync();

            //Ophalen van alle e-mail adressen
            ParseQuery<ParseObject> queryemail = ParseObject.GetQuery("Subscribers").WhereEqualTo("SubscribedClass", klasnaam); ;
            var emailadressen = await queryemail.FindAsync();
            ParseObject emails = new ParseObject("Email");
            ArrayList listOfEmails = new ArrayList();

            foreach (var obj in emailadressen)
            {
                listOfEmails.Add(obj["Email"].ToString());
            }

            //MAIL VERZENDEN
            MailMessage mail = new MailMessage("dannybrouwertest@hotmail.com", "dannybrouwertest@mailinator.com");
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "localhost";
            mail.Subject = "this is a test email.";
            mail.Body = "this is my test email body";
            
            for (int i = 0; i < listOfEmails.Count; i++) {
                mail.To.Add( listOfEmails[i].ToString() );
            }

            client.Send(mail);

            ViewBag.Emails = emailadressen;

            return RedirectToAction("Push", "Account");
        }
        public async Task<ActionResult> Push()
        {
            if (Session["loggedin"] != null && (Session["loggedin"].ToString() == "True"))
            {
                ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
                var klassen = await query.FindAsync();
                ViewBag.Message = klassen;
                return View();
            }
            return RedirectToAction("Login");
        }

        public async Task<ActionResult> Edit()
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            
            var klassen = await query.FindAsync();

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
        public ActionResult EditClassname(string klasName)
        {
            ViewBag.KlasName = klasName;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditClassname(ClassViewModel viewModel, string klasName)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var query = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", klasName);
            ParseObject klas = await query.FirstAsync();

            klas["Klasnaam"] = viewModel.Name;
            await klas.SaveAsync();

            return RedirectToAction("Edit");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string klasName)
        {
            var query = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", klasName);
            ParseObject klas = await query.FirstAsync();
            await klas.DeleteAsync();

            return RedirectToAction("Edit");
        }
        public async Task<ActionResult> IndexDocent()
        {
            if (Session["loggedin"] != null && (Session["loggedin"].ToString() == "True"))
            {
                ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
                var klassen = await query.FindAsync();
                ViewBag.Message = klassen;
                return View();
            }
            return RedirectToAction("Login", "Account");
        }
    }
}