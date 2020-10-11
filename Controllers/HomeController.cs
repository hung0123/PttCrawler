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
using System.Text;
using PttCrawler.Base;

namespace PttCrawler.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Article()
        {
            return View();
        }
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
        /// 
        /// </summary>
        /// <param name="boardId">目標boardid</param>
        /// <param name="count">要幾筆</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetTitleInfo(string boardId, int? count,string query,int? filterHeat)
        {
            TitleInfoModel result = new TitleInfoModel();
            try
            {
                if(query != null)
                {
                    boardId = $"bbs/{boardId}/search?page=1&q={query}";
                }
                if(count==null)
                {
                    count = 50;
                }
                int infoCount = 0;
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
                        Heat = item.SelectSingleNode("div[@class='nrec']").ChildNodes.Count > 0 ? item.SelectSingleNode("div[@class='nrec']").ChildNodes[0].InnerText : "0",
                        Author = item.SelectSingleNode("div[@class='meta']").ChildNodes[1].InnerText,
                        Date = item.SelectSingleNode("div[@class='meta']").ChildNodes[5].InnerText,
                        Title = item.SelectSingleNode("div[@class='title']").ChildNodes[1].InnerText,
                    };
                    titleInfo.HeatM = basePtt.ConvertHeat(titleInfo.Heat);
                    if (titleInfo.HeatM < filterHeat)
                    {
                        continue;
                    }

                    titleInfos.Add(titleInfo);
                    infoCount++;
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
                    if(content==null)
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
    }
}