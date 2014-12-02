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
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
            try
            {
                await ParseUser.LogInAsync(username, password);

                login = true;
                Session["loggedin"] = login.ToString();
                return RedirectToAction("IndexDocent","Home");
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
                    ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
                    ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
                    var klassen = await query.FindAsync();
                    return View();
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

            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");

            ParseObject pushObject = new ParseObject("Push");
            pushObject["Pushnotification"] = message;
            pushObject["Klasnaam"] = klasnaam;
            await pushObject.SaveAsync();


            //Ophalen van alle e-mail adressen
            ParseQuery<ParseObject> queryemail = ParseObject.GetQuery("Subscribers");
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


                //mail.To.Add(listOfEmails);
                client.Send(mail);

            ViewBag.Emails = emailadressen;

            return RedirectToAction("Push", "Account");
        }
        public async Task<ActionResult> Push()
        {
            if (Session["loggedin"] != null && (Session["loggedin"].ToString() == "True"))
            {
                ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
                ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
                var klassen = await query.FindAsync();
                ViewBag.Message = klassen;
                return View();
            }
            return RedirectToAction("Login");
        }

        public async Task<ActionResult> Edit()
        {
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
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
        public async Task<ActionResult> EditClassname()
        {
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
            String KlasName = Request.QueryString["klasName"];
            ViewBag.KlasName = KlasName;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EditClassname(string newKlasNaam, string oldKlasNaam)
        {

            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");

            String KlasName = Request.QueryString["klasName"];
            var query = ParseObject.GetQuery("Klas").WhereEqualTo("Klasnaam", KlasName);
            ParseObject klas = await query.FirstAsync();

            if (oldKlasNaam != null)
            {
                await klas.DeleteAsync();
            }
            else if (newKlasNaam != null)
            {
                klas["Klasnaam"] = newKlasNaam;
                await klas.SaveAsync();
            }

            ViewBag.KlasName = KlasName;
            return RedirectToAction("Edit");
        }
    }
}