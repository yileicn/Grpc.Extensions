using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace FM.GrpcDashboard.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signin")]
        public ActionResult SignIn(string returnUrl)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public ActionResult SignOut()
        {
             return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/AccessDenied")]
        public ActionResult AccessDenied(string returnUrl)
        {
            return Ok("无访问权限");
        }
    }
}