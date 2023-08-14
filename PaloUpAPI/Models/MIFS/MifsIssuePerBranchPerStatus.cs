using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsIssuePerBranchPerStatus
    {
        public int SumOpen { get; set; }
        public int SumClosed { get; set; }
        public int SumParked { get; set; }
        public int SumDropped { get; set; }
        public int IssuedYear { get; set; }
        public int IssuedMonth { get; set; }
      
    }

    public class MifsIssuePerBranchPerStatusWeekly
    {
        public int SumOpen { get; set; }
        public int SumClosed { get; set; }
        public int SumParked { get; set; }
        public int SumDropped { get; set; }
        public int IssuedYear { get; set; }
        public int IssuedWeek { get; set; }

    }

    public class MifsGetCorporateName
    {
        public string CorporateName { get; set; }
    }

    public class MifsGetBusinessGroup
    {
        public string BusinessGroup { get; set; }
    }

    public class MifsGetIssuedYear
    {
        public int IssuedYear { get; set; }
    }
}