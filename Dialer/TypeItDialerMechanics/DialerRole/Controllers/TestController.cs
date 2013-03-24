using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace DialerRole.Controllers
{
    public class JsonpWrapResult : JsonResult
    {
        public string Callback { get; set; }


        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            HttpResponseBase response = context.HttpContext.Response;
            if (!String.IsNullOrEmpty(ContentType))
                response.ContentType = ContentType;
            else
                response.ContentType = "application/javascript";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Callback == null)
            {
                throw new ArgumentException("Callback is null");
            }

            if (Data != null)
            {
                response.Write(Callback + "(" + Data + ");");
            }
        }
    }

    //This controller has materials that are in a temporary stage.
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Demo()
        {
            return View();
        }

        public ActionResult Sodiio()
        {
            var client = new WebClient();
            var content = client.DownloadString("http://sodiio.media.mit.edu/m/sodiio/app/storyscape/get_story/");
            var jwr = new JsonpWrapResult {Callback = "getSodiio"};
            jwr.Data = content;
            return jwr;
        }
    }
}
