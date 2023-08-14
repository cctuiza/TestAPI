using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;
using PaloUpAPI.Models.MIFS;
using System.Data.Entity;
using System.Web.Http;
using HtmlAgilityPack;
using System.Collections.Specialized;
using System.Configuration;

namespace PaloUpAPI.SQLData
{
    public class MifsIssues
    {
        Library phpLib = new Library();
        clsprocedure procedure = new clsprocedure();
        MailParamsModel mailParams = new MailParamsModel();

        public string MifsStatus(string p)
        {
            switch (p)
            {
                case "O":
                    return "Open";
                    break;
                case "C":
                    return "Close";
                    break;
                case "P":
                    return "Park";
                    break;
                case "D":
                    return "Drop";
                    break;
                default:
                    return "";
            }
        }

        public void putIssueStatus(mifs_issues p)
        {
            try
            {
                putIssueAcknowledged(p);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       

        public void putIssueAcknowledged(mifs_issues p)
        {

            string sourceName, respondentName, issueDtl,sourceEmail,respondentEmail,sourcehascode,respondenthascode,sourceMobile,respondentMobile;
            try
            {

                MifsResponse n = new MifsResponse();
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var issues = db.mifs_issues.Where(wr => wr.issueId == p.issueId).FirstOrDefault();
                    int sourceId=issues.topicId;
                    var qSourceType = db.mifs_topic.Where(wr => wr.topicId == sourceId).FirstOrDefault();
                    //var sourceMember = getMemberName(issues.escalatedby);
                    //var respondentMember = getMemberName(issues.respondent);

                    sourceName = issues.raisedby;
                    sourcehascode = phpLib.generatePassword(issues.raisedbyempid).ToString();
                    sourceEmail = issues.raisedbyemail;
                    sourceMobile = issues.raisedbyphone;
     
                    respondentName = issues.responsible;
                    respondentEmail =issues.responemail;
                    respondenthascode = phpLib.generatePassword(issues.responempid).ToString();
                    respondentMobile = issues.responphone;
                    issueDtl = issues.issuedetails;
                    
                    //if the acknowledgedbysource is not equal thier are changes in the status
                    if (issues.acknowledgedbysource != p.acknowledgedbysource)
                    {
                        issues.acknowledgedbysource = p.acknowledgedbysource;
                        issues.acknowledgesourcedate = DateTime.Now;
                        issues.parkdate = p.parkdate;
                        issues.resumedate = p.resumedate;
                    }

                    //if the acknowledgebyrespondent is not equal thier are changes in the status
                    if (issues.acknowledgebyrespondent != p.acknowledgebyrespondent)
                    {
                        issues.acknowledgebyrespondent = p.acknowledgebyrespondent;
                        issues.acknowledgerespondentdate = DateTime.Now;
                        issues.parkdate = p.parkdate;
                        issues.resumedate = p.resumedate;


                    }

                    //if the acknowledgedbysource + acknowledgebyrespondent equal to 2 they acknowledge both side parties
                    //set the status to  new status
                    if (issues.acknowledgedbysource + issues.acknowledgebyrespondent == 2)
                    {
                        issues.status = issues.newstatus;
                        SmsNotification(respondentMobile, "Issue #." + p.issueId.ToString() + " " + p.issuedetails +" have change status from " + p.status + " to " + issues.newstatus , "MIFS");
                        SmsNotification(sourceMobile, "Issue #." + p.issueId.ToString() + " " + p.issuedetails + " have change status from " + p.status + " to " + issues.newstatus, "MIFS");

                    }
                    else
                    {
                        issues.newstatus = p.newstatus;
                    }

                    db.SaveChanges();
                }
            }
            catch(Exception ex){
                throw ex;
            }
        }

        public void putChangeStatus(MifsChangeStatusModel p)
        {
            string sourceName, respondentName, issueDtl, sourceEmail, respondentEmail, sourcehascode, respondenthascode, sourceMobile, respondentMobile;
            string[] param = new string[20];
            string strResponse;

            MifsIssues n = new MifsIssues();

            try
            {

                //MifsResponse n = new MifsResponse();
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var issues = db.mifs_issues.Where(wr => wr.issueId == p.issueId).FirstOrDefault();
                    int sourceId = issues.topicId;
                    var qSourceType = db.mifs_topic.Where(wr => wr.topicId == sourceId).FirstOrDefault();
                    //var sourceMember = getMemberName(issues.escalatedby);
                    //var respondentMember = getMemberName(issues.respondent);

                    sourceName = issues.raisedby;
                    sourcehascode = phpLib.generatePassword(issues.raisedbyempid).ToString();
                    sourceEmail = issues.raisedbyemail;
                    sourceMobile = issues.raisedbyphone;

                    respondentName = p.responseName;
                    respondentEmail = p.responseEmail;
                    respondenthascode = phpLib.generatePassword(p.responseId).ToString();
                    respondentMobile = "";
                    issueDtl = issues.issuedetails;


                    // set park to open
                    if(issues.status=="P" && p.newstatus == "O")
                    {
                        var iresult = db.Sproc_mifsSetResponse(0, p.issueId, sourceName + " change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason
    , sourceName, p.responseId, DateTime.Now, sourceEmail).First().Value;

                        p.newstatus = string.Empty;
                        issues.status = "O";
                        issues.acknowledgedbysource = 0;
                        issues.acknowledgebyrespondent = 0;
                        //effect changes
                        db.SaveChanges();

                        var qCurrentResponse = db.mifs_vResponse.Where(wr => wr.issueId == p.issueId).ToList();

                        return;

                    }

                    //if the acknowledgedbysource is not equal thier are changes in the status
                    if (issues.acknowledgedbysource != p.acknowledgedbysource)
                    {
                      
                        //if the issue set to close by tollgater then set status to close
                        if (p.newstatus == "C" || p.newstatus=="D")
                        {
                            

                            var iresult = db.Sproc_mifsSetResponse(0, p.issueId, sourceName + " change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason
      , sourceName, p.responseId, DateTime.Now, sourceEmail).First().Value;


                            issues.status = p.newstatus;//update status value

                            //Begin: sent notification
                            param[0] = issues.responsible;
                            param[1] = issues.raisedby;
                            param[2] = n.MifsStatus(issues.status);
                            param[3] = n.MifsStatus(p.newstatus);
                            param[4] = issues.issueId.ToString();
                            param[5] = issues.issuedetails;
                            param[6] = issues.actionplan;
                            param[7] = p.reason;
                            mailParams.recipientsMail = issues.responemail;
                            // mailParams.ccMail = currentIssue.raisedbyrepresentativemail + ';' + currentIssue.responrepresentativemail;
                            mailParams.mailTemplatePath = "mailTemplate//mifs.tollgater.close.issue.notification.html";
                            mailParams.sendername = issues.raisedby;
                            mailParams.subject = "[paloUp] Issue Closure";
                            mailParams.param = param;
                            mailParams.createdby = "paloUp";
                            procedure.EmailNotification(mailParams);


                        }
                        else
                        {
                            issues.acknowledgedbysource = p.acknowledgedbysource;
                            issues.acknowledgesourcedate = DateTime.Now;
                           
                            issues.newstatus = p.newstatus;
                            issues.resumedate = p.resumedate;

                            if (p.newstatus == "P")
                            {
                                issues.parkdate = DateTime.Now;
                                strResponse = respondentName + " requested change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason + " and it will resume on " + DateTime.Parse( p.resumedate.ToString()).ToShortDateString();
                            }
                            else
                            {
                                strResponse = respondentName + " requested change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason;
                            }

                            var iresult = db.Sproc_mifsSetResponse(0, p.issueId, strResponse
                           , respondentName, p.responseId, DateTime.Now, respondentEmail).First().Value;

                            EmailNotification(sourceName, sourceEmail, respondentName, p.issueId, qSourceType.aliascode, issues.status, p.newstatus, issues.hashcode, sourcehascode, p.resumedate, p.reason, p.responseId);
                        }

                    }

                    //if the acknowledgebyrespondent is not equal thier are changes in the status
                    if (issues.acknowledgebyrespondent != p.acknowledgebyrespondent)
                    {
                        issues.acknowledgebyrespondent = p.acknowledgebyrespondent;
                        issues.acknowledgerespondentdate = DateTime.Now;
                       
                        issues.resumedate = p.resumedate;
                        issues.newstatus = p.newstatus;

                        if (p.newstatus == "P")
                        {
                            issues.parkdate = p.parkdate;
                            strResponse = respondentName + " requested change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason + " and it will resume on " + DateTime.Parse(p.resumedate.ToString()).ToShortDateString();
                        }
                        else
                        {
                            strResponse = respondentName + " requested change status from " + MifsStatus(issues.status) + " to " + MifsStatus(p.newstatus) + " with the following reason: " + p.reason;
                        }

                        var iresult = db.Sproc_mifsSetResponse(0, p.issueId, strResponse
                            , respondentName, p.responseId, DateTime.Now, respondentEmail).First().Value;


                        EmailNotification(sourceName, sourceEmail, respondentName, p.issueId, qSourceType.aliascode, issues.status, p.newstatus, issues.hashcode, sourcehascode, p.resumedate, p.reason, p.responseId);

                        // SmsNotification(sourceMobile, "Please check Issue #." + p.issueId.ToString() + " " + p.issuedetails, "MIFS");
                    }

                    //if the acknowledgedbysource + acknowledgebyrespondent equal to 2 they acknowledge both side parties
                    //set the status to  new status
                    if (issues.acknowledgedbysource + issues.acknowledgebyrespondent == 2)
                    {
                        issues.status = issues.newstatus;
                        p.newstatus = string.Empty;
                        SmsNotification(respondentMobile, "Issue #." + p.issueId.ToString() + " " + issues.issuedetails + " have change status from " + issues.status + " to " + issues.newstatus, "MIFS");
                        SmsNotification(sourceMobile, "Issue #." + p.issueId.ToString() + " " + issues.issuedetails + " have change status from " + issues.status + " to " + issues.newstatus, "MIFS");

                    }
                   
                    //effect changes
                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void EmailNotification(string recipientsName, string recipientsMail, string requestorName, int issueId, string sourceType, string statusFrom, string statusTo, string hascode, string respondentCode, DateTime? resumeDate, string closingReason, string recipientId)
        {
            try
            {
                PALOUPEntities db = new PALOUPEntities();

                var currentIssue = db.mifs_issues.Where(wr => wr.issueId == issueId).FirstOrDefault();

                string[] param = new string[20];

                param[0] = recipientsName;
                param[1] = requestorName;
                param[2] = MifsStatus(statusFrom);
                param[3] = MifsStatus(statusTo);
                param[4] = sourceType;
                param[5] = issueId.ToString();
                param[6] = currentIssue.issuedetails;
                param[7] = currentIssue.actionplan;
                param[8] = currentIssue.issuedate.ToShortDateString();
                param[9] = currentIssue.targetcompletiondate.ToShortDateString();
                param[10] = hascode;
                param[11] = respondentCode;
                param[12] = resumeDate.ToString()==string.Empty ? "" : resumeDate.Value.ToShortDateString();
                param[13] = closingReason;
                param[14] = recipientsMail;
                param[15] = recipientId;
                
                if (statusTo == "C" || statusTo == "O")
                {
                    mailParams.mailTemplatePath = "mailTemplate//mifs.request.chance.status.html";
                }
                else if(statusTo=="P")
                {
                    mailParams.mailTemplatePath = "mailTemplate//mifs.request.chance.status.park.html";
                }
                else if (statusTo == "D")
                {
                    mailParams.mailTemplatePath = "mailTemplate//mifs.request.chance.status.drop.html";
                }

                mailParams.recipientsMail = recipientsMail;
                mailParams.sendername = "Paloap System";
                mailParams.subject = "Requesting For Acknowledge";
                mailParams.param = param;
                mailParams.createdby = "Paloap";
                procedure.EmailNotification(mailParams);

 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SmsNotification(string mobile ,string strMsg, string appName)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Sproc_SendSms(mobile, strMsg, appName,"SYSTEM");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      

       

 

        public dynamic getPaloupIssueList(int id, string responseEmail)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qIssue = db.mifs_vIssues
                        .Where(wr => wr.CurrentTopicId == id && (wr.responemail==responseEmail || wr.raisedbyemail==responseEmail ))
                        .OrderByDescending(or => or.CreateDate)
                        .ToList()
                        .Select(aa => new
                        {
                            aa,
                            canResponse = isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, responseEmail)
                        });

                    return qIssue;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       


     

     
        public bool isCanupdateEmail(int topicId, string raisedMail, string responMail, string uEmail)
        {
            using (PALOUPEntities db = new PALOUPEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;

                if (uEmail.ToLower() == raisedMail.ToLower() || uEmail.ToLower() == responMail.ToLower())
                {
                    return true;
                }
                else
                {

                    return false;
                }

            }
        }
     

        private dynamic getAttachment(int id)
        {
            using (PALOUPEntities db = new PALOUPEntities())
            {
                return db.core_attachments.Where(wr => wr.attachtype == "R" && wr.transactionId == id).ToList()
                    .Select(ss => new { 
                        url = setURl(ss.filename,ss.filetype,ss.uploadtype),
                        ss.filename,
                        ss.uploadtype,
                        IsImage=IsImageExtension(ss.filetype.Remove(0,1).ToLower())
                    });
            }
        }

        private static readonly string[] _validExtensions = { "jpg", "bmp", "gif", "png" }; //  etc

        private static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }

        private string SourceTypeAlias(int id)
        {
            string strAlias = string.Empty;
            using (PALOUPEntities db = new PALOUPEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                strAlias = db.mifs_topic.Where(wr => wr.topicId == id).FirstOrDefault().aliascode;

                return strAlias;

            }
        }

        private string setURl(string filename, string filetype,string uploadtype)
        {
            try
            {
                if (uploadtype=="I")
                {
                    string imgConvert = ((NameValueCollection)ConfigurationSettings.GetConfig("appSettings"))["imgConvert"];
                
                    return imgConvert + filename + filetype;
                }
                else
                {
                    string otherFile = ((NameValueCollection)ConfigurationSettings.GetConfig("appSettings"))["otherFile"];
                   
                    return otherFile + filename + filetype;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic setDefer(int id, string reason)
        {
            try
            {
                string strSender = string.Empty;
                string strRecever = string.Empty;
                decimal? iresult = 0;
                string fromStatus, toStatus;
                MifsIssues n = new MifsIssues();
                string[] param = new string[20];
                string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;


                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                    fromStatus = MifsStatus(qIssue.status);
                    toStatus = MifsStatus(qIssue.newstatus);
                    if(toStatus != "")
                    {
                        if (qIssue.acknowledgedbysource == 1)
                        {
                            strSender = qIssue.responsible;
                            strRecever = qIssue.raisedby;
                            recipientMail = qIssue.raisedbyemail;
                            
                            //   iresult = db.Sproc_mifsSetResponse(0, id, reason, qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                            if (reason == "undefined" || reason == null || reason == "")
                            {
                                iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " defered to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + ""
                                                                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;


                            }
                            else
                            {
                                iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " defered to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + reason
                                                                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                            }

                            if (reason == "undefined" || reason == null || reason == "")
                            {


                                //Begin: sent notification
                                param[0] = strRecever;
                                param[1] = strSender;
                                param[2] = fromStatus;
                                param[3] = toStatus;
                                param[4] = qIssue.issueId.ToString();
                                param[5] = qIssue.issuedetails;
                                param[6] = qIssue.actionplan;
                                param[7] = ""; //Notes
                                param[8] = "defer";
                                mailParams.recipientsMail = recipientMail;
                                mailParams.ccMail = qIssue.responemail;
                                mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                                mailParams.sendername = strSender;
                                mailParams.subject = "[paloUp] Acknowledge Issue";
                                mailParams.param = param;
                                mailParams.createdby = "paloUp";
                                procedure.EmailNotification(mailParams);
                            }
                            else
                            {
                                param[0] = strRecever;
                                param[1] = strSender;
                                param[2] = fromStatus;
                                param[3] = toStatus;
                                param[4] = qIssue.issueId.ToString();
                                param[5] = qIssue.issuedetails;
                                param[6] = qIssue.actionplan;
                                param[7] = reason;
                                param[8] = "defer";
                                mailParams.recipientsMail = recipientMail;
                                mailParams.ccMail = qIssue.responemail;
                                mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                                mailParams.sendername = strSender;
                                mailParams.subject = "[paloUp] Acknowledge Issue";
                                mailParams.param = param;
                                mailParams.createdby = "paloUp";
                                procedure.EmailNotification(mailParams);
                            }
                        }
                        else
                        {
                            strSender = qIssue.raisedby;
                            strRecever = qIssue.responsible;
                            recipientMail = qIssue.responemail;
                            // iresult = db.Sproc_mifsSetResponse(0, id, reason, qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                            if (reason == "undefined" || reason == null || reason == "")
                            {
                                iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " defered to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + ""
                                                                        , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                            }
                            else
                            {
                                iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " defered to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + reason
                                                                    , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                            }

                            if (reason == "undefined" || reason == null || reason == "")
                            {


                                //Begin: sent notification
                                param[0] = strRecever;
                                param[1] = strSender;
                                param[2] = fromStatus;
                                param[3] = toStatus;
                                param[4] = qIssue.issueId.ToString();
                                param[5] = qIssue.issuedetails;
                                param[6] = qIssue.actionplan;
                                param[7] = ""; //Notes
                                param[8] = "defer";
                                mailParams.recipientsMail = recipientMail;
                                mailParams.ccMail = qIssue.raisedbyemail;
                                mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                                mailParams.sendername = strSender;
                                mailParams.subject = "[paloUp] Acknowledge Issue";
                                mailParams.param = param;
                                mailParams.createdby = "paloUp";
                                procedure.EmailNotification(mailParams);
                            }
                            else
                            {
                                param[0] = strRecever;
                                param[1] = strSender;
                                param[2] = fromStatus;
                                param[3] = toStatus;
                                param[4] = qIssue.issueId.ToString();
                                param[5] = qIssue.issuedetails;
                                param[6] = qIssue.actionplan;
                                param[7] = reason;
                                param[8] = "defer";
                                mailParams.recipientsMail = recipientMail;
                                mailParams.ccMail = qIssue.raisedbyemail;
                                mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                                mailParams.sendername = strSender;
                                mailParams.subject = "[paloUp] Acknowledge Issue";
                                mailParams.param = param;
                                mailParams.createdby = "paloUp";
                                procedure.EmailNotification(mailParams);
                            }
                        }


                        var qCurrentResponse = db.mifs_vResponse.Where(wr => wr.issueId == qIssue.issueId).ToList().Select(aa => new
                        {
                            aa,
                            Attachment = getAttachment(aa.responseId)
                        }).ToList();


                        qIssue.newstatus = string.Empty;
                        qIssue.acknowledgedbysource = 0;
                        qIssue.acknowledgebyrespondent = 0;

                          db.SaveChanges();


                       

                        return qCurrentResponse;
                    }
                    else
                    {
                        throw new Exception("Please Refresh!! Issue is Already Updated to " + fromStatus);
                    }
                
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}