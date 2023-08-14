using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Core.Objects;
using PaloUpAPI.SQLData;
using System.Data.Entity;
using System.Diagnostics;
using PaloUpAPI.Models;

namespace PaloUpAPI.Service
{
    public class MifsServices
    {
        clsprocedure procedure = new clsprocedure();

        public void fixPendingNotification()
        {
            MifsIssues mifsIssues = new MifsIssues();
            MailParamsModel mailParam = new MailParamsModel();
            string emailRecipient = string.Empty;
            string senderName = string.Empty;
            string aliesCode = string.Empty;

            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {

                    db.Configuration.LazyLoadingEnabled = false;

                    //get all iss where lastdate is equal to today and status is open and no pinding for acknowledgement
                    var qDailyIssue = db.mifs_issues
                        .Where(wr => wr.status == "O" && (wr.acknowledgedbysource + wr.acknowledgebyrespondent) == 0 && DbFunctions.TruncateTime(DateTime.Now) > DbFunctions.TruncateTime(wr.lastsenddate))
                        .ToList();

                    //loop all issue ithe qDailyIssue paloup
                    foreach (var row in qDailyIssue)
                    {
                        int followUps = 0; //decalre paloup counter holder
                        string[] param = new string[20];//declare email parameter holder

                        //select current issue from database
                        var currentIssue = db.mifs_issues.Where(wr => wr.issueId == row.issueId).FirstOrDefault();

                        followUps = row.fupcounter + 1; // current follow up counter add 1 
                        senderName = row.raisedby; //set sender name equal to raisedby
                        aliesCode = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault().aliascode;
                        emailRecipient = row.responemail;

                        //set email content parameter
                        param[0] = row.responsible;//responsible
                        param[1] = aliesCode + ":" + currentIssue.issuedetails;
                        param[2] = aliesCode;//alias code
                        param[3] = currentIssue.issueId.ToString();// issue ID
                        param[4] = currentIssue.issuedetails;// issue details
                        param[5] = currentIssue.actionplan;//action plan
                        param[6] = currentIssue.hashcode;//issue hashcode
                        param[7] = emailRecipient;//responsible email
                        param[8] = row.responsible;//responsible name
                        param[9] = row.responempid;//responsible id if employee


                        //set mailparam set value
                        mailParam.recipientsMail = emailRecipient;
                        mailParam.mailTemplatePath = "mailTemplate//mifs.daily.notification.html";
                        mailParam.sendername = senderName;
                        mailParam.subject = "MIFS " + strfollowups(followUps) + " Followup [Ref#:" + currentIssue.issueId.ToString() + "]";
                        mailParam.param = param;
                        mailParam.createdby = "MIFS Sys";
                        procedure.EmailNotification(mailParam);

                        //update current issue lastsendate and fupcounter
                        DateTime dFollowDate = DateTime.Now.AddDays(double.Parse(row.followfrequency.ToString()));

                        //check if the dFollowDate is equal to sunday if equal then add 1 day to make monday
                        if (dFollowDate.ToString("dddd") == "Sunday")
                            dFollowDate.AddDays(1);

                        currentIssue.lastsenddate = dFollowDate;
                        currentIssue.fupcounter = followUps;
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public void postNotificationNotOverDue()
        {
            MifsIssues mifsIssues = new MifsIssues();
            MailParamsModel mailParam = new MailParamsModel();
            string emailRecipient = string.Empty;
            string senderName = string.Empty;
            string aliesCode = string.Empty;

            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {

                    db.Configuration.LazyLoadingEnabled = false;

                    //get all iss where lastdate is equal to today and status is open and no pinding for acknowledgement
                    var qDailyIssue = db.mifs_issues
                        .Where(wr => wr.status == "O" && (wr.acknowledgedbysource + wr.acknowledgebyrespondent) == 0 && DbFunctions.TruncateTime(DateTime.Now) == DbFunctions.TruncateTime(wr.lastsenddate))
                        .ToList();

                    //loop all issue ithe qDailyIssue paloup
                    foreach (var row in qDailyIssue)
                    {
                        int followUps = 0; //decalre paloup counter holder
                        string[] param = new string[20];//declare email parameter holder

                        //select current issue from database
                        var currentIssue = db.mifs_issues.Where(wr => wr.issueId == row.issueId).FirstOrDefault();

                        followUps = row.fupcounter + 1; // current follow up counter add 1 
                        senderName = row.raisedby; //set sender name equal to raisedby
                        aliesCode = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault().aliascode;
                        emailRecipient = row.responemail; 
             
                        //set email content parameter
                        param[0] = row.responsible;//responsible
                        param[1] = aliesCode + ":" + currentIssue.issuedetails;
                        param[2] = aliesCode;//alias code
                        param[3] = currentIssue.issueId.ToString();// issue ID
                        param[4] = currentIssue.issuedetails;// issue details
                        param[5] = currentIssue.actionplan;//action plan
                        param[6] = currentIssue.hashcode;//issue hashcode
                        param[7] = emailRecipient;//responsible email
                        param[8] = row.responsible;//responsible name
                        param[9] = row.responempid;//responsible id if employee
                

                        //set mailparam set value
                        mailParam.recipientsMail = emailRecipient;
                        mailParam.mailTemplatePath = "mailTemplate//mifs.daily.notification.html";
                        mailParam.sendername = senderName;
                        mailParam.subject = "MIFS " + strfollowups(followUps) + " Followup [Ref#:" + currentIssue.issueId.ToString() + "]";
                        mailParam.param = param;
                        mailParam.createdby = "MIFS Sys";
                        procedure.EmailNotification(mailParam);

                        //update current issue lastsendate and fupcounter
                        DateTime dFollowDate = DateTime.Now.AddDays(double.Parse(row.followfrequency.ToString()));
                       
                        //check if the dFollowDate is equal to sunday if equal then add 1 day to make monday
                        if (dFollowDate.ToString("dddd") == "Sunday")
                            dFollowDate.AddDays(1);

                        currentIssue.lastsenddate =dFollowDate;
                        currentIssue.fupcounter = followUps;
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void postNotificationOverDue()
        {
            MifsIssues mifsIssues = new MifsIssues();
            MailParamsModel mailParam = new MailParamsModel();
            string emailRecipient = string.Empty;
            string senderName = string.Empty;
            string aliesCode = string.Empty;


            using (PALOUPEntities db = new PALOUPEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                var qDailyIssue = db.mifs_issues
                    .Where(wr => wr.status == "O" && (wr.acknowledgedbysource + wr.acknowledgebyrespondent) == 0)
                    .Where(w => (DateTime.Now > DbFunctions.TruncateTime(w.targetcompletiondate)))
                    .ToList();


               // return qDailyIssue;
                foreach (var row in qDailyIssue)
                {
                    int followUps = 0;
                    int idate = Convert.ToInt32((DateTime.Now - row.lastsenddate.Value).TotalDays);

                    bool isToday = idate == row.followfrequency;
                     followUps = row.fupcounter + 1;
                    //send notification
                    var currentIssue = db.mifs_issues.Where(wr => wr.issueId == row.issueId).FirstOrDefault();

                    if (idate > row.followfrequency)
                    {
                        currentIssue.lastsenddate = DateTime.Now;
                        db.SaveChanges();
                        this.postNotificationOverDue();
                    }


                    senderName = row.raisedby;
                    aliesCode = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault().aliascode;

                    emailRecipient = row.responemail;

                    if (isToday)
                    {
                        //  sendNotification(emailRecipient, senderName, row.responsible, sourceType + ":" + currentIssue.issuedetails, sourceType, currentIssue.issueId.ToString(), currentIssue.issuedetails, currentIssue.actionplan, followUps);

                        string[] param = new string[20];


                        param[0] = row.responsible;
                        param[1] = aliesCode + ":" + currentIssue.issuedetails;
                        param[2] = aliesCode;
                        param[3] = currentIssue.issueId.ToString();
                        param[4] = currentIssue.issuedetails;
                        param[5] = currentIssue.actionplan;
                        param[6] = currentIssue.hashcode;//issue hashcode
                        param[7] = emailRecipient;//responsible email
                        param[8] = row.responsible;//responsible name
                        param[9] = row.responempid;//responsible id if employee
                        mailParam.recipientsMail = emailRecipient;
                        mailParam.mailTemplatePath = "mailTemplate//mifs.over.due.notification.html";
                        mailParam.sendername = senderName;
                        mailParam.subject = "MIFS (" + strfollowups(followUps) + ") Followup";
                        mailParam.param = param;
                        mailParam.createdby = "MIFS Sys";
                        procedure.EmailNotification(mailParam);

                    }

                    currentIssue.fupcounter = followUps;

                }

                db.SaveChanges(); //update all counter add 1
            }

        }
        public void postNotificationStatusChange()
        {
            MifsIssues mifsIssues = new MifsIssues();
            MailParamsModel mailParam = new MailParamsModel();
            MifsIssues issue = new MifsIssues();
            string recipientMail = string.Empty;
            string recipientName = string.Empty;
            string recipientId = string.Empty;
            string requestorName = string.Empty;
            string senderName = string.Empty;
            string aliesCode = string.Empty;

            try
            {


                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var qDailyIssue = db.mifs_issues
                        .Where(wr => (wr.status == "O" || wr.status == "P") && (wr.acknowledgedbysource + wr.acknowledgebyrespondent) != 0 )
                        .ToList();

                    int watchcount = 0;
                    foreach (var row in qDailyIssue)
                    {
                        int followUps = 0;
                        int? statchangefupcounter = 0;
                        string[] param = new string[20];

                        //send notification
                        var currentIssue = db.mifs_issues.Where(wr => wr.issueId == row.issueId).FirstOrDefault();

                        watchcount = watchcount + 1;

                        statchangefupcounter = row.statchangefupcounter + 1;
                        followUps = row.fupcounter + 1;
                        senderName = row.raisedby;

                        if (string.IsNullOrEmpty(statchangefupcounter.ToString()))
                            statchangefupcounter = 0;

                        var topic = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault();

                        //for raised
                        if (currentIssue.acknowledgedbysource == 0 && currentIssue.acknowledgebyrespondent == 1)
                        {
                            recipientMail = row.raisedbyemail;
                            recipientName = row.raisedby;
                            requestorName = row.responsible;
                            recipientId = row.raisedbyempid;
                        }

                        //for responsible
                        if (currentIssue.acknowledgedbysource == 1 && currentIssue.acknowledgebyrespondent == 0)
                        {
                            recipientMail = row.responemail;
                            recipientName = row.responsible;
                            requestorName = row.raisedby;
                            recipientId = row.responempid;
                        }

                        param[0] = recipientName;
                        param[1] = requestorName;
                        param[2] = issue.MifsStatus(currentIssue.status);
                        param[3] = issue.MifsStatus(currentIssue.newstatus);
                        param[4] = topic.aliascode;
                        param[5] = currentIssue.issueId.ToString();
                        param[6] = currentIssue.issuedetails;
                        param[7] = currentIssue.actionplan;
                        param[8] = currentIssue.issuedate.ToShortDateString();
                        param[9] = currentIssue.targetcompletiondate.ToShortDateString();
                        param[10] = currentIssue.hashcode;
                        param[11] = topic.hashcode;
                        param[12] = currentIssue.resumedate.ToString();
                        param[13] = string.Empty;
                        param[14] = recipientMail;
                        param[15] = recipientId;


                        if (currentIssue.newstatus == "C" || currentIssue.newstatus == "O")
                        {
                            mailParam.mailTemplatePath = "mailTemplate//mifs.request.chance.status.html";
                        }
                        else if (currentIssue.newstatus == "P")
                        {
                            mailParam.mailTemplatePath = "mailTemplate//mifs.request.chance.status.park.html";
                        }
                        else if (currentIssue.newstatus == "D")
                        {
                            mailParam.mailTemplatePath = "mailTemplate//mifs.request.chance.status.drop.html";
                        }

                    

                        mailParam.recipientsMail = recipientMail;
                        mailParam.sendername = "Paloup System";
                        mailParam.subject = "PaloUp Request for Change Status (" + strfollowups(int.Parse(statchangefupcounter.ToString())) + ") [Ref#:"+ currentIssue.issueId.ToString() + "]";
                        mailParam.param = param;
                        mailParam.createdby = "PaloUp";
                        procedure.EmailNotification(mailParam);

                        DateTime dFollowDate =DateTime.Parse(currentIssue.lastsenddate.ToString());
                        dFollowDate = dFollowDate.AddDays(1);

                        //check if the dFollowDate is equal to sunday if equal then add 1 day to make monday
                        if (dFollowDate.ToString("dddd") == "Sunday")
                            dFollowDate.AddDays(1);

                        currentIssue.lastsenddate = dFollowDate;
                        currentIssue.statchangefupcounter = statchangefupcounter;
                        currentIssue.fupcounter = followUps;
                        db.SaveChanges();
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void putIssueToResume()
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var qparkIssue = db.mifs_issues.Where(m => m.status == "P" && DbFunctions.TruncateTime(m.resumedate) <= DateTime.Now).ToList();
                    foreach (var row in qparkIssue)
                    {
                        var currentIssue = db.mifs_issues.Where(wr => wr.issueId == row.issueId).FirstOrDefault();

                        currentIssue.status = "O";
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string strfollowups(int p)
        {
            if (p == 1)
            {
                return p.ToString() + "st";
            }
            else if (p == 2)
            {
                return p.ToString() + "nd";
            }
            else if (p == 3)
            {
                return p.ToString() + "rd";
            }
            else
            {
                return p.ToString() + "th";
            }
        }

        public void CounterLimitCheck()
        {
            string[] param = new string[20];
            MifsIssues issue = new MifsIssues();
            MailParamsModel mailParams = new MailParamsModel();
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    List<string> strStatus = new List<string>();
                    strStatus.Add("O");
                    strStatus.Add("P");
                    var qIssueview = db.mifs_vIssues.Where(m => strStatus.Contains(m.IssueStatus) && m.newstatus != string.Empty && m.statchangecounterlimit > 0 && m.statchangecounterlimit <= m.statchangefupcounter).ToList();

                    foreach (var row in qIssueview)
                    {
                        var qIssue = db.mifs_issues.Where(m => m.issueId == row.issueId).FirstOrDefault();
                        string respondentname = string.Empty, responsebyemail = string.Empty, receipent = string.Empty;

                        if (qIssue.acknowledgebyrespondent == 1)
                        {
                            respondentname = qIssue.raisedby;
                            responsebyemail = qIssue.responemail;
                            receipent = qIssue.responsible;
                        }
                        else
                        {
                            respondentname = qIssue.responsible;
                            responsebyemail = qIssue.raisedbyemail;
                            receipent = qIssue.raisedby;
                        }

                        param[0] = receipent;
                        param[1] = row.issueId.ToString() + "-" + row.Details;
                        param[2] = issue.MifsStatus(qIssue.status);
                        param[3] = issue.MifsStatus(qIssue.newstatus);
                        param[4] = respondentname;
                        param[5] = row.statchangefupcounter.ToString();
                        param[6] = row.IssueHashcode;
                        //set mail Parameter value
                        using (PALOUPEntities db2 = new PALOUPEntities())
                        {
                            string strMsg = "The issue has automatically changed the status from " + issue.MifsStatus(qIssue.status) + " to " + issue.MifsStatus(qIssue.newstatus) + " because the respondent " + respondentname + " didn’t take action on notifications sent after " + row.statchangefupcounter.ToString() + " followups";
                            db2.Sproc_mifsSetResponse(0, row.issueId, strMsg, "PaloUp System", string.Empty, DateTime.Now, responsebyemail);
                        }
                        qIssue.status = qIssue.newstatus;
                        qIssue.newstatus = string.Empty;
                        qIssue.statchangefupcounter = 0;
                        db.SaveChanges();

                        mailParams.mailTemplatePath = "mailTemplate//mifs.change.status.counterlimit.html";
                        mailParams.recipientsMail = responsebyemail;
                        mailParams.sendername = "Paloup System";
                        mailParams.subject = "PaloUp counter limit exceed!";
                        mailParams.param = param;
                        mailParams.createdby = "Paloup";
                        procedure.EmailNotification(mailParams);
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}