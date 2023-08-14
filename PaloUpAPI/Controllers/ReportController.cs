using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using PaloUpAPI.Models;
using PaloUpAPI.ActionFilters;

namespace PaloUpAPI.Controllers
{
    [Compress]
    public class ReportController : Controller
    {
        //
        // GET: /Default1/

        clsprocedure procedure = new clsprocedure();

        PALOUPEntities db = new PALOUPEntities();

        public ActionResult Index()
        {
            return View();
        }

        #region "MIFS Report"

        public ActionResult Minutes(int id)
        {
           
            var qIssueList = db.mifs_vIssues.Where(wr => wr.meetingId == id).ToList();

            var qMeeting = db.mifs_meeting.Where(wr => wr.meetingId == id).First();
            ViewBag.note = qMeeting.notes;
            ViewBag.meetingId = qMeeting.meetingId;
            ViewBag.CurrentTopicDescription = qMeeting.mifs_topic.description;
            ViewBag.venue = qMeeting.venue;
            ViewBag.facilitator = qMeeting.facilitator;
            ViewBag.agenda = qMeeting.agenda;


            return View(qIssueList);
        }


        public ActionResult MifsReponseMessage(int issueID,string response,string name ,string email,string empId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            var mifsIssue = db.mifs_issues.Where(m => m.issueId == issueID).FirstOrDefault();
           var topic = db.mifs_topic.Where(wr => wr.topicId == mifsIssue.topicId).FirstOrDefault().aliascode;
            var mifsResponse = db.mifs_response.Where(m => m.issueId == issueID).OrderBy(m => m.responseId).ToList();

            ViewBag.raisedbyemail = mifsIssue.raisedbyemail;
            ViewBag.topic = topic;
            ViewBag.response = response;
            ViewBag.issuedetails = mifsIssue.issuedetails;
            ViewBag.issueId = issueID.ToString();
            ViewBag.actionplan = mifsIssue.actionplan;
            ViewBag.issuedate = mifsIssue.issuedate.ToShortDateString();
            ViewBag.hashcode = mifsIssue.hashcode;
            ViewBag.name = name;
            ViewBag.email = email;
            ViewBag.empId = empId;

            return View(mifsResponse);
        }
        #endregion

        

        

        #region "System Message"

        public ActionResult ErrorReport()
        {
            ViewBag.errorMessage = ViewBag.errorMessage;
            ViewBag.errorTrace = ViewBag.errorTrace;
            return View();

        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}