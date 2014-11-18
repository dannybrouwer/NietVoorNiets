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
                return RedirectToAction("Create");
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
            return RedirectToAction("Login");
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
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var pushes = await query.FindAsync();

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
            ViewBag.Message = klassen;
            
            return View();
        }
    }
}