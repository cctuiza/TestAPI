using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace PaloUpAPI.Models
{
    public class IssueMeetingModel
    {
        public int  issueId { get; set; }
        public string hashcode { get; set; }
        public int topicId { get; set; }
        public string issuedetails { get; set; }
        public string actionplan { get; set; }
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
        public Nullable<System.DateTime> lastsenddate { get; set; }
        public Nullable<int> statchangefupcounter { get; set; }
        public int meetingId { get; set; }
        public string reference { get; set; }
        public string sourcetype { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public string hashcodeMeetingId { get; set; }
        public string hashcodeIssueId { get; set; }
        public string collaboratorname { get; set; }
        public string collaboratoremail { get; set; }
        public Byte pflag { get; set; }
        public string category { get; set; }
        public Byte issuetype { get; set; }
        public string issuecategory { get; set; }
        public string branchcode { get; set; }
        public string districtcode { get; set; }
        public string quadrant { get; set; }
        public string subkpi { get; set; }
        public Nullable<int> reviewlevel { get; set; }
        public Nullable<int> fromissueid { get; set; }

        public string changebyempname { get; set; } // used in edit
        public string changebyempId { get; set; } // used in edit
        public string changebyempemailadd { get; set; } // used in edit
    }


    public class IssueMeetingModel2
    {
        public int issueId { get; set; }
        public string hashcode { get; set; }
        public int topicId { get; set; }
        public string issuedetails { get; set; }
        public string actionplan { get; set; }
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
        public Nullable<System.DateTime> lastsenddate { get; set; }
        public Nullable<int> statchangefupcounter { get; set; }
        public int meetingId { get; set; }
        public string reference { get; set; }
        public string sourcetype { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public string hashcodeMeetingId { get; set; }
        public string hashcodeIssueId { get; set; }
        public string collaboratorname { get; set; }
        public string collaboratoremail { get; set; }
        public Byte pflag { get; set; }
        public Byte issuetype { get; set; }
        public string issuecategory { get; set; }
        public string branchcode { get; set; }
        public string districtcode { get; set; }
        public string quadrant { get; set; }
        public Nullable<int> fromissueid { get; set; }


    }


    public class IssueCompareModel
    {

        [Column("Issue Details")]
        public string issuedetails { get; set; }
        [DisplayName("Action Plan")]
        public string actionplan { get; set; }
        [DisplayName("ACtion Plan")]
        public System.DateTime issuedate { get; set; }

        [DisplayName("Target Completion Date")]
        public System.DateTime targetcompletiondate { get; set; }

        [DisplayName("Tollgater/Raised By")]
        public string raisedby { get; set; }
  
        [Column("Responsible")]
        public string responsible { get; set; }
   
        [Column("Collaborator Name")]
        public string collaboratorname { get; set; }


        [Column("Issue Type")]
        public Byte issuetype { get; set; }
        [Column("Issue Category")]
        public string issuecategory { get; set; }
     
        [Column("KPI Tagging")]
        public string quadrant { get; set; }

    }





    public class BulkIssueForDrop
    {
        public List<IssueMeetingModel> issuemodel{ get; set; }
        public int ticketno { get; set; }
        public string requestorname { get; set; }
        public string requestoremail { get; set; }
        public string approvername { get; set; }
        public string approveremail { get; set; }
        public string dropbyemployeename { get; set; }
        public string dropbyemployeeId { get; set; }
        public string dropbyemployeeemail { get; set; }
    }


    public class BulkIssueForTransfer
    {
        public List<IssueMeetingModel> issuemodel { get; set; }
        public int ticketno { get; set; }
        public string requestorname { get; set; }
        public string requestoremail { get; set; }
        public string approvername { get; set; }
        public string approveremail { get; set; }
        public string transferbyemployeename { get; set; }

        public int transferbytopicId { get; set; }

        public string transferbyemployeeId { get; set; }
        public string transferbyemployeeemail { get; set; }
        public string transferraisedbyempId { get; set; }
        public string transferresponsibleempId { get; set; }

    }


    public class BulkIssue
    {
        public int issueId { get; set; }
      
    }
}