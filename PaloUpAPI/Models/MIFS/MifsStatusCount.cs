using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsStatusCount
    {
        public string mifsStatus { get; set; }
        public int mifsStatusCount { get; set; }

        public List<MifsSourceIssueCount> sourceCount { get; set; }
    }

    public class MifsSourceIssueCount{
        public string sourceAlias {get;set;}
        public int countIssues {get;set;}
    }
}