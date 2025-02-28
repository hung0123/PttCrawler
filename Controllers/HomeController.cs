﻿using System;
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
using System.Text;
using PttCrawler.Base;
using System.Security.Cryptography;
using PttCrawler.Filter;

namespace PttCrawler.Controllers
{
    public class HomeController : Controller
    {
        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            LoginModel login = new LoginModel()
            {
                acc = "",
                pw = ""
            };
            return View(login);
        }
        [HttpPost]
        public ActionResult LoginAcc(LoginModel data)
        {
            string acc = data.acc;
            string pw = data.pw;
            if (pw == "0000")
            {
                string authKey = Encrypt.EncryptAES("5566good" + "|" + System.DateTime.Now.AddHours(1).ToString("yyyy/MM/dd HH:mm:ss"));

                HttpCookie pttAuth = new HttpCookie("PttAuth");
                pttAuth.Value = authKey;
                pttAuth.Expires = DateTime.Now.AddHours(1);
                Response.SetCookie(pttAuth);
                return RedirectToAction("Index");
            }
            else
            {
                LoginModel login = new LoginModel()
                {
                    acc = "帳號/密碼錯誤",
                };
                return View("Login",login);
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Response.Cookies.Clear();
            return RedirectToAction("Login");
        }
        #endregion
        [HttpGet]
        //[AuthFliter]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 取得文章內容
        /// </summary>
        /// <param name="ContentUrl">內容連結</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Article(string ContentUrl)
        {
            Content content = new Content();
            BasePtt basePtt = new BasePtt();
            var res = basePtt.RequestPtt(ContentUrl);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);

            content.Title = htmlDoc.DocumentNode.SelectNodes("//span[@class='article-meta-value']")[2].InnerText;
            content.Author = htmlDoc.DocumentNode.SelectNodes("//span[@class='article-meta-value']")[0].InnerText;
            content.ContentDateTime = htmlDoc.DocumentNode.SelectNodes("//span[@class='article-meta-value']")[3].InnerText;
            var a = htmlDoc.DocumentNode.SelectNodes("//div[@id='main-content']/text()").Select(item => item.InnerText).ToList();

            var l = htmlDoc.DocumentNode.SelectNodes("//div[@id='main-content']/a");

            List<string> linkList = new List<string>();
            if (l!=null)
            {
                linkList = htmlDoc.DocumentNode.SelectNodes("//div[@id='main-content']/a").Select(item => item.InnerText).ToList();
            }
            content.MainContent = string.Join("", a);

            
            content.Imglink = linkList.Where(item=>item.Contains("img")).ToList();
            content.Link = linkList.Where(item => !item.Contains("img")).ToList();
            //var detail = board.Select(item => item.SelectNodes("div"));
            return View(content);
        }
        /// <summary>
        /// 取得熱門列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetBoardList()
        {
            BoardListModel result = new BoardListModel();
            try
            {
                BasePtt basePtt = new BasePtt();
                var res = basePtt.RequestPtt("bbs/index.html");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(res);
                var board = htmlDoc.DocumentNode.SelectNodes("//a[@class='board']");
                var detail = board.Select(item => item.SelectNodes("div"));

                result = new BoardListModel()
                {
                    Status = "OK",
                    Msg = "",
                    Data = detail.Select(item => new BoardList()
                    {
                        BoardID = item[0].InnerText,
                        VisitCount = item[1].ChildNodes[0].InnerText,
                        BoardType = item[2].InnerText,
                        BoardHeader = item[3].InnerText
                    }).ToList()
                };

            }
            catch (Exception ex)
            {
                result.Status = "NG";
                result.Msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 文章列表
        /// </summary>
        /// <param name="boardId">目標boardid</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetArticleList(string boardId)
        {
            TitleInfoModel result = new TitleInfoModel();
            try
            {
                int count = 100;
                int infoCount = 0;
                int index = 0;
                List<TitleInfo> titleInfos = new List<TitleInfo>();
                BasePtt basePtt = new BasePtt();
                HtmlNode htmlNode = new HtmlNode(HtmlNodeType.Comment, new HtmlDocument(), 0);
                HtmlNodeCollection htmlNodes = new HtmlNodeCollection(htmlNode);
                var infos = basePtt.TraversalPtt(boardId, infoCount, count, htmlNodes);

                foreach (var item in infos)
                {
                    TitleInfo titleInfo = new TitleInfo();
                    if (item.SelectSingleNode("div[@class='title']").ChildNodes.Count <= 1)//已刪除的文章
                    {
                        continue;
                    }

                    titleInfo = new TitleInfo()
                    {
                        Index = index,
                        Heat = item.SelectSingleNode("div[@class='nrec']").ChildNodes.Count > 0 ? item.SelectSingleNode("div[@class='nrec']").ChildNodes[0].InnerText : "0",
                        Author = item.SelectSingleNode("div[@class='meta']").ChildNodes[1].InnerText,
                        Date = item.SelectSingleNode("div[@class='meta']").ChildNodes[5].InnerText,
                        Title = item.SelectSingleNode("div[@class='title']").ChildNodes[1].InnerText,
                        ContentID = item.SelectSingleNode("div[@class='title']").ChildNodes[1].GetAttributeValue("href", string.Empty)
                    };
                    titleInfo.HeatM = basePtt.ConvertHeat(titleInfo.Heat);

                    if(titleInfo.Title.Contains("公告"))//跳過公告
                    {
                        continue;
                    }

                    titleInfos.Add(titleInfo);
                    infoCount++;
                    index++;
                    if (infoCount == count)
                    {
                        break;
                    }
                }
                result.Data = titleInfos;
            }
            catch (Exception ex)
            {
                result.Status = "NG";
                result.Msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetContent(string boardId, int count)
        {
            ArticleModel result = new ArticleModel();
            try
            {
                int infoCount = 0;
                List<Article> articles = new List<Article>();
                BasePtt basePtt = new BasePtt();
                HtmlNode htmlNode = new HtmlNode(HtmlNodeType.Comment, new HtmlDocument(), 0);
                HtmlNodeCollection htmlNodes = new HtmlNodeCollection(htmlNode);
                var infos = basePtt.TraversalPtt(boardId, infoCount, count, htmlNodes);

                foreach (var item in infos)
                {
                    TitleInfo titleInfo = new TitleInfo();
                    if (item.SelectSingleNode("div[@class='title']").ChildNodes.Count <= 1)//已刪除的文章
                    {
                        continue;
                    }
                    var contentLink = item.SelectSingleNode("div[@class='title']").ChildNodes[1].Attributes["href"].Value;
                    var res = basePtt.RequestPtt(contentLink);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(res);
                    var content = htmlDoc.DocumentNode.SelectNodes("//div[@class='article-metaline']");
                    if (content == null)
                    {
                        continue;
                    }
                    var article = new Article()
                    {
                        Author = content[0].ChildNodes[1].InnerText,
                        Title = content[1].ChildNodes[1].InnerText,
                        Time = basePtt.HandleTime(content[2].ChildNodes[1].InnerText).ToString("yyyy/MM/dd"),
                        Content = content[2].NextSibling.InnerText,
                        Heat = item.SelectSingleNode("div[@class='nrec']").ChildNodes.Count > 0 ? item.SelectSingleNode("div[@class='nrec']").ChildNodes[0].InnerText : "0"
                    };
                    articles.Add(article);
                }
                result.Data = articles;
            }
            catch (Exception ex)
            {
                result.Status = "NG";
                result.Msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 文章列表Board
        [HttpGet]
        public ActionResult Board(string id)
        {
            ViewBag.BoardID = id;
            return View();
        }
        #endregion
    }
}