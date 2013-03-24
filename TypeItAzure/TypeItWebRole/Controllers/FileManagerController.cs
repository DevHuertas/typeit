using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TypeItWebRole.Controllers
{
    public class FileManagerController : Controller
    {
        
        [HttpPost]
        public ActionResult Upload()
        {
            var uI = new UserInfo();
            string imageUrl = String.Empty;
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null)
                {
                    imageUrl = uI.UploadImage(file.InputStream, file.FileName);
                }
            }
            var word = Request.Form["Word"];
            var rewardUrl = Request.Form["reward"];
            var c = new HomeController();
            c.UpdateWord("Jenny", word, imageUrl, rewardUrl);


            return RedirectToAction("Index", "StaticPage");
        }

    }
}
