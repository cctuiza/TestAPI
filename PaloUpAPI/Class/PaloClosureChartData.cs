using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Class
{
    public class PaloClosureChartData
    {
        public List<DailyPendingApprovedRaisedby> DailyPendingApprovedRaisedby { get; set; }
        public List<WeeklyPendingAprovedRaisedBy> WeeklyPendingAprovedRaisedBy { get; set; }
        public List<DailyOpenIssues> DailyOpenIssues { get; set; }
        public List<WeeklyCloseIssue> WeeklyCloseIssue { get; set; }
        public List<WeeklyPendingApprovedByResponsible> WeeklyPendingApprovedByResponsible { get; set; }
        public List<RequestCloseTollgater> RequestClosueTollgater { get; set; }
        public List<RequestCloseCollaborator> RequestCloseCollaborator { get; set; }
    }

    public class DailyPendingApprovedRaisedby
    {
        public string DailyPendingApproveAnalysisDate { get; set; }
        public int DailyCountPendingforAcknowledgementbyRaised { get; set; }
    }

    public class WeeklyPendingAprovedRaisedBy
    {
        public string WeeklyPendingAproveRaiseAnalysisWeek { get; set; }
        public int WeeklyCountPendingforAcknowledgementbyRaised { get; set; }
    }

    public class DailyOpenIssues
    {
        public string DailyOpenIssueAnalysisDate { get; set; }
        public int DailyCountOpenIssue { get; set; }
    }


    public class WeeklyCloseIssue
    {
        public string WeeklyCloseIssueAnalysisWeek { get; set; }
        public int WeeklyCountClose { get; set; }
    }


    public class WeeklyPendingApprovedByResponsible
    {
        public string WeeklyPendingApproveResponsibleAnalysisWeek { get; set; }
        public int CountPendingforAcknowledgementbyResponsible { get; set; }
    }


    public class RequestCloseTollgater
    {
        public string RaisedByEmail { get; set; }
        public int CountPendingforTollgater { get; set; }
        public string TollgaterNickname { get; set; }
    }
    public class RequestCloseCollaborator
    {
        public string CollaboratorEmail { get; set; }
        public int CountPendingInCollaborator { get; set; }
        public string CollaboratorNickname { get; set; }
    }

}