using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.Models;
using System.Diagnostics;
using PaloUpAPI.ActionFilters;
namespace PaloUpAPI.Controllers
{
    [AuthorizationRequired]
    [Compress]
    public class MifsCalendarController : ApiController
    {
        PALOUPEntities db = new PALOUPEntities();

        public class Calendar
        {
            public int id { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string color { get; set; } // color of bg
            public string borderColor { get; set; } // border color
            public string textColor { get; set; } // font color
            public string className { get; set; } // class if want
            public string head { get; set; } // tooltip
            public string typeInfo { get; set; } //M for Meeting and I for Issue

        }

        public class VM
        {
            public List<Calendar> Calendar { get; set; }
            public List<mifs_vIssues> VIssues { get; set; }
            public List<mifs_vMeetingSchedule> VMeeting { get; set; }

        }

        public HttpResponseMessage GetCalendar(string empid, int month, int year)
        {
            try
            {

                string yellow = "#fff70f";
                string red = "#f22222";
                string green = "#00ff7f";
                string blue = "#007fff";
                string orange = "#f49e42";
                string transparent = "transparent";
                string black = "#000000";

                VM model = new VM();
                List<Calendar> calList = new List<Calendar>();
                if (empid == null || empid == "")
                {
                    return null;
                }
                else
                {
                    DateTime startDate = new DateTime(year, month, 1, 00, 00, 00);
                    DateTime endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
                    var issueList = db.mifs_vIssues
                        .Where(m =>
                            (m.responempid == empid || m.collaboratorempid == empid || m.raisedbyempid == empid) &&
                            ((m.lastsenddate >= startDate && m.lastsenddate <= endDate && (m.IssueStatus == "O" || m.IssueStatus == "D")) ||
                            (m.IssueDateParked >= startDate && m.IssueDateParked <= endDate && m.IssueStatus == "P") ||
                            (m.DateClosed >= startDate && m.DateClosed <= endDate && m.IssueStatus == "C"))
                            ).ToList();
                    var meetingList = db.mifs_vMeetingSchedule
                        .Where(m =>
                        m.memberId == empid && m.datestart.Month == month && m.datestart.Year == year
                        ).ToList();
                    foreach (var item in meetingList)
                    {
                        Calendar temp = new Calendar();
                        temp.id = item.meetingId;
                        temp.start = item.datestart.ToString("MM/dd/yyyy");
                        temp.title = "<span class='fa fa-comments'></span> <span>" + item.agenda + "</span>";
                        temp.head = "Meeting in " + item.description;
                        temp.textColor = black;
                        temp.color = transparent;
                        temp.typeInfo = "M";
                        calList.Add(temp);
                    }
                    foreach (var item in issueList)
                    {
                        if (item.issueId == 249)
                        {
                            var q = true;
                        }
                        Calendar temp = new Calendar();
                        if (item.IssueStatus == "C" || item.IssueStatus == "D")
                        {
                            if (item.DateClosed != null)
                            {
                                temp.color = transparent;
                                temp.id = item.issueId;
                                temp.start = item.DateClosed.Value.ToString("MM/dd/yyyy");
                                temp.title = "<span style='color:" + green + "'>  " + findClassification(item, empid) + "<span class='fa fa-circle'></span> </span> <span style='color:" + item.bgcolor + "'>" + item.Details + "</span>";
                                temp.head = item.CurrentTopicDescription;
                                temp.className = "moreBorder";
                                temp.typeInfo = "I";
                                calList.Add(temp);
                            }
                        }
                        else if (item.IssueStatus == "P")
                        {
                            if (item.IssueDateParked != null)
                            {
                                temp.color = transparent;
                                temp.id = item.issueId;
                                temp.start = item.IssueDateParked.Value.ToString("MM/dd/yyyy");
                                temp.title = "<span style='color:" + blue + "'>" + findClassification(item, empid) + "<span class='fa fa-circle'></span>  </span>  <span style='color:" + item.bgcolor + "'>" + item.Details + "</span>";
                                temp.head = item.CurrentTopicDescription;
                                temp.className = "moreBorder";
                                temp.typeInfo = "I";
                                calList.Add(temp);
                            }
                        }
                        else if (item.IssueStatus == "O")
                        {
                            if (item.lastsenddate != null)
                            {
                                temp.color = transparent;
                                temp.id = item.issueId;
                                temp.start = item.lastsenddate.Value.ToString("MM/dd/yyyy");
                                var clr = item.lastsenddate >= item.targetcompletiondate ? red : yellow;
                                temp.title = "<span style='color:" + clr + "'>" + findClassification(item, empid) + "<span class='fa fa-circle'></span> </span>  <span style='color:" + item.bgcolor + "'>" + item.Details + "</span>";
                                temp.head = item.CurrentTopicDescription;
                                temp.className = "moreBorder";
                                temp.typeInfo = "I";
                                calList.Add(temp);
                            }
                        }


                    }
                    model.Calendar = calList;
                    model.VIssues = issueList;
                    model.VMeeting = meetingList;

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public string findClassification(mifs_vIssues obj, string emppId)
        {
            if (obj.responempid == emppId)
            {
                return "R";
            }
            else if (obj.collaboratorempid == emppId)
            {
                return "C";
            }
            else if (obj.raisedbyempid == emppId)
            {
                return "T";
            }
            else
            {
                return null;
            }


        }
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
