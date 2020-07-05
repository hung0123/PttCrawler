using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PttCrawler.Models
{
    public class ArticleModel:BaseResultModel
    {
        public List<Article> Data { get; set; }
    }
    public class Article
    {
        public string Title { get; set; }
        public string Time { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string Heat { get; set; }
    }
}