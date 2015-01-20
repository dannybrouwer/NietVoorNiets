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

                var cookie = new HttpCookie("userName");
                cookie.Value = username;
                cookie.Expires = DateTime.Now.AddHours(1);
                Response.Cookies.Set(cookie);
                return RedirectToAction("IndexDocent", "Account");
                // Login was successful.
            }
            catch (Exception e)
            {
                login = false;
                return View();
                // The login failed
            }
        }

        [Authorize(Roles = "teacher")]
        public async Task<ActionResult> Create()
        {

            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            return View();
        }

        [Authorize(Roles = "teacher")]
        [HttpPost]
        public async Task<ActionResult> Create(string KlasNaam)
        {
            ParseObject pushObject = new ParseObject("Klas");
            pushObject["Klasnaam"] = KlasNaam;
            await pushObject.SaveAsync();
            return RedirectToAction("IndexDocent", "Account");
        }

        [Authorize(Roles = "teacher")]
        public async Task<ActionResult> Push(string id)
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            ViewBag.ID = id;
            return View();
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


            ParseQuery<ParseObject> queryKlas = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", klasnaam); ;
            ParseObject Klas = await queryKlas.FirstAsync();
            ParseObject klas = new ParseObject("Klas");
            ParseObject pushObject = new ParseObject("Push");
            pushObject["Pushnotification"] = message;
            pushObject["KlasId"] = Klas.ObjectId;
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
            mail.Subject = "Rooster wijziging";
            mail.Body = message.ToString();
            
            for (int i = 0; i < listOfEmails.Count; i++) {
                mail.To.Add( listOfEmails[i].ToString() );
            }

            //client.Send(mail);
            ViewBag.Emails = emailadressen;
            return RedirectToAction("IndexDocent", "Account");
        }

        public ActionResult EditClassname(string klasName)
        {
             var cookie = Request.Cookies["userName"];
             if (cookie == null)
             {
                 ViewBag.KlasName = klasName;
                 return RedirectToAction("Login", "Account");
             }
             else
             {
                 ViewBag.KlasName = klasName;
             }
                 return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditClassname(ClassViewModel viewModel, string klasName, string verwijderen)
        {
            if (!ModelState.IsValid)
            {
                if (verwijderen != null)
                {
                    var query = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", klasName);
                    ParseObject klas = await query.FirstAsync();
                    await klas.DeleteAsync();

                    return RedirectToAction("IndexDocent", "Account");
                }
                return View(viewModel);
            }
            else {
                var query = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", klasName);
                ParseObject klas = await query.FirstAsync();

                klas["Klasnaam"] = viewModel.Name;
                await klas.SaveAsync();

                return RedirectToAction("IndexDocent", "Account");
            }
        }

        public ActionResult SignOut()
        {
            var cookie = Request.Cookies["userName"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Set(cookie);
            }
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles= "teacher")]
        public async Task<ActionResult> IndexDocent()
        {

            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> MultiPush(string message, string[] klasnaam)
        {
            foreach (string group in klasnaam)
            {
            // Get all groups
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            
            // Send push
            bool isPushMessageSend = false;
            string postString = "";
            string urlpath = "https://api.parse.com/1/push";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlpath);
            postString = "{ \"channels\": [ \"class_" + group + "\"  ], " +
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

            ParseQuery<ParseObject> queryKlas = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", group); ;
            ParseObject Klas = await queryKlas.FirstAsync();
            ParseObject klas = new ParseObject("Klas");
            ParseObject pushObject = new ParseObject("Push");
            pushObject["Pushnotification"] = message;
            pushObject["KlasId"] = Klas.ObjectId;
            pushObject["Klasnaam"] = group;
            await pushObject.SaveAsync();

            //Ophalen van alle e-mail adressen
            ParseQuery<ParseObject> queryemail = ParseObject.GetQuery("Subscribers").WhereEqualTo("SubscribedClass", group); ;
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
            mail.Subject = "Rooster wijziging";
            mail.Body = message.ToString();

            for (int i = 0; i < listOfEmails.Count; i++)
            {
                mail.To.Add(listOfEmails[i].ToString());
            }

            //client.Send(mail);
            ViewBag.Emails = emailadressen;
            }
            return RedirectToAction("IndexDocent", "Account");
        }

        public async Task<ActionResult> MultiPush()
        {
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;

            return View();
        }
    }
}