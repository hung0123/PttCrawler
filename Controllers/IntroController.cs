using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PttCrawler.Controllers
{
    public class IntroController:Controller
    {
        public ActionResult Intro()
        {
            return View();
        }
    }
}