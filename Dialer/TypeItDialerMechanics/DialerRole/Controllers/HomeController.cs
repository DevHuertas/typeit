using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;

namespace DialerRole.Controllers
{
    public class HomeController : Controller
    {
        public const string SID = "AC896eea1ff45610a94b4d23f4eacd9262";
        public const string AUTHTOKEN = "cd3a52dec1c5a663d096607059d4619a";

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public JsonResult Panic()
        {
            var twilio = new TwilioRestClient(SID, AUTHTOKEN);
            //var call = twilio.InitiateOutboundCall("+1555456790", "+15551112222", "http://example.com/handleCall");
            string response = "Success";
            try
            {
                var msg = twilio.SendSmsMessage("12037598973", "14255169283",
                                                "Your son is currently indicating that he needs help with TypeIt");
            }
            catch (Exception)
            {
                response = "Failed";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
