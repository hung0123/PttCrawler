using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using PttCrawler.Models;
using HtmlAgilityPack;

namespace PttCrawler.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Json("312", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetBoardList()
        {
            BaseResultModel result = new BaseResultModel();
            try
            {
                string res = "";
                string index = @"https://www.ptt.cc/bbs/index.html";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(index).Result;
                    response.EnsureSuccessStatusCode();
                    res = response.Content.ReadAsStringAsync().Result;
                }
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(res);
                var div = htmlDoc.DocumentNode.SelectNodes("//div/class");
                

            }
            catch (Exception)
            {

                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);


        }
    }
}