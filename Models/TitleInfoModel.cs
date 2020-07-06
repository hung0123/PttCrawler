using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PttCrawler.Models
{
    public class TitleInfoModel:BaseResultModel
    {
        public List<TitleInfo> Data { get; set; }
    }
    public class TitleInfo
    {
        public string Title { get; set; }
        public string Heat { get; set; }
        public string Date { get; set; }
        public string Author { get; set; }
        public int HeatM { get; set; }

    }
}