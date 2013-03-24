using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime;
using TypeItWebRole;

namespace TypeItWebRole.Controllers
{
    /// <summary>
    /// Renders result as JSON and also wraps the JSON in a call
    /// to the callback function specified in "JsonpResult.Callback".
    /// </summary>
    public class JsonpResult : JsonResult
    {
        /// <summary>
        /// Gets or sets the javascript callback function that is
        /// to be invoked in the resulting script output.
        /// </summary>
        /// <value>The callback function name.</value>
        public string Callback { get; set; }

        /// <summary>
        /// Enables processing of the result of an action method by a
        /// custom type that inherits from <see cref="T:System.Web.Mvc.ActionResult"/>.
        /// </summary>
        /// <param name="context">The context within which the
        /// result is executed.</param>
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

            if (Callback == null || Callback.Length == 0)
                Callback = context.HttpContext.Request.QueryString["callback"];

            if (Data != null)
            {
                // The JavaScriptSerializer type was marked as obsolete
                // prior to .NET Framework 3.5 SP1 
#pragma warning disable 0618
                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string ser = serializer.Serialize(Data);
                response.Write("jsonCallback" + Callback + "(" + ser + ");");
#pragma warning restore 0618
            }
        }
    }

    class TypeItImage
    {
        public string Src { get; set; }
        public bool IsImage { get; set; }
        public string Word { get; set; }
        public string RewardVideo { get; set; }
    }

    public class HomeController : Controller
    {        
        public ActionResult Index(string category)
        {

            JsonpResult jr = new JsonpResult();
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            TypeItImage img = new TypeItImage();

            //go to wikipedia to get some data!
            if (category == null)
            {
                ViewBag.Message = "Your category is empty";
            }
            else if (category == "Test")
            {
                img.Src = "http://paradoxoff.com/files/2012/12/hitachi-seaside-park-2.jpg";
                img.Word = "Flowers";
                jr.Data = img;
                return jr;
            }
            else   //real data
            {
                WikiPediaService srv = new WikiPediaService();
                YoutubeService youtubeSrv = new YoutubeService();

                //for example this will return for category = "dog"
                //["dog",["Dog","Doge of Venice","Dogma","Dog breed","Dogfight","Dogwood","Dog sled","Doge's Palace, Venice","Dog Day Afternoon","D\u014dgen"]]
                try
                {
                    List<WikiImage> imgs = srv.GetImages(category);

                    if (imgs.Count > 0)
                    {
                        img.Src = imgs[0].Uri;
                        img.Word = imgs[0].Title;
                        //grab the reward video 
                        img.RewardVideo = youtubeSrv.GetVideo(category);
                        jr.Data = img;
                        return jr;
                    }

                    ViewBag.Message = "No images were found! ";

                }
                catch (System.Exception ex)
                {
                   // System.Diagnostics.Debug.WriteLine(ex.Message);
                    ViewBag.Message = "There was an error! " + ex.Message;
                }
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            UserInfo user = new UserInfo();

            user.UploadImage("C:\\Temp\\RegSet.cfg");

            ViewBag.Message = "File Uploaded!";
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Word"></param>
        /// <param name="Duration"></param>
        /// <param name="Panic"></param>
        /// <param name="Bored"></param>
        /// <returns></returns>
        public ActionResult Stats(string UserName, string Word, int Duration, bool Panic, bool Bored)
        {
            UserInfo user = new UserInfo();

            //word, time to completion, used panic, bored, missed letters (array, count)
            List<MissedLetter> missed = new List<MissedLetter>();

            user.UploadStats(UserName, Word, Duration, Panic, Bored, missed);

            //get the latest stats and put in the message
            ViewBag.Message = "Stats: Delivered";
            return View();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Word"></param>
        /// <param name="Duration"></param>
        /// <param name="Panic"></param>
        /// <param name="Bored"></param>
        /// <returns></returns>
        public ActionResult GetStats(string UserName, string Word)
        {
            //get the latest stats and put in the message
            ViewBag.Message = "Stats: Delivered";
            UserInfo user = new UserInfo();

            List<WordStat> stats = user.GetStats(UserName, Word);

            JsonpResult jr = new JsonpResult();
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jr.Data = stats;
            return jr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Word"></param>
        /// <param name="Duration"></param>
        /// <param name="Panic"></param>
        /// <param name="Bored"></param>
        /// <returns></returns>
        public ActionResult UpdateWord(string UserName, string Word, string ImageUrl, string RewardUrl)
        {
            //get the latest stats and put in the message
            ViewBag.Message = "Stats: Updated";
            UserInfo user = new UserInfo();

            user.UpdateWord(UserName, Word, ImageUrl, RewardUrl);

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Word"></param>
        /// <param name="Duration"></param>
        /// <param name="Panic"></param>
        /// <param name="Bored"></param>
        /// <returns></returns>
        public ActionResult GetUserData(string UserName)
        {
            //get the latest stats and put in the message
            ViewBag.Message = "Stats: Updated";
            UserInfo user = new UserInfo();

            UserConfig config= user.GetUserData(UserName);

            JsonpResult jr = new JsonpResult();
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jr.Data = config;
            return jr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="highlight"></param>
        /// <param name="showWord"></param>
        /// <param name="sounds"></param>
        /// <param name="blink"></param>
        /// <returns></returns>
        public ActionResult UpdateUserData(string UserName, bool highlight, bool showWord, bool sounds, bool blink)
        {
            //get the latest stats and put in the message
            ViewBag.Message = "Stats: Updated";
            UserInfo user = new UserInfo();

            user.UpdateUserData(UserName, highlight, showWord, sounds, blink);

            return View();
        }

    }
}
