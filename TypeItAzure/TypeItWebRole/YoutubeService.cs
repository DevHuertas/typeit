using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TypeItWebRole
{
    public class YoutubeService
    {
        //https://gdata.youtube.com/feeds/api/videos?q=football+-soccer&orderby=published&start-index=11&max-results=5&v=2&alt=json
        public string GetVideo(string category)
        {
            string url = "https://gdata.youtube.com/feeds/api/videos?q=" + category + "&orderby=published&max-results=5&v=2&alt=json";
            string _UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);
                string json = webClient.DownloadString(url);

                var feed = System.Web.Helpers.Json.Decode(json).feed;

                foreach (var entry in feed.entry)
                {
                    var content = entry.content;
                    return content.src; //return the first one for now!
                }
                return "";
            }
        }

    }
}