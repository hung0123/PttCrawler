using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PttCrawler.Models
{
    public class Content
    {
        public string Title { get; set; }
        public string ContentDateTime { get; set; }
        public string Author { get; set; }
        public string MainContent { get; set; }
        public List<string> link { get; set; }
        public List<Comment> Comments { get; set; }
    }
    public class Comment
    {
        public string Flag { get; set; }
        public string Memo { get; set; }
        public string CommentTime { get; set; }
    }
}