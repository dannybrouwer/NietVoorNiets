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
        // GET: Home
        public async Task<ActionResult> Index()
        {
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            return View();
        }

        public async Task<ActionResult> Create()
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

        [HttpPost]
        public ActionResult Push(string message, string klasnaam)
        {
            bool isPushMessageSend = false;
            string postString = "";
            string urlpath = "https://api.parse.com/1/push";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlpath);
            postString = "{ \"channels\": [ \"class_"+klasnaam+"\"  ], " +
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
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Push()
        {
            ParseClient.Initialize("QGr7SiC0ROlcAJsSmB4ryzFgviGcNYMPz7JlCvCa", "J8W5RChPP6N22Ah25Q1krRvTPobl4wPP2rs0BFFa");
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Klas");
            var klassen = await query.FindAsync();
            ViewBag.Message = klassen;
            return View();
        }
    }
}