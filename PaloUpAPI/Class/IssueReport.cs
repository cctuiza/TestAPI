using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Class
{
    public class IssueReport
    {

        public int issueId { get; set; }
        public byte pflag { get; set; }
        public System.DateTime DateIssued { get; set; }
        public string Details { get; set; }
        public string TopicAlias { get; set; }
        public string actionplan { get; set; }
        public System.DateTime targetcompletiondate { get; set; }
        public string raisedby { get; set; }
        public string responsible { get; set; }
        public string update { get; set; }
        public int FollowUpCounter { get; set; }


    

    }
}