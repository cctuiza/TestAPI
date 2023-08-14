using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsDashboard
    {
        public int? topicId { get; set; }
        public string topicDes { get; set; }
        public int? iOpen { get; set; }
        public int? iPark { get; set; }
        public int? iClose { get; set; }
        public int? iDrop { get; set; }
    }
}