using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TypeItWebRole.Controllers
{
    public class StaticPageController : Controller
    {
        //
        // GET: /StaticPage/

        public ActionResult Index()
        {
            return View();
        }

    }
}
