using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.IO;
using HtmlAgilityPack;

namespace PttCrawler.Base
{
    public class BasePtt
    {
        public enum Month
        {
            Jan = 1,
            Feb = 2,
            Mar = 3,
            Apr = 4,
            May = 5,
            Jun = 6,
            Jul = 7,
            Aug = 8,
            Sep = 9,
            Oct = 10,
            Nov = 11,
            Dec = 12
        }
        public string RequestPtt(string target)
        {
            string res = "";
            var baseAddress = new Uri("https://www.ptt.cc/");

            var targetAddress = new Uri(baseAddress, target);
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                HttpCookie httpCookie = new HttpCookie("over18", "1");
                var message = new HttpRequestMessage(HttpMethod.Get, targetAddress);
                message.Headers.Add("Cookie", "over18=1");
                //HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                HttpResponseMessage response = client.SendAsync(message).Result;
                response.EnsureSuccessStatusCode();
                res = response.Content.ReadAsStringAsync().Result;
            }
            return res;
        }
        public HtmlNodeCollection TraversalPtt(string target, int count, int targetCount, HtmlNodeCollection htmlNodes)
        {
            string res = "";
            if (count == 0)//第一筆，index
            {
                if (target.Contains("search"))
                {
                    res = RequestPtt($"{target}");
                }
                else
                {
                    res = RequestPtt($"bbs/{target}/index.html");
                }
            }
            else
            {
                res = RequestPtt($"{target}");
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);
            var next = htmlDoc.DocumentNode.SelectNodes("//div[@class='btn-group btn-group-paging']")[0].ChildNodes[3].Attributes[1].Value;

            var infos = htmlDoc.DocumentNode.SelectNodes("//div[@class='r-ent']");

            count += infos.Count;
            foreach (var info in infos)
            {
                htmlNodes.Add(info);
            }

            if (count >= targetCount)
            {
                return htmlNodes;
            }
            return TraversalPtt(next, count, targetCount, htmlNodes);
        }

        public DateTime HandleTime(string time)
        {
            string timeM ;
            int timeD;
            int timeY;
            string[] t = time.Split(' ');
            var tr = t.Where(item => item != "").ToArray();

            timeM = tr[1];
            timeD = int.Parse(tr[2]);
            timeY = int.Parse(tr[4]);


            var m = (int)Enum.Parse(typeof(Month), timeM);
            DateTime result = new DateTime(timeY, m, timeD);
            return result;
        }
    }
}


