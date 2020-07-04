using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.IO;

namespace PttCrawler.Base
{
    public class PttRequest
    {
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
    }
}