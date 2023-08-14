using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsIssue
    {
        public int issueId { get; set; }
        public string hashcode { get; set; }
        public int topicId { get; set; }
        public string issuedetails { get; set; }
        public List<MifsIsssueActionPlan> actionplan { get; set; }
        public System.DateTime issuedate { get; set; }
        public Nullable<int> fromtopicId { get; set; }
        public System.DateTime targetcompletiondate { get; set; }
        public string raisedby { get; set; }
        public string raisedbyemail { get; set; }
        public string raisedbyempid { get; set; }
        public string raisedbyphone { get; set; }
        public string responsible { get; set; }
        public string responemail { get; set; }
        public string responempid { get; set; }
        public string responphone { get; set; }
        public int followfrequency { get; set; }
        public byte followmail { get; set; }
        public byte followsms { get; set; }
        public string status { get; set; }
        public string newstatus { get; set; }
        public byte acknowledgedbysource { get; set; }
        public Nullable<System.DateTime> acknowledgesourcedate { get; set; }
        public byte acknowledgebyrespondent { get; set; }
        public Nullable<System.DateTime> acknowledgerespondentdate { get; set; }
        public int fupcounter { get; set; }
        public Nullable<System.DateTime> parkdate { get; set; }
        public Nullable<System.DateTime> resumedate { get; set; }
        public Nullable<System.DateTime> firstsenddate { get; set; }
        public Nullable<System.DateTime> lastsenddate { get; set; }
        public Nullable<int> statchangefupcounter { get; set; }
        public int meetingId { get; set; }
        public string reference { get; set; }
        public string sourcetype { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public string collaboratorname { get; set; }
        public string collaboratoremail { get; set; }
        public string collaboratorempid { get; set; }
        public byte pflag { get; set; }
        public byte issuetype { get; set; }
        public string category { get; set; }
        public Nullable<System.DateTime> closedate { get; set; }
        public string referenceId { get; set; }
        public string referencetype { get; set; }
        public string refsource { get; set; }
        public string issuecategory { get; set; }
        public string branchcode { get; set; }
    }

    public class MifsIsssueActionPlan
    {

        public string actionplan { get; set; }
    }

    public class MifsIsssueParams
    {
        public string issuedetails { get; set; }
        public string raisedbyempid { get; set; }
        public string responempid { get; set; }
        public int issueId { get; set; }
    }



    public class MifsIsssueBenchmark
    {
        public string issuestatus { get; set; }
        public string issuedetails { get; set; }
  
    }
}