using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsActivityLogModel
    {
        public string eventCode { get; set; }
        public DateTime eventDate { get; set; }
        public string eventType { get; set; }
        public string eventDescription { get; set; }
    }
}