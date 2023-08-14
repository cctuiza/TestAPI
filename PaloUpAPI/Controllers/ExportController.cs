using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using System.Data;
using PaloUpAPI.Models;
using PaloUpAPI.Core.BusinessServices;
using PaloUpAPI.SQLData;
using PaloUpAPI.Controllers;
using PaloUpAPI.Class;
using System.Net.Http;
using System.Net;

namespace FscCore.Controllers
{
    public class ExportController : Controller
    {
        PALOUPEntities db = new PALOUPEntities();
        clsprocedure procedure = new clsprocedure();
     
        // GET: Export
        public ActionResult PaloUpExport(string tokenId, string useremail, string statusCode)
        {
            
            var provider = new TokenServices(db);
            bool hasError = true;

            if (!string.IsNullOrEmpty(tokenId))
            {
                //validate
                hasError = !provider.ValidateToken(tokenId);
            }


            //check return
            if (!hasError)
            {
                string filename = DateTime.Now.ToString("MMddyy-hhmmsss-") + statusCode;
                db.Configuration.LazyLoadingEnabled = false;

                List<int> topicIdLst = new List<int>();
                MifsReport n = new MifsReport();
              //  var qTopic = db.mifs_topicmembers.Where(wr => wr.ema == userId).ToList();
                var qTopic=  (from aa in db.mifs_topicmembers
                              join bb in db.mifs_members on aa.memberId equals bb.memberId
                 where bb.emailaddress == useremail
                 select new {aa.topicId });
                //get user topic
                foreach (var topic in qTopic)
                    topicIdLst.Add(topic.topicId);


                var meetingTypes = db.mifs_vIssueReport
                        .Where(wr => wr.IssueStatus == statusCode && topicIdLst.Contains(wr.CurrentTopicId))
                       .OrderBy(od => od.DateIssued)
                       .ToList()
                       .Select(aa => new
                       {
                          
                           aa.issueId,
                           aa.pflag,
                           aa.DateIssued,
                           aa.Details,
                           aa.TopicAlias,
                           aa.actionplan,
                           aa.targetcompletiondate,
                           aa.raisedby,
                           aa.responsible,
                           update = n.setNewLine(aa.Updates),
                           aa.FollowUpCounter
                       }).ToList();


                DataTable dt = new DataTable();
                dt = procedure.ToDataSet(meetingTypes);

                var grid = new GridView();
                grid.DataSource = dt;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename="+ filename + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                return View("MyView");
            }
            else
            {
                return Content("Invalid Request");
            }

        }



      
        public ActionResult PaloUpExport2(List<TopicList> topics)
        {


            string filename = DateTime.Now.ToString("MMddyy-hhmmsss-");
                db.Configuration.LazyLoadingEnabled = false;

                //List<int> topicIdLst = new List<int>();
                MifsReport n = new MifsReport();
                ////  var qTopic = db.mifs_topicmembers.Where(wr => wr.ema == userId).ToList();
                //var qTopic = (from aa in db.mifs_topicmembers
                //              join bb in db.mifs_members on aa.memberId equals bb.memberId
                //              where bb.emailaddress == useremail
                //              select new { aa.topicId });


                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                DataTable dh = new DataTable();

                dt.Columns.Add("CategoryName");
                dt.Columns.Add("GoalDesc");
                dt.Columns.Add("ActionDesc");
                dt.Columns.Add("GoalId");
                dt.Columns.Add("ActionStatus");
                dt.Columns.Add("GoalStatus");


                //get user topic
                //foreach (var topic in topics)
                //{

                //        DataRow row = dt.NewRow();
                //var issueList = db.mifs_vIssueReport
                //    .Where(mm => mm.IssueStatus == topic.status && mm.CurrentTopicId == topic.topicId)
                //   .OrderBy(mm => mm.DateIssued)
                //   .ToList()
                //   .Select(aa => new
                //   {

                //       aa.issueId,
                //       aa.pflag,
                //       aa.DateIssued,
                //       aa.Details,
                //       aa.TopicAlias,
                //       aa.actionplan,
                //       aa.targetcompletiondate,
                //       aa.raisedby,
                //       aa.responsible,
                //       update = n.setNewLine(aa.Updates),
                //       aa.FollowUpCounter
                //   }).ToList();
                //     foreach (var issue in issueList)
                //      {
                //        row["CategoryName"] = issue.issueId;
                //        row["GoalDesc"] = issue.Details;
                //        row["ActionDesc"] = issue.actionplan;
                //        row["GoalId"] = issue.update;
                  
                //        dt.Rows.Add(row);
                //      }
                       
                 








                //}
                //topicIdLst.Add(topic.topicId);


                //var meetingTypes = db.mifs_vIssueReport
                //        .Where(wr => wr.IssueStatus == statusCode && topicIdLst.Contains(wr.CurrentTopicId))
                //       .OrderBy(od => od.DateIssued)
                //       .ToList()
                //       .Select(aa => new
                //       {

                //           aa.issueId,
                //           aa.pflag,
                //           aa.DateIssued,
                //           aa.Details,
                //           aa.TopicAlias,
                //           aa.actionplan,
                //           aa.targetcompletiondate,
                //           aa.raisedby,
                //           aa.responsible,
                //           update = n.setNewLine(aa.Updates),
                //           aa.FollowUpCounter
                //       }).ToList();


                //DataTable dt = new DataTable();
                //dt = procedure.ToDataSet(meetingTypes);

                var grid = new GridView();
                grid.DataSource = dt;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + filename + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();


            return View("MyView");


        }


        public ActionResult exportFinishMeetingToExcel(string tokenId, int topicId)
        {

            
            var provider = new TokenServices(db);
            bool hasError = true;

            if (!string.IsNullOrEmpty(tokenId))
            {
                //validate
                hasError = !provider.ValidateToken(tokenId);
            }


            //check return
            if (!hasError)
            {
              
                db.Configuration.LazyLoadingEnabled = false;

                List<int> topicIdLst = new List<int>();
                MifsController n = new MifsController();

                var qTopic = db.mifs_topic.Where(wr => wr.topicId == topicId).FirstOrDefault();
                string filename = "Finish Meeting " + qTopic.description + DateTime.Now.ToString(" MMddyy-hhmmsss");

                var qMeeting = db.mifs_meeting.Where(wr => wr.topicId == topicId)
                 .Where(wr => wr.status == "C")
                 .OrderBy(od => od.meetingId)
                 .ToList()
                  .Select(aa => new
                  {
                      aa.meetingId,
                      aa.topicId,
                      aa.status,
                      aa.datestart,
                      aa.dateend,
                      aa.venue,
                      aa.facilitator,
                      aa.agenda,
                      topicDesc = qTopic.description,
                      openIssue = n.countOpenMeetingIssue(int.Parse(aa.meetingId.ToString())),
                      parkIssue = n.countParkMeetingIssue(int.Parse(aa.meetingId.ToString())),
                      closeIssue = n.countCloseMeetingIssue(int.Parse(aa.meetingId.ToString()))
                  }).ToList();


                DataTable dt = new DataTable();
                dt = procedure.ToDataSet(qMeeting);

                var grid = new GridView();
                grid.DataSource = dt;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + filename + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                return View("MyView");
            }
            else
            {
                return Content("Invalid Request");
            }

        }
        public ActionResult ExportCalendar(string tokenId, string useremail, int imonth, int iyear)
        {

            var provider = new TokenServices(db);
            bool hasError = true;

            if (!string.IsNullOrEmpty(tokenId))
            {
                //validate
                hasError = !provider.ValidateToken(tokenId);
            }


            //check return
            if (!hasError)
            {
                string filename = "Calendar" + DateTime.Now.ToString("MMddyy-hhmmsss-") + imonth.ToString() + iyear.ToString() ;
                db.Configuration.LazyLoadingEnabled = false;

                List<int> topicIdLst = new List<int>();
                MifsReport n = new MifsReport();
                var qTopic = (from aa in db.mifs_topicmembers
                              join bb in db.mifs_members on aa.memberId equals bb.memberId
                              where bb.emailaddress == useremail
                              select new { aa.topicId });

                //get user topic
                foreach (var topic in qTopic)
                    topicIdLst.Add(topic.topicId);

                DateTime startDate = new DateTime(iyear, imonth, 1, 00, 00, 00);
                DateTime endDate = new DateTime(iyear, imonth, DateTime.DaysInMonth(iyear, imonth), 23, 59, 59);
                var issueList = db.mifs_vIssueReport
                    .Where(m =>
                        (m.responemail == useremail || m.collaboratoremail == useremail || m.raisedbyemail == useremail) &&
                        ((m.lastsenddate >= startDate && m.lastsenddate <= endDate && (m.IssueStatus == "O" || m.IssueStatus == "D")) ||
                        (m.IssueDateParked >= startDate && m.IssueDateParked <= endDate && m.IssueStatus == "P") ||
                        (m.DateClosed >= startDate && m.DateClosed <= endDate && m.IssueStatus == "C"))
                        ).ToList();

                //var meetingList = db.mifs_vMeetingSchedule
                //    .Where(m =>
                //    m.memberId == userId && m.datestart.Month == imonth && m.datestart.Year == iyear
                //    ).ToList();


                DataTable dt = new DataTable();
                dt = procedure.ToDataSet(issueList);

                var grid = new GridView();
                grid.DataSource = dt;
                grid.DataBind();

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + filename + ".xls");
                Response.ContentType = "application/ms-excel";

                Response.Charset = "";
                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                grid.RenderControl(htw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

                return View("MyView");
            }
            else
            {
                return Content("Invalid Request");
            }

        }
    }
}