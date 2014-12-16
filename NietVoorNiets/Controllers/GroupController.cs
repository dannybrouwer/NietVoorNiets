using System.Threading.Tasks;
using System.Web.Mvc;
using Parse;

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
    }
}