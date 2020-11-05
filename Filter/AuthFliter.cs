using PttCrawler.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PttCrawler.Filter
{
    public class AuthFliter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cookie = filterContext.HttpContext.Request.Cookies;
            
            
            if (cookie != null)
            {
                if (cookie["PttAuth"] != null)
                {
                    
                    var value = cookie["PttAuth"].Value;
                    var answer = Encrypt.DecryptAES(value).Split('|');
                    if(answer.Length==2)
                    {
                        if (answer[0] != "5566good")
                        {
                            //filterContext.Result = new ViewResult
                            //{
                            //    ViewName = "Login",
                            //    ViewData = filterContext.Controller.ViewData,
                            //    TempData = filterContext.Controller.TempData
                            //};
                            filterContext.Result = new RedirectResult("Login");
                        }
                        if (Convert.ToDateTime(answer[1]) < System.DateTime.Now)//Expired
                        {
                            filterContext.Result = new RedirectResult("Login");
                        }
                    }
                    else
                    {
                        filterContext.Result = new RedirectResult("Login");
                    }
                }
                else
                {
                    filterContext.Result = new RedirectResult("Login");
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("Login");
            }

            
            base.OnActionExecuting(filterContext);
        }
    }
}