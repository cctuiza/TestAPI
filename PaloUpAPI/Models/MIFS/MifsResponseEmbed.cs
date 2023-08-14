using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsResponseEmbed
    {
        public int responseId { get; set; }
        public int issueId { get; set; }
        public string messagedetails { get; set; }
        public string responseby { get; set; }
        public string responsebyempid { get; set; }
        public System.DateTime responsedate { get; set; }
        public string responsebyemail { get; set; }
        public string youtubeUrl { get; set; }
        
    }
}