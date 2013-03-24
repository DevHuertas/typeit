using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DialerRole.Models;
using Microsoft.WindowsAzure;
using Twilio;
using Twilio.TwiML;
using Twilio.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;


namespace DialerRole.Controllers
{
    public class HomeController : Controller
    {
        public const string Sid = "AC896eea1ff45610a94b4d23f4eacd9262";
        public const string Authtoken = "cd3a52dec1c5a663d096607059d4619a";
        public const string Appphonenumber = "12037598973";
        public Dictionary<int, string> UserIdVsStringMap; 

        public HomeController()
        {
            UserIdVsStringMap = new Dictionary<int, string> {{1, "14255169283"}};
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        
        public JsonResult Panic(int userId=1)
        {
            const string msg = "Your son is currently indicating that he needs help with TypeIt";
            var response = SendSms(userId, msg);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private string SendSms(int userId, string msg)
        {
            var twilio = new TwilioRestClient(Sid, Authtoken);

            var response = "Success";
            try
            {
                twilio.SendSmsMessage(Appphonenumber, UserIdVsStringMap[userId],
                                      msg);
            }
            catch (Exception)
            {
                response = "Failed";
            }
            return response;
        }

        public JsonResult RequestNewContent(int userId=1)
        {
            var material = new AvailableMaterials();
            var topicNames = material.GetTop3MaterialsForId(userId);
            const string msgFormat =
                "Your son needs a new topic select an option \n 1.{0} \n 2.{1} \n 3.{2}";
            //TODO: Break down since for 130 chars if we are using the trial or 160 if not right now it only sends one message.
            var msg = String.Format(msgFormat, topicNames[0], topicNames[1], topicNames[2]);
            var response = SendSms(userId, msg);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public TwilioResponse ManageSms()
        {
            return ManageSms(new SmsRequest() {Body = "1"});
        }

        [HttpPost]
        public TwilioResponse ManageSms(SmsRequest request)
        {
            int controlNumber;
            string type;
            if (Int32.TryParse(request.Body, out controlNumber))
            {
                switch (controlNumber)
                {
                    case 1:
                    case 2:
                    case 3:
                        var materials = new AvailableMaterials();
                        type = materials.GetTop3MaterialsForId(1)[controlNumber-1];
                        break;
                    default:
                        throw new Exception("Response contained other characters that were not numbers");
                        break;
                }
            }
            else
            {
                //TODO:Handle This prettier
                throw new Exception("Response contained other characters that were not numbers");
            }
            //TODO: Save The information of what type is and create and output for it.
            
            var blockBlob = CloudBlockBlob();
            byte[] byteArray = Encoding.UTF8.GetBytes(type);

            using (var stream = new MemoryStream(byteArray))
            {
                blockBlob.UploadFromStream(stream);
            }

            //System.IO.File.WriteAllText("holdData.txt", type);
            var re = new TwilioResponse();
            re.Sms("Message Received");
            return re;
        }

        private static CloudBlockBlob CloudBlockBlob()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("mycontainer");
            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("myblob");
            return blockBlob;
        }

        public JsonResult RequestForNewContent(int Id)
        {
            string response = "noun";
            var blob = CloudBlockBlob();
            if (blob.Exists())
            {
                Stream n = new MemoryStream();
                blob.DownloadToStream(n);
                n.Position = 0;
                var reader = new StreamReader(n);
                response = reader.ReadToEnd();
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        public void Clean()
        {
            var blob = CloudBlockBlob();
            if (blob.Exists())
            {
                blob.Delete();
            }
        }
    }
}
