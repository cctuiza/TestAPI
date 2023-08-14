using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;
using PaloUpAPI.Models.MIFS;
using System.Data.Entity;

using System.Data.SqlClient;

namespace PaloUpAPI.SQLData
{
    public class MifsReport
    {
        public dynamic getMifsIssue(string status, string userEmail)
        {
            try
            {
                TransList transList = new TransList();
                //int skip = tk * (pg - 1); // skip the record
                List<int> topicIdLst = new List<int>();

                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    string memberId = db.mifs_members.Where(m => m.emailaddress == userEmail).FirstOrDefault().memberId;
                    var qTopic = db.mifs_topicmembers.Where(wr => wr.memberId == memberId).ToList();

                    foreach (var topic in qTopic)
                    {
                        topicIdLst.Add(topic.topicId);
                    }

                    if (string.IsNullOrEmpty(status))
                    {


                        var meetingTypes = db.mifs_vIssueReport
                            .Where(wr => topicIdLst.Contains(wr.CurrentTopicId))
                       .OrderBy(od => od.DateIssued)
                     
                       .ToList()
                       .Select(aa => new
                       {
                           aa.issueId,
                           aa.Details,
                           aa.pflag,
                           aa.DateIssued,
                           aa.TopicAlias,
                           aa.actionplan,
                           aa.targetcompletiondate,
                           aa.raisedby,
                           aa.responsible,
                           update = setNewLine(aa.Updates),
                           aa.FollowUpCounter,
                           aa.collaboratorname
                       })
                       .OrderBy(m => m.TopicAlias)
                       .ThenBy(m => m.pflag);

                        transList.total_count = db.mifs_vIssueReport
                                     .Where(wr => topicIdLst.Contains(wr.CurrentTopicId))
                       .OrderBy(od => od.DateIssued)
                       
                       .Count();

                        transList.data = meetingTypes;
                        return transList;
                    }
                    else
                    {
                        var meetingTypes = db.mifs_vIssueReport
                        .Where(wr => wr.IssueStatus == status && topicIdLst.Contains(wr.CurrentTopicId))
                       .OrderBy(od => od.DateIssued)
                      
                       .ToList()
                       .Select(aa => new
                       {
                           aa.issueId,
                           aa.Details,
                           aa.pflag,
                           aa.DateIssued,
                           aa.TopicAlias,
                           aa.actionplan,
                           aa.targetcompletiondate,
                           aa.raisedby,
                           aa.responsible,
                           update = setNewLine(aa.Updates),
                           aa.FollowUpCounter,
                           aa.collaboratorname
                       })
                            .OrderBy(m => m.TopicAlias)
                       .ThenBy(m => m.pflag); ;

                        transList.total_count = db.mifs_vIssueReport
                        .Where(wr => wr.IssueStatus == status && topicIdLst.Contains(wr.CurrentTopicId))
                       .OrderBy(od => od.DateIssued)
                       
                       .Count();

                        transList.data = meetingTypes;
                        return transList;
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public dynamic getMifsIssueOverdue(int pg, int tk, string userEmail)
        {
            try
            {
                TransList transList = new TransList();
                int skip = tk * (pg - 1); // skip the record
                List<int> topicIdLst = new List<int>();


                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    string memberId = db.mifs_members.Where(m => m.emailaddress == userEmail).FirstOrDefault().memberId;
                    var qTopic = db.mifs_topicmembers.Where(wr => wr.memberId == memberId).ToList();

                    foreach (var topic in qTopic)
                    {
                        topicIdLst.Add(topic.topicId);
                    }

                    var meetingTypes = db.mifs_vIssueReport
                                .Where(wr => wr.IssueStatus == "O")
                                  .Where(wr => topicIdLst.Contains(wr.CurrentTopicId))
                                   .Where(w => (DateTime.Now > DbFunctions.TruncateTime(w.targetcompletiondate)))
                       .OrderBy(od => od.DateIssued)
                       .Skip(skip)
                       .Take(tk)
                       .ToList()
                       .Select(aa => new
                       {
                           aa.issueId,
                           aa.DateIssued,
                           aa.TopicAlias,
                           aa.actionplan,
                           aa.targetcompletiondate,
                           aa.raisedby,
                           aa.responsible,
                           update = setNewLine(aa.Updates),
                           aa.FollowUpCounter
                       });

                    transList.total_count = db.mifs_vIssueReport
                          .Where(wr => wr.IssueStatus == "O")
                            .Where(wr => topicIdLst.Contains(wr.CurrentTopicId))
                               .Where(w => (DateTime.Now > DbFunctions.TruncateTime(w.targetcompletiondate)))
                   .OrderBy(od => od.DateIssued)
                   .Skip(skip)
                   .Take(tk)
                   .Count();

                    transList.data = meetingTypes;
                    return transList;


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic getMyDashboard(string id)
        {
            try
            {
                List<MifsDashboard> dashboardLst = new List<MifsDashboard>();
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qMyTopic = db.mifs_vTopicMembers
                        .Where(wr => wr.MemberEmailAddress == id && wr.status == "A")
                        .ToList();

                    foreach (var row in qMyTopic)
                    {
                        dashboardLst.Add(new MifsDashboard
                        {
                            topicId = row.topicId,
                            topicDes = row.TopicDescription,
                            iOpen = iStatusCount(row.topicId, "O"),
                            iPark = iStatusCount(row.topicId, "P"),
                            iClose = iStatusCount(row.topicId, "C"),
                            iDrop = iStatusCount(row.topicId, "D")
                        });
                    }

                    return dashboardLst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string setNewLine(string update)
        {
            string strUpdate = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(update))
                    return string.Empty;

                string[] arUpdate = update.Split(';');
                int newTotalLength = arUpdate.Length - 1;
                if (newTotalLength == 1)
                {
                    strUpdate = update.Remove(update.Length - 2, 1);
                }
                else
                {


                    for (int i = 0; i < newTotalLength; i++)
                    {
                        if (i == newTotalLength)
                        {
                            strUpdate = strUpdate + arUpdate[i];
                        }
                        else
                        {
                            strUpdate = strUpdate + arUpdate[i] + "</br>";
                        }
                    }
                }

                //return new output
                return strUpdate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private int iStatusCount(int topicId, string Status)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qIssue = db.mifs_vIssues
                        .Where(wr => wr.CurrentTopicId == topicId)
                        .Where(wr => wr.IssueStatus == Status)
                        .ToList()
                        .Count();

                    return qIssue;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic GetParameters()
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                   
                    var getYr = db.Database.SqlQuery<MifsGetIssuedYear>("select distinct IssuedYear from mifs_vIssuePerBranchPerStatus").ToList();
                    var corporateName = db.Database.SqlQuery<MifsGetCorporateName>("select distinct CorporateName  from mifs_vIssuePerBranchPerStatus").ToList();
                    var branch = db.Database.SqlQuery<MifsGetBusinessGroup>("select distinct BusinessGroup from mifs_vIssuePerBranchPerStatus").ToList();

                    var responseData = new
                    {
                        years = getYr,
                        corporateName = corporateName,
                        branch = branch
                    };

                    return responseData;
                }
                
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public dynamic GetMonthlyTrendPerIssue(string sbu, string branch, int currentYear)
        {
            List<string> prefixMonth = new List<string>();
            List<int> openMonth = new List<int>();
            List<int> parkMonth = new List<int>();
            List<int> closeMonth = new List<int>();
            List<int> dropMonth = new List<int>();
         
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    //var resData = db.mifs_vIssuePerBranchPerStatus
                    //                .Where(m => m.IssuedYear == currentYear && m.CorporateName == sbu && m.BusinessGroup == branch)
                    //                .OrderBy(m => m.IssuedMonth)
                    //                .Select(m => new { m.IssuedMonth })
                    //                .Distinct()
                    //                .ToList();
                    var resData = db.Database.SqlQuery<MifsIssuePerBranchPerStatus>("SELECT SUM(TotalOpen) AS SumOpen, SUM(TotalClosed) AS SumClosed, SUM(TotalParked) AS SumParked, SUM(TotalDropped) AS SumDropped, IssuedYear, IssuedMonth " +
"FROM            mifs_vIssuePerBranchPerStatus " +
"WHERE(IssuedYear ="+ currentYear +") AND(CorporateName = '"+ sbu + "') AND(BusinessGroup = '"+ branch  + "') " +
"GROUP BY IssuedYear, IssuedMonth ")
                      .ToList();

                    foreach (var resultValue in resData)
                    {
                        var openCount = resultValue.SumOpen;

                        var parkCount = resultValue.SumParked;

                        var closeCount = resultValue.SumClosed;

                        var dropCount = resultValue.SumDropped;

                        openMonth.Add(openCount);
                        parkMonth.Add(parkCount);
                        closeMonth.Add(closeCount);
                        dropMonth.Add(dropCount);
                        prefixMonth.Add(System.Globalization.DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName((int)resultValue.IssuedMonth));
                    }

                    var responseData = new
                    {
                        prefixMonth = prefixMonth,
                        totalMonthOpen = openMonth,
                        totalMonthPark = parkMonth,
                        totalMonthClose = closeMonth,
                        totalMonthDrop = dropMonth
                    };

                    return responseData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public dynamic GetWeeklyTrendPerIssue(string sbu, string branch, int currentYear)
        {

            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    List<string> weeks = new List<string>();
                    List<int> openweeks = new List<int>();
                    List<int> parkweeks = new List<int>();
                    List<int> closeweeks = new List<int>();
                    List<int> dropweeks = new List<int>();


                    var resDatas = db.Database.SqlQuery<MifsIssuePerBranchPerStatusWeekly>("SELECT   top 8     SUM(TotalOpen) AS SumOpen, SUM(TotalClosed) AS SumClosed, SUM(TotalParked) AS SumParked, SUM(TotalDropped) AS SumDropped, IssuedYear, IssuedWeek "+
"FROM            mifs_vIssuePerBranchPerStatus WHERE(IssuedYear =" + currentYear + ") AND(CorporateName = '" + sbu + "') AND(BusinessGroup = '" + branch + "') " +
"GROUP BY IssuedYear, IssuedWeek "+
"ORDER BY IssuedWeek DESC ")
    .ToList();



                    foreach (var resWeek in resDatas.OrderBy(or => or.IssuedWeek).ToList())
                    {
                        var openCount = resWeek.SumOpen;

                        var parkCount = resWeek.SumParked;

                        var closeCount = resWeek.SumClosed;

                        var dropCount = resWeek.SumDropped;

                        openweeks.Add(openCount);
                        parkweeks.Add(parkCount);
                        closeweeks.Add(closeCount);
                        dropweeks.Add(dropCount);
                        weeks.Add("w" + resWeek.IssuedWeek.ToString());
                    }

                    var responseData = new
                    {
                        lstweek = weeks,
                        totalWeeksOpen = openweeks,
                        totalWeeksPark = parkweeks,
                        totalWeeksClose = closeweeks,
                        totalWeeksDrop = dropweeks
                    };
                    return responseData;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}