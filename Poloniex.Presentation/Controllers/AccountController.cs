using Poloniex.Core.Domain;
using Poloniex.Presentation.Framework.Session;
using System.Web.Mvc;
using System.Web.Security;

namespace Poloniex.Presentation.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous, HttpPost]
        public ActionResult Login(User user)
        {
            if (user.Password == "HelloWorld!")
            {
                SessionManager.IsAuthenticated = true;
                FormsAuthentication.SetAuthCookie(user.UserName, true);
            }
            else
            {
                ViewBag.LoginError = "Invalid Login Credentials";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}