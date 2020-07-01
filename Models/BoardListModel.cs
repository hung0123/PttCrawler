using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PttCrawler.Models
{
    public class BoardListModel:BaseResultModel
    {
        public List<BoardList> Data { get; set; }
    }
    public class BoardList
    {
        public string BoardID { get; set; }
        public string BoardHeader { get; set; }
        public string BoardType { get; set; }

        public string VisitCount { get; set; }
    }
}