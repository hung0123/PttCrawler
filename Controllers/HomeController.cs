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
            return Json("", JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetBoardList()
        {
            BoardListModel result = new BoardListModel();
            try
            {
                PttRequest pttRequest = new PttRequest();
                var res = pttRequest.RequestPtt("bbs/index.html");
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
        [HttpGet]
        public ActionResult GetTitleInfo(string boardId, int count)
        {
            TitleInfoModel result = new TitleInfoModel();
            try
            {
                int countA = 0;
                List<TitleInfo> titleInfos = new List<TitleInfo>();
                string res = "";

                HandleTitleInfo(boardId, 0, count, titleInfos);
                result.Data = titleInfos;
            }
            catch (Exception ex)
            {
                result.Status = "NG";
                result.Msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 處理主資訊
        /// </summary>
        /// <param name="target"></param>
        /// <param name="count"></param>
        /// <param name="targetCount"></param>
        private void HandleTitleInfo(string target, int count, int targetCount, List<TitleInfo> titleInfos)
        {
            PttRequest pttRequest = new PttRequest();
            string res = "";
            if (count == 0)//第一筆，index
            {
                res = pttRequest.RequestPtt($"bbs/{target}/index.html");
            }
            else
            {
                res = pttRequest.RequestPtt($"{target}");
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(res);
            var next = htmlDoc.DocumentNode.SelectNodes("//div[@class='btn-group btn-group-paging']")[0].ChildNodes[3].Attributes[1].Value;

            var infos = htmlDoc.DocumentNode.SelectNodes("//div[@class='r-ent']");

            foreach (var item in infos)
            {
                TitleInfo titleInfo = new TitleInfo();
                if (item.SelectSingleNode("div[@class='title']").ChildNodes.Count <= 1)//已刪除的文章
                {
                    continue;
                }

                titleInfo = new TitleInfo()
                {
                    Popular = item.SelectSingleNode("div[@class='nrec']").ChildNodes.Count > 0 ? item.SelectSingleNode("div[@class='nrec']").ChildNodes[0].InnerText : "0",
                    Author = item.SelectSingleNode("div[@class='meta']").ChildNodes[1].InnerText,
                    Date = item.SelectSingleNode("div[@class='meta']").ChildNodes[5].InnerText,
                    Title = item.SelectSingleNode("div[@class='title']").ChildNodes[1].InnerText,
                };


                titleInfos.Add(titleInfo);
                count++;
                if (count == targetCount)
                {
                    break;
                }
            }
            if (count == targetCount)
            {
                return;
            }
            HandleTitleInfo(next, count, targetCount, titleInfos);
        }

    }
}