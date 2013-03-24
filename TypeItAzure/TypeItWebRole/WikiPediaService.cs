using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TypeItWebRole
{
    public class WikiImage
    {

        public string Title { get; set; }
        public string Uri { get; set; }
    }

    /*
     * this is how it will work for commons images
        http://commons.wikimedia.org/w/api.php?action=query&prop=categories&titles=dog&prop=images
        get the image url
        http://commons.wikimedia.org/w/api.php?action=query&titles=File:2008-06-28 Ruby begging.jpg&prop=imageinfo&iiprop=url
    
     * wikipedia is easier!
           http://en.wikipedia.org/w/api.php?action=query&titles=dog&prop=images&format=json
           http://en.wikipedia.org/w/api.php?action=query&titles= + img + &prop=imageinfo&iiprop=url&format=json
     */

    public class WikiPediaService
    {
        bool _useCommons = true;

        //public async Task<string> GetGizmosAsync();
        // Implementation removed.

        //open search for related categories
        public string GetCategories(string category)
        {
            string url = "http://en.wikipedia.org/w/api.php?action=opensearch&search=" + category + "&format=json";
            string _UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

 
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);
                return webClient.DownloadString(url);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public string GetPath(string img)
        {
            string url = "";

            if (_useCommons)
            {
                url = "http://commons.wikimedia.org/w/api.php?action=query&titles=" + img + "&prop=imageinfo&iiprop=url&format=json";
            }
            else
            {
                url = "http://en.wikipedia.org/w/api.php?action=query&titles=" + img + "&prop=imageinfo&iiprop=url&format=json";
            }
            string _UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);
                string json = webClient.DownloadString(url);

                var pages = System.Web.Helpers.Json.Decode(json).query.pages;

                foreach (var page in pages)
                {
                    return page.Value.imageinfo[0].url;
                }
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public List<WikiImage> GetImages(string title)
        {
            List<WikiImage> retList = new List<WikiImage>();

            string url;
            
            if (_useCommons)
            {
                url = "http://commons.wikimedia.org/w/api.php?action=query&prop=categories&titles=" + title + "&prop=images&format=json";
            }
            else
            {
                url = "http://en.wikipedia.org/w/api.php?action=query&titles=" + title + "&prop=images&format=json";
            }
            string _UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

            using (WebClient webClient = new WebClient())
            {
                //{"query-continue":{"images":{"imcontinue":"736|Citizen-Einstein.jpg"}},"query":{"pages":{"736":{"pageid":736,"ns":0,"title":"Albert Einstein","images":[{"ns":6,"title":"File:1919 eclipse positive.jpg"},{"ns":6,"title":"File:Albert Einstein's exam of maturity grades (color2).jpg"},{"ns":6,"title":"File:Albert Einstein (Nobel).png"},{"ns":6,"title":"File:Albert Einstein Head.jpg"},{"ns":6,"title":"File:Albert Einstein as a child.jpg"},{"ns":6,"title":"File:Albert Einstein at the age of three (1882).jpg"},{"ns":6,"title":"File:Albert Einstein german.ogg"},{"ns":6,"title":"File:Albert Einstein photo 1920.jpg"},{"ns":6,"title":"File:Albert Einstein photo 1921.jpg"},{"ns":6,"title":"File:Albert Einstein signature 1934.svg"}]}}}}
                webClient.Headers.Add(HttpRequestHeader.UserAgent, _UserAgent);
                string json = webClient.DownloadString(url);

                try
                {
                    //break this down. 
                    var pages = System.Web.Helpers.Json.Decode(json).query.pages;

                    foreach (var page in pages)
                    {
                        //should be an ID
                        var images = page.Value.images;
                        foreach (var image in images)
                        {
                            WikiImage img = new WikiImage();
                            img.Title = page.Value.title;        //this is our image name!
                            img.Uri = GetPath(image.title);
                            retList.Add(img);
                        }
                    }
                }
                catch(System.Exception)
                {
                }
    
                //will return empty list if nothing found
                return retList;
            }

        }

    }
}