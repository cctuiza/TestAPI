using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.SQLData;
using PaloUpAPI.Models.MIFS;
using System.IO;
using System.Data.Entity;
using System.Web.Http.ExceptionHandling;
using PaloUpAPI.ErrorHelper;
using System.Data.Entity.Validation;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Globalization;
using PaloUpAPI.Models;
using PaloUpAPI.ActionFilters;
using PaloUpAPI.Class;
using System.Data.SqlClient;
using System.Data;
using KellermanSoftware.CompareNetObjects;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using System.Dynamic;

namespace PaloUpAPI.Controllers
{
    //[AuthorizationRequired]
    public class MifsController : ApiController
    {

        PALOUPEntities db = new PALOUPEntities();
        //TempPaloUpModel db2 = new TempPaloUpModel();
        Library phpLib = new Library();
        MailParamsModel mailParams = new MailParamsModel();

        clsprocedure procedure = new clsprocedure();

        #region "Dashboard"

        
        [HttpGet]
        public HttpResponseMessage GetPerTopicDashboard(int topicId, int iyr ,int ? weekCount =8)
        {
            db.Configuration.LazyLoadingEnabled = false;
            List<string> weeks = new List<string>();
            List<int> openweeks = new List<int>();
            List<int> parkweeks = new List<int>();
            List<int> closeweeks = new List<int>();
            List<int> dropweeks = new List<int>();
            try
            {

                var resData = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                     .OrderByDescending(m => m.IssuedWeek)
                     .Select(m => new { m.IssuedWeek }).Take((int)weekCount).ToList();



                foreach (var resWeek in resData.OrderBy(or => or.IssuedWeek).ToList())
                {
                    var openCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedWeek == resWeek.IssuedWeek)
                     .Sum(m => m.TotalOpen).Value;

                    var parkCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedWeek == resWeek.IssuedWeek)
                     .Sum(m => m.TotalParked).Value;

                    var closeCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedWeek == resWeek.IssuedWeek)
                     .Sum(m => m.TotalClosed).Value;

                    var dropCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedWeek == resWeek.IssuedWeek)
                     .Sum(m => m.TotalDropped).Value;

                    openweeks.Add(openCount);
                    parkweeks.Add(parkCount);
                    closeweeks.Add(closeCount);
                    dropweeks.Add(dropCount);
                    weeks.Add("w" + resWeek.IssuedWeek.ToString());
                }

                var responseData = new { lstweek = weeks,
                totalWeeksOpen= openweeks,
                totalWeeksPark=parkweeks,
                totalWeeksClose=closeweeks,
                totalWeeksDrop=dropweeks
                };

                var result = new {
                    weekly = responseData,
                    monthly = IssuePerMonthlyPerStatus(topicId,iyr),
                     pieGraph = IssuePerStatus(topicId, iyr)
                };
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

      
        public dynamic IssuePerMonthlyPerStatus(int topicId, int iyr)
        {
            DateTimeFormatInfo mfi = new DateTimeFormatInfo();
            db.Configuration.LazyLoadingEnabled = false;
            List<string> months = new List<string>();
            List<int> openMonths = new List<int>();
            List<int> parkMonths = new List<int>();
            List<int> closeMonths = new List<int>();
            List<int> dropMonths = new List<int>();
            try
            {


                var resData = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                     .OrderByDescending(m => m.IssuedMonth)
                     .Select(m => new { m.IssuedMonth }).Distinct().ToList();



                foreach (var resMonth in resData.OrderBy(or => or.IssuedMonth).ToList())
                {
                    var openCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedMonth == resMonth.IssuedMonth)
                     .Sum(m => m.TotalOpen).Value;

                    var parkCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedMonth == resMonth.IssuedMonth)
                     .Sum(m => m.TotalParked).Value;

                    var closeCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedMonth == resMonth.IssuedMonth)
                     .Sum(m => m.TotalClosed).Value;

                    var dropCount = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId && m.IssuedMonth == resMonth.IssuedMonth)
                     .Sum(m => m.TotalDropped).Value;

                    openMonths.Add(openCount);
                    parkMonths.Add(parkCount);
                    closeMonths.Add(closeCount);
                    dropMonths.Add(dropCount);
                    months.Add(mfi.GetMonthName((int)resMonth.IssuedMonth).ToString().Substring(0, 3));
                }

                var responseData = new
                {
                    listMonth = months,
                    totalMonthOpen = openMonths,
                    totalMonthPark = parkMonths,
                    totalMonthClose = closeMonths,
                    totalMonthDrop = dropMonths
                };
                return responseData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public dynamic IssuePerStatus(int topicId, int iyr)
        {
            DateTimeFormatInfo mfi = new DateTimeFormatInfo();
            db.Configuration.LazyLoadingEnabled = false;
            int totalOpen = 0;
            int totalPark = 0;
            int totalClose = 0;
            int totalDrop = 0;
            try
            {

                if (db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId).Count() > 0)
                totalOpen = db.mifs_vIssuePerWeekPerStatus
                     .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                     .Select(m => new { m.TotalOpen }).Sum(m => m.TotalOpen).Value ;


                if (db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId).Count() > 0)
                totalPark = db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                  .Select(m => new { m.TotalParked }).Sum(m => m.TotalParked).Value;

                if(db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId).Count() > 0)
                totalClose = db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                  .Select(m => new { m.TotalClosed }).Sum(m => m.TotalClosed).Value;

                if(db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId).Count() >0)
                totalDrop = db.mifs_vIssuePerWeekPerStatus
                  .Where(m => m.IssuedYear == iyr && m.CurrentTopicId == topicId)
                  .Select(m => new { m.TotalDropped }).Sum(m => m.TotalDropped).Value;



                var responseData = new
                {
                    totalOpen,
                    totalPark,
                    totalClose,
                    totalDrop
                };
                return responseData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMonthlyAndWeeklyTrendPerIssue(string sbu,string branch, int currentYear )
        {

            MifsReport mifsReport = new MifsReport();
            try
            {
                var responseData = new {param=mifsReport.GetParameters(),
                    monthly = mifsReport.GetMonthlyTrendPerIssue(sbu, branch, currentYear),
                    weekly = mifsReport.GetWeeklyTrendPerIssue(sbu, branch, currentYear)
                };
                return Request.CreateResponse(HttpStatusCode.OK, responseData);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        public HttpResponseMessage GetUserIssueStackedChart(string userEmail)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                MifsDashboardViewModel model = new MifsDashboardViewModel();
                StackColumnViewModel stackCol = new StackColumnViewModel();
                List<StackColumnData> dataList = new List<StackColumnData>();
                List<TreeMapViewModel> resTreeMap = new List<TreeMapViewModel>();
                //List<TreeMapViewModel> tollTreeMap = new List<TreeMapViewModel>();
                List<int> openCount = new List<int>();
                List<int> parkCount = new List<int>();
                List<int> closeCount = new List<int>();
                List<int> dropCount = new List<int>();
                List<string> cat = new List<string>();

                var issueList = db.mifs_vIssues.Where(m => (m.responemail == userEmail || m.collaboratoremail == userEmail || m.raisedbyemail == userEmail)).ToList();

                ////Responsible
                openCount.Add(issueList.Where(m => m.responemail == userEmail && m.IssueStatus == "O").Count());
                parkCount.Add(issueList.Where(m => m.responemail == userEmail && m.IssueStatus == "P").Count());
                closeCount.Add(issueList.Where(m => m.responemail == userEmail && m.IssueStatus == "C").Count());
                dropCount.Add(issueList.Where(m => m.responemail == userEmail && m.IssueStatus == "D").Count());

                ////Collaborator
                openCount.Add(issueList.Where(m => m.collaboratoremail == userEmail && m.IssueStatus == "O").Count());
                parkCount.Add(issueList.Where(m => m.collaboratoremail == userEmail && m.IssueStatus == "P").Count());
                closeCount.Add(issueList.Where(m => m.collaboratoremail == userEmail && m.IssueStatus == "C").Count());
                dropCount.Add(issueList.Where(m => m.collaboratoremail == userEmail && m.IssueStatus == "D").Count());

                ////Tollgater
                openCount.Add(issueList.Where(m => m.raisedbyemail == userEmail && m.IssueStatus == "O").Count());
                parkCount.Add(issueList.Where(m => m.raisedbyemail == userEmail && m.IssueStatus == "P").Count());
                closeCount.Add(issueList.Where(m => m.raisedbyemail == userEmail && m.IssueStatus == "C").Count());
                dropCount.Add(issueList.Where(m => m.raisedbyemail == userEmail && m.IssueStatus == "D").Count());

                StackColumnData Open = new StackColumnData();
                Open.name = "Open";
                Open.data = openCount;

                StackColumnData Park = new StackColumnData();
                Park.name = "Park";
                Park.data = parkCount;

                StackColumnData Close = new StackColumnData();
                Close.name = "Close";
                Close.data = closeCount;


                StackColumnData Drop = new StackColumnData();
                Drop.name = "Drop";
                Drop.data = dropCount;

                dataList.Add(Open);
                dataList.Add(Park);
                dataList.Add(Close);
                dataList.Add(Drop);

                stackCol.categories = cat;
                stackCol.series = dataList;

                cat.Add("Responsible");
                cat.Add("Collaborator");
                cat.Add("Tollgater");


                model.stackColumn = stackCol;

                TreeMapViewModel tempreshead = new TreeMapViewModel()
                {
                    id= "responsible",
                    name = "For Response",
                    color = "#2196F3",
                    value = 33
                };
                resTreeMap.Add(tempreshead);
                TreeMapViewModel temptoolgaterhead = new TreeMapViewModel()
                {
                    id = "tollgater",
                    name = "For Tollgating",
                    color = "#FF9800",
                    value = 33
                };
                resTreeMap.Add(temptoolgaterhead);
                TreeMapViewModel tempcolabhead = new TreeMapViewModel()
                {
                    id = "collaborator",
                    name = "For Collab",
                    color = "#009688",
                   value=33
                };
                resTreeMap.Add(tempcolabhead);


               

                //get issue owner
                var qowner = db.mifs_issues.Where(m => m.responemail == userEmail && m.status == "O")
                    .ToList().Select(m => new { m.raisedby, m.issueId })
                    .GroupBy(p => p.raisedby)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach(var row in qowner)
                {
                    TreeMapViewModel temp= new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "responsible",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }


                //get issue responsible
                var qresponsible = db.mifs_issues.Where(m => m.raisedbyemail == userEmail && m.status == "O")
                    .ToList().Select(m => new { m.responsible, m.issueId })
                    .GroupBy(p => p.responsible)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach (var row in qresponsible)
                {
                    TreeMapViewModel temp = new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "tollgater",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }

                //get issue responsible
                var qcolab= db.mifs_issues.Where(m => m.collaboratoremail == userEmail && m.status == "O")
                    .ToList().Select(m => new { m.raisedby, m.issueId })
                    .GroupBy(p => p.raisedby)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach (var row in qcolab)
                {
                    TreeMapViewModel temp = new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "collaborator",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }


                //set box side
                if (qowner.Count() == 0)
                    resTreeMap.Where(m => m.id == "responsible").FirstOrDefault().value = 0;

                if (qresponsible.Count() == 0)
                    resTreeMap.Where(m => m.id == "tollgater").FirstOrDefault().value = 0;

                if(qcolab.Count() == 0)
                    resTreeMap.Where(m => m.id == "collaborator").FirstOrDefault().value = 0;



                model.responsibleTreeMap = resTreeMap;

           

                // EnD TREE MAP

                return Request.CreateResponse(HttpStatusCode.OK, model);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUserReviewGraph(string userEmail, int topicId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                MifsDashboardViewModel model = new MifsDashboardViewModel();
                List<TreeMapViewModel> resTreeMap = new List<TreeMapViewModel>();
               

                TreeMapViewModel tempreshead = new TreeMapViewModel()
                {
                    id = "responsible",
                    name = "For Response",
                    color = "#2196F3",
                    value = 33
                };
                resTreeMap.Add(tempreshead);
                TreeMapViewModel temptoolgaterhead = new TreeMapViewModel()
                {
                    id = "tollgater",
                    name = "For Tollgating",
                    color = "#FF9800",
                    value = 33
                };
                resTreeMap.Add(temptoolgaterhead);
                TreeMapViewModel tempcolabhead = new TreeMapViewModel()
                {
                    id = "collaborator",
                    name = "For Collab",
                    color = "#009688",
                    value = 33
                };
                resTreeMap.Add(tempcolabhead);




                //get issue owner
                var qowner = db.mifs_issues.Where(m => m.responemail == userEmail && m.status == "O" && m.topicId== topicId)
                    .ToList().Select(m => new { m.raisedby, m.issueId })
                    .GroupBy(p => p.raisedby)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach (var row in qowner)
                {
                    TreeMapViewModel temp = new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "responsible",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }


                //get issue responsible
                var qresponsible = db.mifs_issues.Where(m => m.raisedbyemail == userEmail && m.status == "O" && m.topicId == topicId)
                    .ToList().Select(m => new { m.responsible, m.issueId })
                    .GroupBy(p => p.responsible)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach (var row in qresponsible)
                {
                    TreeMapViewModel temp = new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "tollgater",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }

                //get issue responsible
                var qcolab = db.mifs_issues.Where(m => m.collaboratoremail == userEmail && m.status == "O" && m.topicId == topicId)
                    .ToList().Select(m => new { m.raisedby, m.issueId })
                    .GroupBy(p => p.raisedby)
                    .Select(g => new { name = g.Key, count = g.Count() });

                foreach (var row in qcolab)
                {
                    TreeMapViewModel temp = new TreeMapViewModel()
                    {
                        name = row.name,
                        parent = "collaborator",
                        value = row.count,
                    };
                    resTreeMap.Add(temp);
                }


                //set box side
                if (qowner.Count() == 0)
                    resTreeMap.Where(m => m.id == "responsible").FirstOrDefault().value = 0;

                if (qresponsible.Count() == 0)
                    resTreeMap.Where(m => m.id == "tollgater").FirstOrDefault().value = 0;

                if (qcolab.Count() == 0)
                    resTreeMap.Where(m => m.id == "collaborator").FirstOrDefault().value = 0;



                model.responsibleTreeMap = resTreeMap;



                // EnD TREE MAP

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetIssueCategoryGraph(string userEmail)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var issuecategorygraph = db.Sproc_mifsIssuesCategoryGraph(userEmail).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, issuecategorygraph);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }






        #endregion

        #region "Topic"

        [HttpGet]
        [ActionName("get-topic-hashcode")]
        public HttpResponseMessage GetTopicHashCode(int topicId)
        {
            try
            {
                string hashcode = "";
                var topic = db.mifs_topic.Where(m => m.topicId == topicId).ToList();
                if (topic.Count > 0)
                {
                    hashcode = topic.First().hashcode;
                }
                return Request.CreateResponse(HttpStatusCode.OK, hashcode);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-source-types")]
        public HttpResponseMessage getSourceTypes(int pg, int tk)
        {

            db.Configuration.LazyLoadingEnabled = false;

            TransList transList = new TransList();
            int skip = tk * (pg - 1); // skip the record

            var meetingTypes = db.mifs_topic
                .OrderBy(od => od.aliascode)
            .Skip(skip)
            .Take(tk)
            .ToList();

            transList.total_count = db.mifs_topic.Count();
            transList.data = meetingTypes;

            if (transList != null)
                return Request.CreateResponse(HttpStatusCode.OK, transList);

            throw new ApiDataException(1000, "No Results Found!", HttpStatusCode.NotFound);
        }

        [HttpGet]
        [ActionName("get-source-types-by-id")]
        public HttpResponseMessage getSourceTypesbyId(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var meetingTypes = db.mifs_topic.Where(wr => wr.topicId == id).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, meetingTypes);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-topic-by-memberId")]
        public HttpResponseMessage getTopicbyMemberId(string memberId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var TopicList = db.mifs_vTopicMembers.Where(wr => wr.memberId == memberId).Select(mm=> new {
                //topicallias = db.mifs_topic.Where(ss => ss.topicId == mm.topicId).Select(ff => ff.aliascode).FirstOrDefault(),
                mm.topicId,
                mm.TopicDescription,
                branchname = db.core_branchmst.Where(ss => ss.brancode == db.mifs_topic.Where(ff => ff.topicId == mm.topicId).Select(ff => ff.branchcode).FirstOrDefault()).Select(ss => ss.branname).FirstOrDefault(),
                districtname = db.core_district.Where(ss => ss.districtcode == db.mifs_topic.Where(ff => ff.topicId == mm.topicId).Select(ff => ff.districtcode).FirstOrDefault()).Select(ss => ss.districtname).FirstOrDefault(),
                mm.CreateDate

                })
                    
                    
                    .ToList();

                return Request.CreateResponse(HttpStatusCode.OK, TopicList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-source-types")]
        public dynamic postSourceTypes(mifs_topic p)
        {
            try
            {
                MifsSourceType n = new MifsSourceType();
                n.post(p);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [ActionName("delete-source-types")]
        public dynamic deleteSourceTypes(int id)
        {
            try
            {
                MifsSourceType n = new MifsSourceType();
                n.delete(id);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        #endregion

        #region "Member"

        [HttpGet]
        [ActionName("get-member")]
        public dynamic getMember(int pg, int tk, string fnd)
        {
            try
            {
                MifsMember n = new MifsMember();
                return Json(new { hasError = false, data = n.get(pg, tk, fnd) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [ActionName("get-member")]
        public dynamic getMember(string id)
        {
            try
            {
                MifsMember n = new MifsMember();
                return Json(new { hasError = false, data = n.get(id) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        [ActionName("post-member")]
        public HttpResponseMessage postMember(mifs_members p)
        {
            try
            {


                var empInfo = db.core_employee.Where(wr => wr.empl_id == p.memberId).FirstOrDefault();

                if (db.mifs_members.Where(aa => aa.memberId == p.memberId).Count() > 0)
                {
                    p.create_date = DateTime.Now.Date;
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    string mname = empInfo.mname.Length > 0 ? empInfo.mname.Trim().Substring(0, 1) : "";

                    p.fullname = empInfo.employeename2;
                    p.emailaddress = empInfo.emailadd;
                    p.create_date = DateTime.Now;
                    p.mobileno = string.Empty;
                    p.nickname = empInfo.fname.Trim().Substring(0, 1) + mname + empInfo.lname.Trim().Substring(0, 1);
                    p.status = "A";
                    db.Entry(p).State = EntityState.Added;
                    db.SaveChanges();

                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("put-member")]
        public HttpResponseMessage putMember(mifs_members p)
        {
            try
            {
                if (db.mifs_members.Where(aa => aa.memberId == p.memberId).Count() > 0)
                {
                    p.create_date = DateTime.Now.Date;
                    db.Entry(p).State = EntityState.Modified;

                    var qTopicMember = db.mifs_topicmembers.Where(wr => wr.memberId == p.memberId).ToList();


                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("delete-member")]
        public HttpResponseMessage deleteMember(string id)
        {
            try
            {
                if (db.mifs_topicmembers.Where(wr => wr.memberId == id).Count() > 1)
                {
                    Exception ex = new Exception("Invalid to delete this item it's being use in Source Member Module");
                    throw ex;
                }
                else
                {
                    mifs_members n = db.mifs_members.Where(wr => wr.memberId == id).FirstOrDefault();
                    db.mifs_members.Remove(n);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [ActionName("get-all-employee")]
        public dynamic GetAllEmployee(int page, int size)
        {
            MifsMember n = new MifsMember();
            return Json(new { data = n.GetAllEmployee(page, size) });
        }

        [HttpGet]
        [ActionName("get-search-employee")]
        public dynamic GetSearchEmployee(int page, int size, string searchParam)
        {
            MifsMember n = new MifsMember();
            return Json(new { data = n.GetSearchEmployee(page, size, searchParam) });
        }

        [HttpGet]
        [ActionName("get-all-employee-hris")]
        public dynamic GetAllEmployeeHris(int page, int size)
        {
            MifsMember n = new MifsMember();
            return Json(new { data = n.GetAllEmployeeHris(page, size) });
        }
        [HttpGet]
        [ActionName("get-search-employee-hris")]
        public dynamic GetSearchEmployeeHris(int page, int size, string searchParam)
        {
            MifsMember n = new MifsMember();
            return Json(new { data = n.GetSearchEmployeeHris(page, size, searchParam) });
        }

        #endregion

        #region "Topic Member"


        //Original Topic Member
        [HttpGet]
        [ActionName("get-topic-members")]
        public HttpResponseMessage getTopicMemberByTopic(int id, int pg, int tk)
        {
            MaterialDataModel res = new MaterialDataModel();
            try
            {

                int skip = tk * (pg - 1); // skip the record

                db.Configuration.LazyLoadingEnabled = false;

                var sourceMembers = db.mifs_vTopicMembers
                 .Where(wr => wr.topicId == id)
                 .OrderBy(od => od.MemberName)
                 .Skip(skip)
                 .Take(tk)
                 .ToList();

                res.count = db.mifs_vTopicMembers
                            .Where(wr => wr.topicId == id)
                            .Count();

                res.data = sourceMembers;
                return Request.CreateResponse(HttpStatusCode.OK, res);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //Topic Member without Pagination
        [HttpGet]
        [ActionName("get-topic-members")]
        public HttpResponseMessage getTopicMember(int id)
        {
            MaterialDataModel res = new MaterialDataModel();
            try
            {

                

                db.Configuration.LazyLoadingEnabled = false;

                var sourceMembers = db.mifs_vTopicMembers
                 .Where(wr => wr.topicId == id)
                 .OrderBy(od => od.MemberName)
               
                 .ToList();

                //res.count = db.mifs_vTopicMembers
                //            .Where(wr => wr.topicId == id)
                //            .Count();

                //res.data = sourceMembers;
                return Request.CreateResponse(HttpStatusCode.OK, sourceMembers);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-topic-members-by-hashcode")]
        public HttpResponseMessage getTopicMemberByHascode(string topicHashcode, string meetingHashcode, string crea_by)
        {
            MaterialDataModel res = new MaterialDataModel();
            try
            {
                //List<string> accessleveltype = new List<string>();
                //accessleveltype.Add("M");
                //accessleveltype.Add("O");

                var meetingId = db.mifs_meeting.Where(wr => wr.hashcode == meetingHashcode).FirstOrDefault().meetingId;

                db.Configuration.LazyLoadingEnabled = false;


                var topicMembers = db.mifs_vTopicMembers
                 .Where(wr => wr.hashcode == topicHashcode)// && accessleveltype.Contains(wr.accesslevel))
                 .OrderBy(od => od.MemberName)
                 .ToList();

                var qAttendance = db.mifs_attendance.Where(wr => wr.meetingId == meetingId).ToList();

                res.count = db.mifs_vTopicMembers
                            .Where(wr => wr.hashcode == topicHashcode)//&& accessleveltype.Contains(wr.accesslevel))
                                .Count();

                res.data = topicMembers;

                //add to meeting attendance
                foreach (var row in topicMembers)
                {

                    if (qAttendance.Where(wr => wr.meetingId == meetingId && wr.attendeemail == row.MemberEmailAddress).Count() == 0)
                    {
                        db.Sproc_mifsSetAttendance(0, meetingId, row.memberId,
                       row.MemberName, row.MemberEmailAddress,
                       row.MemberMobileNumber,
                       1, 1,
                       crea_by, DateTime.Now);
                    }

                }

                //get topic details
                var topic = db.mifs_topic.Where(m => m.hashcode == topicHashcode).Select(m => new
                {
                    m.aliascode,
                    m.bgcolor,
                    m.created_by,
                    m.create_date,
                    m.description,
                    m.fupfrequency,
                    m.hashcode,
                    m.occurence,
                    m.status,
                    m.topicId,
                    m.topiclevel,
                    m.topicscope,
                    m.branchcode,
                    m.districtcode
                }).FirstOrDefault();

                res.obj1 = topic;

                return Request.CreateResponse(HttpStatusCode.OK, res);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-source-detail-by-hashcode")]
        public HttpResponseMessage getSourceDetailsbyHascode(string id)
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;
                var qTopic = db.mifs_topic.Where(wr => wr.hashcode == id).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qTopic);


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-topic-members")]
        public HttpResponseMessage postTopicMember(mifs_topicmembers p)
        {
            try
            {
                p.create_date = DateTime.Now.Date;
                p.accesslevel = "M";
                db.Entry(p).State = EntityState.Added;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-bulk-topic-members")]
        public HttpResponseMessage postBulkTopicMember(TopicMembers p)
        {
            try
            {

                foreach (var row in p.topicmembersdetails)
                {
                    row.create_date = DateTime.Now.Date;
                    row.accesslevel = "M";
                    row.created_by = p.created_by;
                    row.topicId = p.topicId;
                    db.Entry(row).State = EntityState.Added;
                    db.SaveChanges();
                }
         

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [ActionName("post-bulk-topic-membersv2")]
        public HttpResponseMessage postBulkTopicMemberV2(TopicMembers p)
        {
            try
            {

                foreach (var row in p.topicmembersdetails)
                {
                    row.create_date = DateTime.Now.Date;
                    row.accesslevel = "M";
                    row.created_by = p.created_by;
                    row.memberId = p.memberId;
                    db.Entry(row).State = EntityState.Added;
                    db.SaveChanges();
                }


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPut]
        [ActionName("put-topic-members")]
        public HttpResponseMessage putTopicMember(mifs_topicmembers p)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Entry(p).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("delete-source-members")]
        public HttpResponseMessage deleteSourceMember(int topicId, string memberId)
        {
            try
            {

                mifs_topicmembers n = db.mifs_topicmembers.Where(wr => wr.topicId == topicId && wr.memberId == memberId).FirstOrDefault();
                db.mifs_topicmembers.Remove(n);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-member-not-selected")]
        public HttpResponseMessage getMemberNotSelected(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<string> memberId = new List<string>();

                var sourceMember = db.mifs_vTopicMembers.Where(wr => wr.topicId == id).Select(aa => new { aa.memberId }).ToList();

                foreach (var row in sourceMember)
                    memberId.Add(row.memberId);

                var query = db.mifs_members.Where(wr => !memberId.Contains(wr.memberId) && wr.status == "A").ToList();

               return Request.CreateResponse(HttpStatusCode.OK, query);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-topic-not-selected")]
        public HttpResponseMessage getTopicNotSelected(string memberId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                List<int> topicId = new List<int>();

                var sourceTopic = db.mifs_vTopicMembers.Where(wr => wr.memberId == memberId).Select(aa => new { aa.topicId }).ToList();

                foreach (var row in sourceTopic)
                    topicId.Add(row.topicId);

                var query = db.mifs_topic.Where(wr => !topicId.Contains(wr.topicId) && wr.status == "A")
                    .Select(aa => new {
                        aa.topicId,
                        aa.description,
                        branchname = db.core_branchmst.Where(mm=>mm.brancode == aa.branchcode).Select(mm=>mm.branname).FirstOrDefault(),
                        districtname = db.core_district.Where(mm => mm.districtcode == aa.districtcode).Select(mm => mm.districtname).FirstOrDefault(),
                    }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, query);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        #endregion

        #region "Issues"

        [HttpPost]
        public HttpResponseMessage BuzzIssue(object param)
        {
            JObject jResult;
            try
            {

                jResult = JObject.Parse(param.ToString());
                int issueId = (int)jResult["issueId"];
                string email = jResult["email"].ToString();

                var qIssue = db.mifs_vIssues.Where(m => m.issueId == issueId).FirstOrDefault();

                if (email == qIssue.responemail)
                {
                    try
                    {
                        //When you are on responsible tab - The system will send sms to the tollgater
                        SMS.SendSms(qIssue.raisedbyphone, "You're attention is needed on Issue#" + issueId.ToString() + "-" + qIssue.Details + " under Topic " + qIssue.TopicAlias, "PaloUP", null);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid Recepient cellphone number!");
                    }


                }
                else if (email == qIssue.raisedbyemail)
                {
                    try
                    {
                        //When you are on tollgater tab - The system will send sms to the responsible
                        SMS.SendSms(qIssue.responphone, "You're attention is needed on Issue#" + issueId.ToString() + "-" + qIssue.Details + " under Topic " + qIssue.TopicAlias, "PaloUP", null);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid Recepient cellphone number!");
                    }
                }
                else if (email == qIssue.collaboratoremail)
                {
                    try
                    {
                        //When you are on collaborator - The system will send sms to both responsible and tollgater
                        SMS.SendSms(qIssue.responphone, "You're attention is needed on Issue#" + issueId.ToString() + "-" + qIssue.Details + " under Topic " + qIssue.TopicAlias, "PaloUP", null);
                        SMS.SendSms(qIssue.raisedbyphone, "You're attention is needed on Issue#" + issueId.ToString() + "-" + qIssue.Details + " under Topic " + qIssue.TopicAlias, "PaloUP", null);
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid Recepient cellphone number!");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// get all issue
        /// </summary>
        /// <param name="pg">page number</param>
        /// <param name="tk">number of record per page</param>
        /// <param name="fnd">search specific item</param>
        /// <returns>List of Issue</returns>
        [HttpGet]
        public HttpResponseMessage GetAllIssue(int pg, int tk, string fnd)
        {
            try
            {
                int take = tk;
                int page = pg;
                int skip = 0;
                int parsedFnd;

                string find = fnd == null ? "" : fnd;

                var dynamicList = new List<dynamic>();
                PaginationViewModel model = new PaginationViewModel();

                List<mifs_vIssues> IssueList = new List<mifs_vIssues>();
                bool result = Int32.TryParse(find, out parsedFnd);

                if (page != 1)
                {
                    skip = (page - 1) * take;
                }

                if (result)
                    IssueList = db.mifs_vIssues.Where(m => m.issueId == parsedFnd || find.Contains(m.actionplan) || find.Contains(m.Details))
                    .OrderByDescending(m => m.CreateDate).Skip(skip).Take(take).ToList();

                else
                    IssueList = db.mifs_vIssues.Where(m => find.Contains(m.actionplan) || find.Contains(m.Details))
                   .OrderByDescending(m => m.CreateDate).Skip(skip).Take(take).ToList();

                dynamicList = IssueList.Cast<dynamic>().ToList();
                model.data = dynamicList;
                model.total_count = dynamicList.Count();

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage getIssue(int pg, int tk, string fnd)
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                int take = tk;
                int page = pg;
                int skip = 0;
                int parsedFnd;

                string find = fnd == null ? "" : fnd;

                var dynamicList = new List<dynamic>();
                PaginationViewModel model = new PaginationViewModel();

                List<mifs_issues> IssueList = new List<mifs_issues>();
                bool result = Int32.TryParse(find, out parsedFnd);

                if (page != 1)
                {
                    skip = (page - 1) * take;
                }

                //check find param is not null
                if (!string.IsNullOrEmpty(find))

                    //if not null then find query will execute
                    IssueList = db.mifs_issues.Where(m => (m.status != "C" || m.status != "D") && m.issueId == parsedFnd || find.Contains(m.actionplan) || find.Contains(m.issuedetails))
                    .OrderByDescending(m => m.create_date).Skip(skip).Take(take).ToList();

                else
                    //find equal to  null select all issue where status is not equal to close or drop
                    IssueList = db.mifs_issues.Where(m => (m.status != "C" || m.status != "D"))
                   .OrderByDescending(m => m.create_date).Skip(skip).Take(take).ToList();

                dynamicList = IssueList.Cast<dynamic>().ToList();
                model.data = dynamicList;
                model.total_count = db.mifs_issues.Where(m => (m.status != "C" || m.status != "D")).Count();

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }





        [ActionName("post-bulk-transfer-raisedby")]
        [HttpPost]
        public HttpResponseMessage PostBulkTransferRaisedBy(List<BulkIssueForTransfer> bulkissuefortransferraisedby)
        {
            MifsIssues n = new MifsIssues();
            string[] param = new string[20];
            string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;
            string fromStatus, toStatus;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

             
                foreach (var i in bulkissuefortransferraisedby)
                {
                    var empinfo = db.mifs_members.Where(mm => mm.memberId == i.transferraisedbyempId).FirstOrDefault();
                    var checkempstatus = db.core_employee.Where(mm => mm.empl_id == empinfo.memberId && mm.type != "SEP").Select(ss => ss.status).Count();
                    if (checkempstatus > 0)
                    {
                        foreach (var j in i.issuemodel)
                        {

                            var issue = db.mifs_issues.Where(mm => mm.issueId == j.issueId).FirstOrDefault();
                            issue.raisedby = empinfo.fullname;
                            issue.raisedbyemail = empinfo.emailaddress;
                            issue.raisedbyempid = empinfo.memberId;
                            issue.raisedbyphone = empinfo.mobileno;

                            //commit Changes
                            db.SaveChanges();

                            var iresult = db.Sproc_mifsSetResponse(0, j.issueId, i.transferbyemployeename + "  Transfer RaisedBy/Tollgater from " + j.raisedby + " to " + empinfo.fullname + " requested by " + i.requestorname + " with the Ticket No : # " + i.ticketno
                                          , i.transferbyemployeename, i.transferbyemployeeId, DateTime.Now, i.transferbyemployeeemail).First().Value;

                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Cant Transfer Issue, The Employee is Already Seperated");

                    }


                }



                //var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                //fromStatus = n.MifsStatus(qIssue.status);
                //toStatus = n.MifsStatus(qIssue.newstatus);

                //if (qIssue.acknowledgedbysource == 0)
                //{
                //    recipientMail = qIssue.responemail;
                //    recipientName = qIssue.responsible;
                //    senderName = qIssue.raisedby;

                //    if (reason == "undefined" || reason == null || reason == "")
                //    {

                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                          , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                //    }


                //}
                //else
                //{
                //    recipientMail = qIssue.raisedbyemail;
                //    recipientName = qIssue.raisedby;
                //    senderName = qIssue.responsible;
                //    if (reason == "undefined" || reason == null || reason == "")
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                //   , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                //    }

                //}

                //string newStatus = qIssue.newstatus;
                //qIssue.acknowledgebyrespondent = 0;
                //qIssue.acknowledgedbysource = 0;
                //qIssue.status = newStatus;
                //qIssue.newstatus = string.Empty;

                ////set close date if the status is Close or Drop
                //if (newStatus == "C" || newStatus == "D")
                //    qIssue.closedate = DateTime.Now;

                ////commit Changes
                //db.SaveChanges();
                //if (reason == "undefined" || reason == null || reason == "")
                //{
                //DataTable dt = new DataTable();
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //sb.AppendLine("<html>");
                //sb.AppendLine("<body>");
                //sb.AppendLine("<table class=" + "blueTable" + ">" +
                //"<thead>" +

                //"<tr>" +
                //"<th>No.</th >" +
                //"<th>Issue Id</th>" +
                //"<th>Details</th>" +
                //"<th>Action Plan</th>" +
                //"<th>Raised By / Tollgater</th>" +
                //"<th>Responsible</th>" +
                //"<th>Issue Date</th>" +
                //"</tr>" +
                //"</thead>");


                //foreach (DataRow dr in bulkissuefordrop)

                //{
                //    sb.Append("<tr>");
                //    foreach (DataColumn dc in dt.Columns)
                //    {
                //        string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                //        sb.AppendFormat("<td>{0}</ td>", cellValue);
                //    }
                //    sb.AppendLine("</tr>");
                //}
                //sb.AppendLine("</table>");

                //sb.ToString();




                //Begin: sent notification
                param[0] = bulkissuefortransferraisedby.FirstOrDefault().approvername;
                param[1] = bulkissuefortransferraisedby.FirstOrDefault().requestorname;
                param[2] = bulkissuefortransferraisedby.FirstOrDefault().ticketno.ToString();
                param[3] = GetHtmlTableTransfer(bulkissuefortransferraisedby);
                param[4] = bulkissuefortransferraisedby.FirstOrDefault().transferbyemployeename;
                mailParams.recipientsMail = bulkissuefortransferraisedby.FirstOrDefault().requestoremail;
                mailParams.ccMail =  bulkissuefortransferraisedby.FirstOrDefault().transferbyemployeeemail;
                mailParams.mailTemplatePath = "mailTemplate//mifs.bulk-transfer-raisedby-issue.html";
                mailParams.sendername = bulkissuefortransferraisedby.FirstOrDefault().transferbyemployeename;
                mailParams.subject = "[paloUp] Transfer Issues";
                mailParams.param = param;
                mailParams.createdby = "paloUp";
                procedure.EmailNotification(mailParams);
                //}
                //else
                //{
                //    param[0] = recipientName;
                //    param[1] = senderName;
                //    param[2] = fromStatus;
                //    param[3] = toStatus;
                //    param[4] = qIssue.issueId.ToString();
                //    param[5] = qIssue.issuedetails;
                //    param[6] = qIssue.actionplan;
                //    param[7] = reason;
                //    mailParams.recipientsMail = recipientMail;
                //    mailParams.ccMail = qIssue.raisedbyemail;
                //    mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                //    mailParams.sendername = senderName;
                //    mailParams.subject = "[paloUp] Acknowledge Issue";
                //    mailParams.param = param;
                //    mailParams.createdby = "paloUp";
                //    procedure.EmailNotification(mailParams);
                //}




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        [ActionName("post-bulk-transfer-responsible")]
        [HttpPost]
        public HttpResponseMessage PostBulkTransferResponsible(List<BulkIssueForTransfer> bulkissuefortransferresponsible)
        {
            MifsIssues n = new MifsIssues();
            string[] param = new string[20];
            string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;
            string fromStatus, toStatus;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                foreach (var i in bulkissuefortransferresponsible)
                {
                    var empinfo = db.mifs_members.Where(mm => mm.memberId == i.transferresponsibleempId).FirstOrDefault();
                    var checkempstatus = db.core_employee.Where(mm => mm.empl_id == empinfo.memberId && mm.type != "SEP").Select(ss => ss.status).Count();
                    if (checkempstatus > 0)
                    {
                        foreach (var j in i.issuemodel)
                        {
                           
                            var issue = db.mifs_issues.Where(mm => mm.issueId == j.issueId).FirstOrDefault();
                            issue.responsible = empinfo.fullname;
                            issue.responemail = empinfo.emailaddress;
                            issue.responempid = empinfo.memberId;
                            issue.responphone = empinfo.mobileno;

                            //commit Changes
                            db.SaveChanges();

                            var iresult = db.Sproc_mifsSetResponse(0, j.issueId, i.transferbyemployeename + "  Transfer Responsible from " + j.responsible + " to " + empinfo.fullname + " requested by " + i.requestorname + " with the Ticket No : # " + i.ticketno
                                            , i.transferbyemployeename, i.transferbyemployeeId, DateTime.Now, i.transferbyemployeeemail).First().Value;

                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Cant Transfer Issue, The Employee is Already Seperated");

                    }

                  

                }



                //var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                //fromStatus = n.MifsStatus(qIssue.status);
                //toStatus = n.MifsStatus(qIssue.newstatus);

                //if (qIssue.acknowledgedbysource == 0)
                //{
                //    recipientMail = qIssue.responemail;
                //    recipientName = qIssue.responsible;
                //    senderName = qIssue.raisedby;

                //    if (reason == "undefined" || reason == null || reason == "")
                //    {

                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                          , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                //    }


                //}
                //else
                //{
                //    recipientMail = qIssue.raisedbyemail;
                //    recipientName = qIssue.raisedby;
                //    senderName = qIssue.responsible;
                //    if (reason == "undefined" || reason == null || reason == "")
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                //   , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                //    }

                //}

                //string newStatus = qIssue.newstatus;
                //qIssue.acknowledgebyrespondent = 0;
                //qIssue.acknowledgedbysource = 0;
                //qIssue.status = newStatus;
                //qIssue.newstatus = string.Empty;

                ////set close date if the status is Close or Drop
                //if (newStatus == "C" || newStatus == "D")
                //    qIssue.closedate = DateTime.Now;

                ////commit Changes
                //db.SaveChanges();
                //if (reason == "undefined" || reason == null || reason == "")
                //{
                //DataTable dt = new DataTable();
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //sb.AppendLine("<html>");
                //sb.AppendLine("<body>");
                //sb.AppendLine("<table class=" + "blueTable" + ">" +
                //"<thead>" +

                //"<tr>" +
                //"<th>No.</th >" +
                //"<th>Issue Id</th>" +
                //"<th>Details</th>" +
                //"<th>Action Plan</th>" +
                //"<th>Raised By / Tollgater</th>" +
                //"<th>Responsible</th>" +
                //"<th>Issue Date</th>" +
                //"</tr>" +
                //"</thead>");


                //foreach (DataRow dr in bulkissuefordrop)

                //{
                //    sb.Append("<tr>");
                //    foreach (DataColumn dc in dt.Columns)
                //    {
                //        string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                //        sb.AppendFormat("<td>{0}</ td>", cellValue);
                //    }
                //    sb.AppendLine("</tr>");
                //}
                //sb.AppendLine("</table>");

                //sb.ToString();




                //Begin: sent notification
                param[0] = bulkissuefortransferresponsible.FirstOrDefault().approvername;
                param[1] = bulkissuefortransferresponsible.FirstOrDefault().requestorname;
                param[2] = bulkissuefortransferresponsible.FirstOrDefault().ticketno.ToString();
                param[3] = GetHtmlTableTransfer(bulkissuefortransferresponsible);
                param[4] = bulkissuefortransferresponsible.FirstOrDefault().transferbyemployeename;
                mailParams.recipientsMail = bulkissuefortransferresponsible.FirstOrDefault().requestoremail;
                mailParams.ccMail = bulkissuefortransferresponsible.FirstOrDefault().transferbyemployeeemail;
                mailParams.mailTemplatePath = "mailTemplate//mifs.bulk-transfer-raisedby-issue.html";
                mailParams.sendername = bulkissuefortransferresponsible.FirstOrDefault().transferbyemployeename;
                mailParams.subject = "[paloUp] Transfer Issues";
                mailParams.param = param;
                mailParams.createdby = "paloUp";
                procedure.EmailNotification(mailParams);
                //}
                //else
                //{
                //    param[0] = recipientName;
                //    param[1] = senderName;
                //    param[2] = fromStatus;
                //    param[3] = toStatus;
                //    param[4] = qIssue.issueId.ToString();
                //    param[5] = qIssue.issuedetails;
                //    param[6] = qIssue.actionplan;
                //    param[7] = reason;
                //    mailParams.recipientsMail = recipientMail;
                //    mailParams.ccMail = qIssue.raisedbyemail;
                //    mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                //    mailParams.sendername = senderName;
                //    mailParams.subject = "[paloUp] Acknowledge Issue";
                //    mailParams.param = param;
                //    mailParams.createdby = "paloUp";
                //    procedure.EmailNotification(mailParams);
                //}




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [ActionName("post-bulk-transfer-topic")]
        [HttpPost]
        public HttpResponseMessage PostBulkTransferTopic(List<BulkIssueForTransfer> bulkissuefortransfertopic)
        {
            MifsIssues n = new MifsIssues();
            string[] param = new string[20];
            string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;
            string fromStatus, toStatus;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                foreach (var i in bulkissuefortransfertopic)
                {
                   
                        foreach (var j in i.issuemodel)
                        {

                            var issue = db.mifs_issues.Where(mm => mm.issueId == j.issueId).FirstOrDefault();
                            issue.fromtopicId = issue.topicId;
                            issue.topicId = i.transferbytopicId;

                            var origtopicdetails = db.mifs_topic.Where(mm => mm.topicId == issue.fromtopicId).FirstOrDefault();
                            var transfertopicdetails = db.mifs_topic.Where(mm => mm.topicId == issue.topicId).FirstOrDefault();
                            //commit Changes
                            db.SaveChanges();

                            var iresult = db.Sproc_mifsSetResponse(0, j.issueId, i.transferbyemployeename + "  Transfer Topic from " + origtopicdetails.description + " to " + transfertopicdetails.description + " requested by " + i.requestorname + " with the Ticket No : # " + i.ticketno
                                            , i.transferbyemployeename, i.transferbyemployeeId, DateTime.Now, i.transferbyemployeeemail).First().Value;

                        }
                



                }



                //var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                //fromStatus = n.MifsStatus(qIssue.status);
                //toStatus = n.MifsStatus(qIssue.newstatus);

                //if (qIssue.acknowledgedbysource == 0)
                //{
                //    recipientMail = qIssue.responemail;
                //    recipientName = qIssue.responsible;
                //    senderName = qIssue.raisedby;

                //    if (reason == "undefined" || reason == null || reason == "")
                //    {

                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                          , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                //    }


                //}
                //else
                //{
                //    recipientMail = qIssue.raisedbyemail;
                //    recipientName = qIssue.raisedby;
                //    senderName = qIssue.responsible;
                //    if (reason == "undefined" || reason == null || reason == "")
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                //   , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                //    }

                //}

                //string newStatus = qIssue.newstatus;
                //qIssue.acknowledgebyrespondent = 0;
                //qIssue.acknowledgedbysource = 0;
                //qIssue.status = newStatus;
                //qIssue.newstatus = string.Empty;

                ////set close date if the status is Close or Drop
                //if (newStatus == "C" || newStatus == "D")
                //    qIssue.closedate = DateTime.Now;

                ////commit Changes
                //db.SaveChanges();
                //if (reason == "undefined" || reason == null || reason == "")
                //{
                //DataTable dt = new DataTable();
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //sb.AppendLine("<html>");
                //sb.AppendLine("<body>");
                //sb.AppendLine("<table class=" + "blueTable" + ">" +
                //"<thead>" +

                //"<tr>" +
                //"<th>No.</th >" +
                //"<th>Issue Id</th>" +
                //"<th>Details</th>" +
                //"<th>Action Plan</th>" +
                //"<th>Raised By / Tollgater</th>" +
                //"<th>Responsible</th>" +
                //"<th>Issue Date</th>" +
                //"</tr>" +
                //"</thead>");


                //foreach (DataRow dr in bulkissuefordrop)

                //{
                //    sb.Append("<tr>");
                //    foreach (DataColumn dc in dt.Columns)
                //    {
                //        string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                //        sb.AppendFormat("<td>{0}</ td>", cellValue);
                //    }
                //    sb.AppendLine("</tr>");
                //}
                //sb.AppendLine("</table>");

                //sb.ToString();




                //Begin: sent notification
                param[0] = bulkissuefortransfertopic.FirstOrDefault().approvername;
                param[1] = bulkissuefortransfertopic.FirstOrDefault().requestorname;
                param[2] = bulkissuefortransfertopic.FirstOrDefault().ticketno.ToString();
                param[3] = GetHtmlTableTransfer(bulkissuefortransfertopic);
                param[4] = bulkissuefortransfertopic.FirstOrDefault().transferbyemployeename;
                mailParams.recipientsMail = bulkissuefortransfertopic.FirstOrDefault().requestoremail;
                mailParams.ccMail = bulkissuefortransfertopic.FirstOrDefault().transferbyemployeeemail;
                mailParams.mailTemplatePath = "mailTemplate//mifs.bulk-transfer-raisedby-issue.html";
                mailParams.sendername = bulkissuefortransfertopic.FirstOrDefault().transferbyemployeename;
                mailParams.subject = "[paloUp] Transfer Issues";
                mailParams.param = param;
                mailParams.createdby = "paloUp";
                procedure.EmailNotification(mailParams);
                //}
                //else
                //{
                //    param[0] = recipientName;
                //    param[1] = senderName;
                //    param[2] = fromStatus;
                //    param[3] = toStatus;
                //    param[4] = qIssue.issueId.ToString();
                //    param[5] = qIssue.issuedetails;
                //    param[6] = qIssue.actionplan;
                //    param[7] = reason;
                //    mailParams.recipientsMail = recipientMail;
                //    mailParams.ccMail = qIssue.raisedbyemail;
                //    mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                //    mailParams.sendername = senderName;
                //    mailParams.subject = "[paloUp] Acknowledge Issue";
                //    mailParams.param = param;
                //    mailParams.createdby = "paloUp";
                //    procedure.EmailNotification(mailParams);
                //}




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPut]
        [ActionName("put-issue-transfer-topic")]
        public HttpResponseMessage putIssueTransferTopic(mifs_issues p)
        {
            try
            {

                var qIssue = db.mifs_issues.Where(wr => wr.issueId == p.issueId).FirstOrDefault();
                int icurrentSource = qIssue.topicId;
                qIssue.topicId = p.topicId;
                qIssue.fromtopicId = icurrentSource;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("put-issue-transfer-to-person")]
        public HttpResponseMessage putIssueTransferToPerson(dynamic p)
        {
            string[] param = new string[20];//declare email parameter holder
            string aliesCode = string.Empty;
            JObject jResult = null;
            string jsonString = p.ToString();
            try
            {

                jResult = JObject.Parse(jsonString);
                int issueId = (int)jResult["issueId"];
                int transfertype = (int)jResult["transfertype"];
                string transferbyname = jResult["transferbyname"].ToString();
                string transferbyemail = jResult["transferbyemail"].ToString();
                string transferbyempid = jResult["transferbyempid"].ToString();
                var qIssue = db.mifs_issues.Where(wr => wr.issueId == issueId).FirstOrDefault();

                qIssue.topicId = (int)jResult["topicId"];

                if (transfertype == 1)
                {


                    //transfer owner
                    string formerraisedby = qIssue.raisedby;
                    qIssue.raisedby = jResult["name"].ToString();
                    qIssue.raisedbyempid = jResult["empid"].ToString();
                    qIssue.raisedbyemail = jResult["email"].ToString();
                    qIssue.raisedbyphone = jResult["phone"].ToString();
                    var checkempstatus = db.core_employee.Where(mm => mm.empl_id == qIssue.raisedbyempid && mm.type != "SEP").Select(ss => ss.status).Count();

                    if (checkempstatus > 0)
                    {
                        db.SaveChanges();//commit changes


                        var iresult = db.Sproc_mifsSetResponse(0, qIssue.issueId, transferbyname + "  Transfer RaisedBy/Tollgater from " + formerraisedby + " to " + qIssue.raisedby
                                          , transferbyname, transferbyempid, DateTime.Now, transferbyemail).First().Value;

                        aliesCode = db.mifs_topic.Where(wr => wr.topicId == qIssue.topicId).FirstOrDefault().aliascode;

                        param[0] = qIssue.raisedby;//responsible
                        param[1] = qIssue.issueId.ToString();
                        param[2] = aliesCode;//alias code
                        param[3] = qIssue.issueId.ToString();// issue ID
                        param[4] = qIssue.issuedetails;// issue details
                        param[5] = qIssue.actionplan;//action plan
                        param[6] = qIssue.hashcode;//issue hashcode
                        param[7] = qIssue.raisedbyemail;//responsible email
                        param[8] = qIssue.raisedby;//responsible name
                        param[9] = qIssue.raisedbyempid;//responsible id if employee

                        //set mailparam set value
                        mailParams.recipientsMail = qIssue.raisedbyemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.issue.transfer.notification.html";
                        mailParams.sendername = qIssue.raisedby;
                        mailParams.subject = "MIFS Issue Transfer";
                        mailParams.param = param;
                        mailParams.createdby = "MIFS Sys";
                        procedure.EmailNotification(mailParams);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Cant Transfer Issue, The Employee is Already Seperated");
                    }


                }
                else
                {
                    //transfer responsible
                    string formerresponsible = qIssue.responsible;
                    qIssue.responsible = jResult["name"].ToString();
                    qIssue.responempid = jResult["empid"].ToString();
                    qIssue.responemail = jResult["email"].ToString();
                    qIssue.responphone = jResult["phone"].ToString();
                    var checkempstatus = db.core_employee.Where(mm => mm.empl_id == qIssue.responempid && mm.type != "SEP").Select(ss => ss.status).Count();
                    if (checkempstatus > 0)
                    {
                        db.SaveChanges();//commit changes

                        var iresult = db.Sproc_mifsSetResponse(0, qIssue.issueId, transferbyname + "  Transfer Responsible from " + formerresponsible + " to " + qIssue.responsible
                                      , transferbyname, transferbyempid, DateTime.Now, transferbyemail).First().Value;

                        aliesCode = db.mifs_topic.Where(wr => wr.topicId == qIssue.topicId).FirstOrDefault().aliascode;

                        param[0] = qIssue.responsible;//responsible
                        param[1] = qIssue.issueId.ToString();
                        param[2] = aliesCode;//alias code
                        param[3] = qIssue.issueId.ToString();// issue ID
                        param[4] = qIssue.issuedetails;// issue details
                        param[5] = qIssue.actionplan;//action plan
                        param[6] = qIssue.hashcode;//issue hashcode
                        param[7] = qIssue.responemail;//responsible email
                        param[8] = qIssue.responsible;//responsible name
                        param[9] = qIssue.responempid;//responsible id if employee

                        //set mailparam set value
                        mailParams.recipientsMail = qIssue.responemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.issue.transfer.notification.html";
                        mailParams.sendername = qIssue.raisedby;
                        mailParams.subject = "MIFS Issue Transfer";
                        mailParams.param = param;
                        mailParams.createdby = "MIFS Sys";
                        procedure.EmailNotification(mailParams);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Cant Transfer Issue, The Employee is Already Seperated");

                    }

                }




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("put-issue-park")]
        public IHttpActionResult putIssuePark(mifs_issues p)
        {
            try
            {
                MifsIssues n = new MifsIssues();
                n.putIssueStatus(p);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }






        [HttpPost]
        [ActionName("post-issues")]
        public HttpResponseMessage postIssues(mifs_issues p)
        {
            try
            {
                DateTime defaultDate = new DateTime(0001, 01, 01, 00, 00, 00);

                string emailRecipient = string.Empty;
                string recipentName = string.Empty;
                string senderName = string.Empty;
                string sourceType = string.Empty;
                string responsibleId = string.Empty;
                string colaborationMail = string.Empty;
                string recipentHascode = string.Empty;
                int topicId = 0;
                decimal? issueId = 0;
                string hashcode = string.Empty;
                string[] param = new string[20];



                hashcode = DateTime.Now.Ticks.ToString();
                hashcode = phpLib.generatePassword(hashcode).ToString();
                //insert mode


                p.newstatus = string.Empty;
                p.acknowledgedbysource = 0;
                p.acknowledgebyrespondent = 0;
                p.create_date = DateTime.Now;
                p.hashcode = hashcode;
                p.fupcounter = 0;
                p.statchangefupcounter = 0;

                db.mifs_issues.Add(p);
                db.SaveChanges();

                //Begin:set email notification
                issueId = p.issueId;
                var currentIssue = db.mifs_issues.Where(wr => wr.issueId == issueId).FirstOrDefault();

                topicId = p.topicId;
                emailRecipient = currentIssue.responemail;
                recipentName = currentIssue.responsible;
                senderName = currentIssue.raisedby;
                responsibleId = currentIssue.responempid;
                sourceType = db.mifs_topic.Where(wr => wr.topicId == topicId).FirstOrDefault().description;
                colaborationMail = string.IsNullOrEmpty(currentIssue.collaboratoremail) ? "" : currentIssue.collaboratoremail;

                param[0] = recipentName;
                param[1] = senderName;
                param[2] = sourceType;
                param[3] = p.topicId.ToString();
                param[4] = issueId.ToString();
                param[5] = p.issuedetails;
                param[6] = p.actionplan;
                param[7] = p.issuedate.ToShortDateString();
                param[8] = p.targetcompletiondate.ToShortDateString();
                param[9] = currentIssue.hashcode;
                param[10] = emailRecipient;
                param[11] = recipentName;
                param[12] = responsibleId;
                //set mail Parameter value

                mailParams.mailTemplatePath = "mailTemplate//mifs.new.issue.report.html";
                mailParams.recipientsMail = emailRecipient;
                mailParams.sendername = "Paloap System";
                mailParams.subject = "New Issue [Ref#: " + p.issueId.ToString() + "]";
                mailParams.param = param;
                mailParams.createdby = "Paloup";
                procedure.EmailNotification(mailParams);

                //End:Email Notification

                issueId = currentIssue.issueId;

                return Request.CreateResponse(HttpStatusCode.OK, issueId);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }


        }

        public HttpResponseMessage GetIssueInformation(string hashCode)
        {
            try
            {
                mifs_vIssues model = new mifs_vIssues();
                var IssueList = db.mifs_vIssues.Where(m => m.IssueHashcode == hashCode).ToList();
                if (IssueList.Count > 0)
                {
                    model = IssueList.First();
                }
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetIssueResponses(int IssueId)
        {
            try
            {
                var responseList = db.mifs_vResponse.Where(m => m.issueId == IssueId).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, responseList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [ActionName("delete-issues")]
        public HttpResponseMessage deleteIssues(int id)
        {
            try
            {
                var qResponse = db.mifs_response.Where(m => m.issueId == id).ToList();

                //list of all responseId 
                List<int> response = qResponse.Select(m => m.responseId).ToList();

                //select issue in attachment table
                var qIssueWithAttachment = db.core_attachments.Where(m => m.transactionId == id && m.attachtype == "I");
                if (qIssueWithAttachment.Count() > 0) // check if the issue has attachment
                    db.core_attachments.Remove(qIssueWithAttachment.FirstOrDefault());//delete issue attachment

                //select response in attachment table
                var qAttachment = db.core_attachments.Where(m => response.Contains(m.transactionId) && m.attachtype == "R");
                if (qAttachment.Count() > 0)// check if the response has attachment
                    db.core_attachments.RemoveRange(qAttachment);//remove response attachment


                if (qResponse.Count() > 0)//check if the specific issue has reponse if have then    
                    db.mifs_response.RemoveRange(qResponse);//delete all reponse on this specific issue

                //select specific issue
                var qissue = db.mifs_issues.Where(m => m.issueId == id).FirstOrDefault();
                db.mifs_issues.Remove(qissue);//remove specific issue


                db.SaveChanges();//commit all changes
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("get-issues-benchmarking")]
        public HttpResponseMessage postIssueBenchmarking(MifsIsssueBenchmark issuesearch)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                //  var issueList = db.mifs_vIssues.Where(mm => mm.Details.Contains(p) || mm.actionplan.Contains(p)).OrderByDescending(mm=>mm.CreateDate).ToList();

                if(issuesearch.issuestatus == "NC")
                {
                    var issueList = db.Database
                                     .SqlQuery<mifs_vIssues>("Select * from mifs_vIssues where Details LIKE '%" + issuesearch.issuedetails + "%'" + "or actionplan LIKE '%" + issuesearch.issuedetails + "%'");



                    return Request.CreateResponse(HttpStatusCode.OK, issueList.OrderByDescending(mm => mm.CreateDate));

                }
                else
                {
                    var issueList = db.Database
                               .SqlQuery<mifs_vIssues>("Select * from mifs_vIssues where (Details LIKE '%" + issuesearch.issuedetails + "%'" + "or actionplan LIKE '%" + issuesearch.issuedetails + "%') and IssueStatus = 'C'");



                    return Request.CreateResponse(HttpStatusCode.OK, issueList.OrderByDescending(mm => mm.CreateDate));

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }


        }





        [HttpPost]
        [ActionName("post-issues-meeting")]
        public HttpResponseMessage postIssuesMeeting(List<IssueMeetingModel> arraymodel)
        {
            try
            {
                DateTime defaultDate = new DateTime(0001, 01, 01, 00, 00, 00);

                string emailRecipient = string.Empty;
                string recipentName = string.Empty;
                string senderName = string.Empty;
                string sourceType = string.Empty;
                string issuesId = string.Empty;
                string recipentHascode = string.Empty;
                int topicId = 0;
                int meetingId = 0;
                decimal? issueId = 0;
                string[] param = new string[20];
                string hashcode = string.Empty;



                foreach (IssueMeetingModel p in arraymodel)
                {

                    if (db.mifs_issues.Where(wr => wr.issueId == p.issueId).Count() > 0)
                    {
                        //modify script here
                        var iresult = db.Sproc_mifsSetIssues(p.issueId,
                            p.hashcode,
                            p.topicId,
                     p.issuedetails,
                     p.actionplan,
                     p.issuedate,
                     p.fromtopicId,
                     p.targetcompletiondate,
                     p.raisedby,
                     p.raisedbyemail,
                     p.raisedbyempid,
                     p.raisedbyphone,
                     p.responsible,
                     p.responemail,
                     p.responempid,
                     p.responphone,
                     p.followfrequency,
                     p.followmail,
                     p.followsms,
                     p.status,
                     p.newstatus,
                     p.acknowledgedbysource,
                     p.acknowledgesourcedate,
                     p.acknowledgebyrespondent,
                     p.acknowledgerespondentdate,
                     p.fupcounter,
                     p.parkdate,
                     p.resumedate,
                     p.lastsenddate,
                     p.statchangefupcounter,
                     p.meetingId,
                     p.reference,
                     p.sourcetype,
                     p.created_by,
                     p.create_date,
                     p.collaboratorname,
                     p.collaboratoremail,
                     p.pflag,
                     p.issuetype,
                     p.issuecategory,
                     p.branchcode,
                     p.districtcode,
                     p.quadrant,
                     p.fromissueid,
                     p.subkpi,
                     p.reviewlevel);

                     issueId = p.issueId;
                    }
                    else
                    {
                        //insert new item here
                        hashcode = DateTime.Now.ToString("MM/dd/yy H:mm:ss.fff tt").ToString();
                        hashcode = phpLib.generatePassword(hashcode).ToString();
                        if (p.hashcodeIssueId == null && p.hashcodeMeetingId == null)
                        {
                            topicId = p.topicId;
                            meetingId = p.meetingId;
                        }
                        else
                        {
                            topicId = db.mifs_topic.Where(wr => wr.hashcode == p.hashcodeIssueId).FirstOrDefault().topicId;
                            meetingId = db.mifs_meeting.Where(wr => wr.hashcode == p.hashcodeMeetingId).FirstOrDefault().meetingId;

                        }

                        //insert mode
                        if (p.issuedate.ToString() == "1/1/0001 12:00:00 AM")
                            p.issuedate = DateTime.Now;

                        p.newstatus = string.Empty;
                        p.acknowledgedbysource = 0;
                        p.acknowledgebyrespondent = 0;
                        p.create_date = DateTime.Now;
                        p.hashcode = hashcode;
                        p.fupcounter = 0;
                        p.statchangefupcounter = 0;
                        p.meetingId = meetingId;
                        p.issueId = 0;
                        p.topicId = topicId;

                        var iresult = db.Sproc_mifsSetIssues(p.issueId, p.hashcode, p.topicId,
                       p.issuedetails, p.actionplan, p.issuedate, p.fromtopicId,
                       p.targetcompletiondate, p.raisedby, p.raisedbyemail,
                       p.raisedbyempid, p.raisedbyphone, p.responsible, p.responemail, p.responempid, p.responphone,
                       p.followfrequency,
                       p.followmail, p.followsms, p.status, null, p.acknowledgedbysource, p.acknowledgesourcedate,
                       p.acknowledgebyrespondent, p.acknowledgerespondentdate, p.fupcounter, p.parkdate,
                       p.resumedate, p.lastsenddate, p.statchangefupcounter, p.meetingId, p.reference,
                       p.sourcetype, p.created_by, p.create_date, p.collaboratorname, p.collaboratoremail, p.pflag, p.issuetype, p.issuecategory, p.branchcode, p.districtcode, p.quadrant, p.fromissueid,p.subkpi,p.reviewlevel);



                        issueId = iresult.First().Value;

                        //   int id = db.is; // Yes it's here
                        //Begin:set email notification
                        if (p.sourcetype != "M")
                        {

                            var currentIssue = db.mifs_issues.Where(wr => wr.issueId == issueId).FirstOrDefault();

                            topicId = p.topicId;
                            emailRecipient = currentIssue.responemail;
                            recipentName = currentIssue.responsible;
                            senderName = currentIssue.raisedby;
                            sourceType = db.mifs_topic.Where(wr => wr.topicId == topicId).FirstOrDefault().description;
                            issuesId = currentIssue.issueId.ToString();
                            recipentHascode = phpLib.generatePassword(currentIssue.responempid).ToString();

                            param[0] = recipentName;
                            param[1] = senderName;
                            param[2] = sourceType;
                            param[3] = p.topicId.ToString();
                            param[4] = issuesId;
                            param[5] = p.issuedetails;
                            param[6] = p.actionplan;
                            param[7] = p.issuedate.ToShortDateString();
                            param[8] = p.targetcompletiondate.ToShortDateString();
                            param[9] = currentIssue.hashcode;
                            param[10] = emailRecipient;

                            //set mail Parameter value

                            mailParams.mailTemplatePath = "mailTemplate//mifs.new.issue.report.html";
                            mailParams.recipientsMail = emailRecipient;
                            mailParams.sendername = "Paloap System";
                            mailParams.subject = "New Issue";
                            mailParams.param = param;
                            mailParams.createdby = "Paloap";
                            procedure.EmailNotification(mailParams);
                        }
                        //End:Email Notification
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, issueId);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut]
        [ActionName("put-issue")]
        public HttpResponseMessage putIssue(IssueMeetingModel arraymodel)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                CompareLogic logic = new CompareLogic();
                logic.Config.MaxDifferences = 100;
                //var origissue = db.mifs_issues.Where(mm => mm.issueId == arraymodel.issueId).FirstOrDefault();

                //ComparisonResult result = logic.Compare(origissue, arraymodel);
                //var issueId = arraymodel.FirstOrDefault().issueId;
                StringBuilder sb = new StringBuilder();
                var obj = new IssueCompareModel();
                string r = nameof(obj.raisedby);
                //IssueCompareModel cc1 = new IssueCompareModel()
                //{
                 
                //    raisedby = origissue.raisedby
                //};


                DataTable dat = new DataTable();
             
                dat.Columns.Add("Issue Details");
                dat.Columns.Add("Action Plan");
                dat.Columns.Add("Issue Date");
                dat.Columns.Add("Target Completion Date");
                dat.Columns.Add("RaisedBy/Tollgater");
                dat.Columns.Add("Responsible");
                dat.Columns.Add("Collaborator Name");
                dat.Columns.Add("Issue Type");
                dat.Columns.Add("Issue Category");
                dat.Columns.Add("KPI Tagging");


                DataTable dt2 = new DataTable();

                dt2.Columns.Add("Issue Details");
                dt2.Columns.Add("Action Plan");
                dt2.Columns.Add("Issue Date");
                dt2.Columns.Add("Target Completion Date");
                dt2.Columns.Add("RaisedBy/Tollgater");
                dt2.Columns.Add("Responsible");
                dt2.Columns.Add("Collaborator Name");
                dt2.Columns.Add("Issue Type");
                dt2.Columns.Add("Issue Category");
                dt2.Columns.Add("KPI Tagging");




                var origissue = db.mifs_issues.Where(mm => mm.issueId == arraymodel.issueId).FirstOrDefault();


              
                    DataRow dr = dat.NewRow();

                    dr["Issue Details"] = origissue.issuedetails;
                    dr["Action Plan"] = origissue.actionplan;
                    dr["Issue Date"] = origissue.issuedate;
                    dr["Target Completion Date"] = origissue.targetcompletiondate.ToString("MMM dd, yyyy");
                    dr["RaisedBy/Tollgater"] = origissue.raisedby;
                    dr["Responsible"] = origissue.responsible;
                    dr["Collaborator Name"] = origissue.collaboratorname;
                    dr["Issue Type"] = origissue.issuetype;
                    dr["Issue Category"] = db.mifs_dropdown.Where(mm => mm.value == origissue.issuecategory && mm.code == "issuecategory").FirstOrDefault().display;
                    dr["KPI Tagging"] = origissue.quadrant;

                    dat.Rows.Add(dr);
             

                var editedissueList = arraymodel;

              
                    DataRow dr2 = dt2.NewRow();

                    dr2["Issue Details"] = editedissueList.issuedetails;
                    dr2["Action Plan"] = editedissueList.actionplan;
                    dr2["Issue Date"] = editedissueList.issuedate;
                    dr2["Target Completion Date"] = editedissueList.targetcompletiondate.ToString("MMM dd, yyyy");
                    dr2["RaisedBy/Tollgater"] = editedissueList.raisedby;
                    dr2["Responsible"] = editedissueList.responsible;
                    dr2["Collaborator Name"] = editedissueList.collaboratorname;
                    dr2["Issue Type"] = editedissueList.issuetype;
                    dr2["Issue Category"] = db.mifs_dropdown.Where(mm=> mm.value == editedissueList.issuecategory && mm.code == "issuecategory").FirstOrDefault().display;
                    dr2["KPI Tagging"] = editedissueList.quadrant;




                    dt2.Rows.Add(dr2.ItemArray);



                var item = dat.AsEnumerable().Select(row=>new { 
                
                
                }).First();

                List<String> lsColumns = new List<string>();

                if (dat.Rows.Count > 0)
                {
                    var count = dat.Rows[0].Table.Columns.Count;

                    for (int i = 0; i < count; i++)
                    {
                        lsColumns.Add(Convert.ToString(dat.Rows[0][i]));
                    }
                }

                //var arrayNames = (from DataTable x
                //    in dat.Rows.Cast<DataRow>()
                //                  select x.Rows).ToArray();
                //IssueCompareModel cc2 = new IssueCompareModel()
                //{ 

                //    raisedby = arraymodel.raisedby 
                //};

                List<Difference> diffs = logic.Compare(dat, dt2).Differences;


                //string JSONresult;
                //JSONresult = JsonConvert.SerializeObject(dat);
                //string JSONresult2;
                //JSONresult2 = JsonConvert.SerializeObject(dt2);
                //List<Difference> diffs2 = logic.Compare(JSONresult, JSONresult2).Differences;

             

                foreach (Difference diff in diffs)
                {



                    var iresult = db.Sproc_mifsSetResponse(0, arraymodel.issueId, arraymodel.changebyempname + " edited/changed " + (diff.PropertyName.Trim(diff.ParentPropertyName.ToCharArray())).Replace("\"", "") + " from " + diff.Object1Value + " to " + diff.Object2Value
                                           , arraymodel.changebyempname, arraymodel.changebyempId, DateTime.Now, arraymodel.changebyempemailadd).First().Value;



                }




                var json = JsonConvert.SerializeObject(arraymodel);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                
                dictionary.Remove("changebyempname");
                dictionary.Remove("changebyempId");
                dictionary.Remove("changebyempemailadd");
                string updatedissue = JsonConvert.SerializeObject(dictionary);
                mifs_issues p = JsonConvert.DeserializeObject<mifs_issues>(updatedissue);
              
                //Get Data from Edited Value
                db.Entry(origissue).CurrentValues.SetValues(p);

               
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [ActionName("put-issues-acknowledged")]
        public dynamic putIssueAcknowledged(mifs_issues p)
        {
            try
            {
                MifsIssues n = new MifsIssues();
                n.putIssueAcknowledged(p);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }



        [HttpPut]
        [ActionName("put-change-status")]
        public dynamic putChangeStatus(MifsChangeStatusModel p)
        {
            try
            {
                MifsIssues n = new MifsIssues();
                n.putChangeStatus(p);
                var qResponse = db.mifs_vResponse
                   .Where(wr => wr.issueId == p.issueId)
                   .OrderBy(or => or.responsedate)
                   .ToList()
                   .Select(aa => new
                   {
                       aa,
                       Attachment = getAttachment(aa.responseId)
                   });
                //var qCurrentResponse = db.mifs_vResponse.Where(wr => wr.issueId == p.issueId).Select(aa => new
                //{
                //    aa,
                //    Attachment = getAttachment(aa.issueId)
                //}).ToList();

                return Json(new { hasError = false ,data = qResponse });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [ActionName("get-escalate-issue")]
        public HttpResponseMessage getIssueList(int topicId, string resEmail, string uEmail, int itype)
        {
            MifsIssues n = new MifsIssues();
            object objResult = null;
            try
            {
                switch (itype)
                {
                    case 1:
                        //get issue where responsible is userid
                        var responsibleIssue = db.mifs_vIssues
                        .Where(wr => wr.CurrentTopicId == topicId)
                        .Where(wr => wr.responemail == resEmail)
                        .OrderBy(m => m.PriorityFlag)
                        .OrderByDescending(or => or.CreateDate)
                        .ToList()
                        .Select(aa => new
                        {
                            aa,
                            canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                        });

                        objResult = responsibleIssue;
                        break;
                    case 2:
                        //get issue where tollgater is userid
                        var tollgaterIssue = db.mifs_vIssues
                             .Where(wr => wr.CurrentTopicId == topicId)
                             .Where(m => m.raisedbyemail == resEmail)
                             .OrderBy(or => or.PriorityFlag)
                             .OrderByDescending(or => or.CreateDate)
                             .ToList()
                             .Select(aa => new
                             {
                                 aa,
                                 canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                             });
                        objResult = tollgaterIssue;
                        break;
                    case 3:
                        //get issue where colaborator is userid
                        var collaboratorIssue = db.mifs_vIssues
                                .Where(wr => wr.CurrentTopicId == topicId)
                                 .Where(m => m.collaboratoremail == resEmail)
                                 .OrderBy(or => or.PriorityFlag)
                                 .OrderByDescending(or => or.CreateDate)
                                 .ToList()
                                 .Select(aa => new
                                 {
                                     aa,
                                     canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                                 });
                        objResult = collaboratorIssue;
                        break;
                }
                return Request.CreateResponse(HttpStatusCode.OK, objResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


     




        [ActionName("get-escalate-issue")]
        public HttpResponseMessage getIssueList(int topicId, string resEmail, string uEmail, int itype, int iyr)
        {
            MifsIssues n = new MifsIssues();
            object objResult = null;
            try
            {
                switch (itype)
                {
                    case 1:
                        //get issue where responsible is userid
                        var responsibleIssue = db.mifs_vIssues
                        .Where(wr => wr.CurrentTopicId == topicId && wr.DateIssued.Year== iyr)
                        .Where(wr => wr.responemail == resEmail)
                        .OrderBy(m => m.PriorityFlag)
                        .OrderByDescending(or => or.DateIssued)
                        .ToList()
                        .Select(aa => new
                        {
                            aa,
                            canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                        });

                        objResult = responsibleIssue;
                        break;
                    case 2:
                        //get issue where tollgater is userid
                        var tollgaterIssue = db.mifs_vIssues
                             .Where(wr => wr.CurrentTopicId == topicId && wr.DateIssued.Year == iyr)
                             .Where(m => m.raisedbyemail == resEmail)
                             .OrderBy(or => or.PriorityFlag)
                             .OrderByDescending(or => or.DateIssued)
                             .ToList()
                             .Select(aa => new
                             {
                                 aa,
                                 canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                             });
                        objResult = tollgaterIssue;
                        break;
                    case 3:
                        //get issue where colaborator is userid
                        var collaboratorIssue = db.mifs_vIssues
                                .Where(wr => wr.CurrentTopicId == topicId && wr.DateIssued.Year == iyr)
                                 .Where(m => m.collaboratoremail == resEmail)
                                 .OrderBy(or => or.PriorityFlag)
                                 .OrderByDescending(or => or.DateIssued)
                                 .ToList()
                                 .Select(aa => new
                                 {
                                     aa,
                                     canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, uEmail)
                                 });
                        objResult = collaboratorIssue;
                        break;
                }
                return Request.CreateResponse(HttpStatusCode.OK, objResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [ActionName("get-paloup-issue")]
        public dynamic getPaloupIssueList(int topicId, string resEmail)
        {
            try
            {
                MifsIssues n = new MifsIssues();
                return Json(new { hasError = false, data = n.getPaloupIssueList(topicId, resEmail) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [ActionName("get-escalate-all-issue")]
        public HttpResponseMessage getAllIssueList(int topicId, string email)
        {
            try
            {
                var qIssue = db.mifs_vIssues
                       .Where(wr => wr.CurrentTopicId == topicId)
                       .OrderBy(m => m.topicId)
                       .ThenBy(m => m.PriorityFlag)
                       .ToList()
                       .Select(aa => new
                       {
                           aa,
                           canResponse = isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                       });

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [ActionName("get-escalate-all-issue")]
        public HttpResponseMessage getAllIssueList(int topicId, string email, int iyr)
        {
            try
            {
                var qIssue = db.mifs_vIssues
                       .Where(wr => wr.CurrentTopicId == topicId && wr.DateIssued.Year== iyr)
                       .OrderBy(m => m.issueId)
                       .ThenBy(m => m.PriorityFlag)
                       .ToList()
                       .Select(aa => new
                       {
                           aa,
                           canResponse = isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                       });

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [ActionName("get-escalated-issue-hashcode")]
        public HttpResponseMessage getIssueList(string hascode, string resEmail, int issueId)
        {
            try
            {
                var qIssue = db.mifs_vIssues
                       .Where(wr => wr.IssueHashcode == hascode && wr.issueId == issueId)
                       .OrderByDescending(or => or.CreateDate)
                       .ToList()
                       .Select(aa => new
                       {
                           aa,
                           canResponse = isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, resEmail)
                       });

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage getIssue(string hashcode, int issueId)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var qIssue = db.mifs_issues.Where(wr => wr.hashcode == hashcode && wr.issueId == issueId).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-issue-by-meeting-id")]
        public HttpResponseMessage getIssueByMeetingId(string id, string email)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                int meetingId = db.mifs_meeting.Where(wr => wr.hashcode == id).FirstOrDefault().meetingId;

                var qIssue = db.mifs_vIssues
                    .Where(wr => wr.meetingId == meetingId)
                    .OrderByDescending(or => or.CreateDate)
                    .ToList()
                    .Select(aa => new
                    {
                        aa,
                        canResponse = isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                    });

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-meeting-details-hash")]
        public HttpResponseMessage getMeetingDtlHash(string id)
        {
            try
            {
                var qMeeting = db.mifs_meeting.Where(wr => wr.hashcode == id).Include(ss => ss.mifs_topic).AsNoTracking().ToList()
                       .Select(aa => new
                       {
                           aa.agenda,
                           aa.create_date,
                           aa.created_by,
                           aa.dateend,
                           aa.datestart,
                           aa.facilitator,
                           aa.hashcode,
                           aa.meetingId,
                           aa.notes,
                           aa.status,
                           aa.topicId,
                           aa.venue,
                           aa.mifs_topic.aliascode,
                           aa.mifs_topic.description
                       }).ToList()[0];

                return Request.CreateResponse(HttpStatusCode.OK, qMeeting);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-meeting-issue")]
        public HttpResponseMessage getMeetingIssue(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => wr.meetingId == id).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [ActionName("get-issue-by-issueId")]
        public HttpResponseMessage getIssuebyIssueId(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("get-bulkissue-by-issueId")]
        public HttpResponseMessage getBulkIssuebyIssueId(List<BulkIssue> issues)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var issueList = issues.Select(ss => ss.issueId).ToList();
                var qIssue = db.mifs_issues.Where(mm => issueList.Contains(mm.issueId) && mm.status == "O").ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [ActionName("get-issue-for-drop-by-issueId")]
        public HttpResponseMessage getIssueForDropbyIssueId(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-issue-for-drop-by-topicId")]
        public HttpResponseMessage getIssueForDropbyTopicId(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => wr.topicId == id && wr.status == "O" ).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [ActionName("get-issue-for-drop-by-employee")]
        public HttpResponseMessage getIssueForDropbyEmployee(EmployeeAccount empdetails)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => 
                ((wr.responemail == empdetails.EmpEmail || wr.responempid == empdetails.EmpID) ||
                (wr.raisedbyemail == empdetails.EmpEmail || wr.raisedbyempid == empdetails.EmpID) ||
                (wr.collaboratoremail == empdetails.EmpEmail || wr.collaboratorempid == empdetails.EmpID))
                && wr.status == "O").OrderBy(mm=>mm.issueId).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-multiple-actionplan")]
        public HttpResponseMessage getMultipleActionPlan(MifsIsssueParams p)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qIssue = db.mifs_issues.Where(wr => wr.fromissueid == p.issueId).OrderBy(mm=>mm.issueId).ToList();
                if(qIssue.Count >= 0)
                {
                    var issue = db.mifs_issues.Where(wr => wr.issueId == p.issueId).ToList();
                    return Request.CreateResponse(HttpStatusCode.OK, issue.FirstOrDefault());
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, qIssue.LastOrDefault());
                }

                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetAllIssueOpen()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var qIssue = db.mifs_issues.Where(wr => wr.status == "O" && wr.newstatus == null).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, qIssue);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        #endregion

        #region Issue Uploading

        [HttpPost]
        [ActionName("upload-issues")]
        public HttpResponseMessage UploadIssues(List<mifs_issues> arraymodel)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                db.Database.CommandTimeout = 0;
                DateTime defaultDate = new DateTime(0001, 01, 01, 00, 00, 00);

                string emailRecipient = string.Empty;
                string recipentName = string.Empty;
                string senderName = string.Empty;
                string sourceType = string.Empty;
                string responsibleId = string.Empty;
                string colaborationMail = string.Empty;
                string recipentHascode = string.Empty;
                int topicId = 0;
                decimal? issueId = 0;
                string hashcode = string.Empty;
                string[] param = new string[20];



                foreach (mifs_issues p in arraymodel)
                {
                    if (db.mifs_issues.Where(mm => mm.referenceId == p.referenceId).Count() == 0)
                    {


                        p.responphone = db.mifs_members.Where(mm => mm.memberId == p.responempid).Select(mm => mm.mobileno).FirstOrDefault();
                        p.raisedbyphone = db.mifs_members.Where(mm => mm.memberId == p.raisedbyempid).Select(mm => mm.mobileno).FirstOrDefault();
                        p.newstatus = string.Empty;
                        p.acknowledgedbysource = 0;
                        p.acknowledgebyrespondent = 0;
                        hashcode = phpLib.generatePassword(p.hashcode).ToString();
                        p.hashcode = hashcode;
                        p.fupcounter = 0;
                        p.statchangefupcounter = 0;

                        db.mifs_issues.Add(p);
                        db.SaveChanges();

                        //Begin:set email notification
                        issueId = p.issueId;
                        var currentIssue = db.mifs_issues.Where(wr => wr.issueId == issueId).FirstOrDefault();

                        topicId = p.topicId;
                        emailRecipient = currentIssue.responemail;
                        recipentName = currentIssue.responsible;
                        senderName = currentIssue.raisedby;
                        responsibleId = currentIssue.responempid;
                        sourceType = db.mifs_topic.Where(wr => wr.topicId == topicId).FirstOrDefault().description;
                        colaborationMail = string.IsNullOrEmpty(currentIssue.collaboratoremail) ? "" : currentIssue.collaboratoremail;

                        param[0] = recipentName;
                        param[1] = senderName;
                        param[2] = sourceType;
                        param[3] = p.topicId.ToString();
                        param[4] = issueId.ToString();
                        param[5] = p.issuedetails;
                        param[6] = p.actionplan;
                        param[7] = p.issuedate.ToString("yyyy-MM-dd HH:mm:ss");
                        param[8] = p.targetcompletiondate.ToString("yyyy-MM-dd HH:mm:ss");
                        param[9] = currentIssue.hashcode;
                        param[10] = emailRecipient;
                        param[11] = recipentName;
                        param[12] = responsibleId;
                        //set mail Parameter value

                        mailParams.mailTemplatePath = "mailTemplate\\mifs.new.issue.report.html";
                        mailParams.recipientsMail = emailRecipient;
                        mailParams.sendername = "Paloap System";
                        mailParams.subject = "New Issue [Ref#: " + p.issueId.ToString() + "]";
                        mailParams.param = param;
                        mailParams.createdby = "Paloup";
                        procedure.EmailNotification(mailParams);
                    }


                }

                return Request.CreateResponse(HttpStatusCode.OK, issueId);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion

        #region Branch and District Maintenance
        [HttpGet]
        [ActionName("get-all-branch")]
        public HttpResponseMessage getAllBranch ()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var branch = db.core_branchmst.Where(mm=>mm.status == "A").Select(mm => new
                {
                    mm.districtcode,
                    mm.brancode,
                    mm.branname
                }
                ).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, branch.OrderBy(mm => mm.branname));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-all-branch-per-district")]
        public HttpResponseMessage getAllBranchperDistrict(string districtcode)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var branch = db.core_branchmst.Where(mm=>mm.districtcode == districtcode && mm.status == "A").ToList();

                return Request.CreateResponse(HttpStatusCode.OK, branch.OrderBy(mm => mm.branname));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-all-district")]
        public HttpResponseMessage getAllDistrict()
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
             
                var branch = db.core_branchmst.Where(mm => mm.status == "A" ).Select(mm =>  mm.districtcode).Distinct().ToList();


                var district = db.core_district.Where(mm=> branch.Contains(mm.districtcode)).Select(mm => new
                {
                    mm.districtcode,
                    mm.districtname

                }
           ).ToList();


                return Request.CreateResponse(HttpStatusCode.OK, district.OrderBy(mm=>mm.districtname));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

      
        #endregion

        #region "Response"

        [HttpPost]
        [ActionName("files-upload")]
        public IHttpActionResult FilesUpload(MifsAttashmentModel p)
        {

            try
            {

                using (PALOUPEntities context = new PALOUPEntities())
                {
                    string filepath = string.Empty;

                    //Live
                    // string otherFile = context.core_drivepath.Where(aa => aa.id == 8).FirstOrDefault().drive_path;

                    //Local for production use only commit this before you publish this project
                    string otherFile = context.core_drivepath.Where(aa => aa.id == 9).FirstOrDefault().drive_path;

                    string fileExtension = System.IO.Path.GetExtension(p.file.FileName);
                    string fileName = p.file.FileName;
                    string newfilename = "File-" + DateTime.Now.ToString("MMddyyyyssfff");
                    var fileLocation = Path.Combine(otherFile, newfilename + fileExtension);
                    p.file.SaveAs(fileLocation);
                    filepath = fileLocation;

                    core_attachments attach = new core_attachments();

                    attach.create_date = DateTime.Now;
                    attach.created_by = p.created_by;
                    attach.transactionId = p.issueresponseId;
                    attach.filename = fileName;
                    attach.newfilename = newfilename;
                    attach.filetype = fileExtension;
                    attach.attachtype = "R";
                    attach.uploadtype = "O";
                    context.Entry(attach).State = EntityState.Added;
                    context.SaveChanges();

                }


                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });

            }

        }

        [ActionName("get-attachment")]
        public dynamic getAttachment(int id)
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var qAttachment = db.core_attachments
                      .Where(wr => wr.transactionId == id)
                      .OrderBy(or => or.create_date)
                      .ToList();

                return qAttachment;
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpPost]
        [ActionName("post-response")]
        public HttpResponseMessage postResponse(mifs_response p)
        {
            try
            {
                MifsIssues mifsIssues = new MifsIssues();

                MailParamsModel mailParam = new MailParamsModel();
                Library phpLib = new Library();
                string emailRecipient = string.Empty;
                string topic = string.Empty;
                string[] param = new string[20];

                var iresult = db.Sproc_mifsSetResponse(0, p.issueId, p.messagedetails, p.responseby, p.responsebyempid, DateTime.Now, p.responsebyemail).First().Value;

                var qCurrentResponse = db.mifs_vResponse
                    .Where(wr => wr.responseId == iresult).FirstOrDefault();

                //send notification

                var currentIssue = db.mifs_issues.Where(wr => wr.issueId == p.issueId).FirstOrDefault();

                topic = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault().aliascode;


                List<MifsMailAccountModel> recipientMail = new List<MifsMailAccountModel>();
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.responemail, name = currentIssue.responsible, empId = currentIssue.responempid });
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.raisedbyemail, name = currentIssue.raisedby, empId = currentIssue.raisedbyempid });
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.collaboratoremail, name = currentIssue.collaboratorname, empId = "" });

                foreach (var row in recipientMail)
                {
                    if (!string.IsNullOrEmpty(row.email))
                    {
                        if (row.email != p.responsebyemail)
                        {
                            //param[0] = topic;
                            //param[1] = p.responseby;
                            //param[2] = currentIssue.issuedetails;
                            //param[3] = topic;
                            //param[4] = currentIssue.issueId.ToString(); ;
                            //param[5] = currentIssue.issuedetails;
                            //param[6] = currentIssue.actionplan;
                            //param[7] = currentIssue.issuedate.ToShortDateString();
                            //param[8] = "";
                            //param[9] = currentIssue.hashcode;
                            //param[10] = row.email;
                            //param[11] = row.name;
                            //param[12] = row.empId;

                            HttpWebRequest request = null;
                            HttpWebResponse response = null;
                            // request = (HttpWebRequest)WebRequest.Create("http://coreapi.fastlogistics.com.ph/Report/Minutes/" + qMeeting.meetingId);
                            request = (HttpWebRequest)WebRequest.Create("https://coreapi.fastlogistics.com.ph/Report/MifsReponseMessage/?issueID=" + p.issueId + "&response=" + p.responseby + "&name=" + row.name + "&email=" + row.email + "&empId=" + row.empId);
                          //  request = (HttpWebRequest)WebRequest.Create("http://localhost:59293/Report/MifsReponseMessage/?issueID=" + p.issueId + "&response=" + p.responseby + "&name=" + row.name + "&email=" + row.email + "&empId=" + row.empId);

                            request.Method = "GET";

                            // get response data
                            response = (HttpWebResponse)request.GetResponse();
                            request = null;

                            string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                            response.Close();

                            // show results
                            //  Response.Write(responseData);

                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(responseData);
                            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
                            string EmailBoody = bodyNode.InnerHtml;// string.Format(bodyNode.InnerHtml, param);

                            using (PaloUpMailEntities dbcon2 = new PaloUpMailEntities())
                            {
                                    core_mailer coreEmail = new core_mailer
                                    {

                                        senderDisplayEmail = "no-reply@fastgroup.biz",
                                        senderDisplayName = "Paloup",
                                        recipient = row.email,
                                        mailSubject = "[paloUp] Issue on " + topic,
                                        mailBody = EmailBoody,
                                        mailStatus = "q",
                                        mailFormat = "HTML",
                                        createdBy = "Paloup",
                                        created = DateTime.Now.Date
                                    };
                                    dbcon2.core_mailer.Add(coreEmail);
                                    dbcon2.SaveChanges();
                
                            }

                            //mailParam.recipientsMail = row.email;
                            //// mailParam.ccMail = currentIssue.raisedbyrepresentativemail + ';' + currentIssue.responrepresentativemail;
                            //mailParam.mailTemplatePath = "mailTemplate//mifs.response.report.html";
                            //mailParam.sendername = p.responseby;
                            //mailParam.subject = "[paloUp] Issue on " + topic;
                            //mailParam.param = param;
                            //mailParam.createdby = "paloUp";
                            //procedure.EmailNotification(mailParam);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, qCurrentResponse);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("put-response")]
        public HttpResponseMessage putResponse(mifs_response p)
        {
            try
            {
                var qMifsResponse = db.mifs_response.Where(m => m.responseId == p.responseId).FirstOrDefault();

                qMifsResponse.messagedetails = p.messagedetails;

                db.SaveChanges();

                var qCurrentResponse = db.mifs_vResponse
                    .Where(wr => wr.responseId == p.responseId).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qCurrentResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [ActionName("delete-response")]
        public HttpResponseMessage deleteResponse(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                List<core_attachments> qAttachment = db.core_attachments.Where(m => m.transactionId == id).ToList();

                //remove first the attachment
                if (qAttachment.Count() > 0)
                    db.core_attachments.RemoveRange(qAttachment);

                //remove second the response
                mifs_response qResponse = db.mifs_response.Where(m => m.responseId == id).FirstOrDefault();
                db.mifs_response.Remove(qResponse);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-response-embed")]
        public dynamic postResponseEmbed(MifsResponseEmbed p)
        {
            try
            {
                MifsIssues mifsIssues = new MifsIssues();
                mifs_response mifsResponse = new mifs_response();
                clsprocedure procedure = new clsprocedure();
                MailParamsModel mailParam = new MailParamsModel();
                Library phpLib = new Library();

                string emailRecipient = string.Empty;
                string topic = string.Empty;
                string[] param = new string[20];


                PALOUPEntities db = new PALOUPEntities();
                var iresult = db.Sproc_mifsSetResponse(0, p.issueId, p.messagedetails, p.responseby, p.responsebyempid, DateTime.Now, p.responsebyemail).First().Value;

                var qCurrentResponse = db.mifs_vResponse
                .Where(wr => wr.responseId == iresult).ToList().ToList()
                        .Select(aa => new
                        {
                            aa,
                            Attachment = getAttachment(aa.responseId)
                        });

                //send notification

                var currentIssue = db.mifs_issues.Where(wr => wr.issueId == p.issueId).FirstOrDefault();

                topic = db.mifs_topic.Where(wr => wr.topicId == currentIssue.topicId).FirstOrDefault().aliascode;

                //Begin: append to attachment
                core_attachments attach = new core_attachments();

                attach.create_date = DateTime.Now;
                attach.created_by = p.responseby;
                attach.transactionId = int.Parse(iresult.ToString());
                attach.filename = p.youtubeUrl;
                attach.filetype = "youtube";
                attach.attachtype = "R";
                attach.uploadtype = "Y";
                db.Entry(attach).State = EntityState.Added;
                db.SaveChanges();
                //End: append to attachment

                List<MifsMailAccountModel> recipientMail = new List<MifsMailAccountModel>();
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.responemail, name = currentIssue.responsible, empId = currentIssue.responempid });
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.raisedbyemail, name = currentIssue.raisedby, empId = currentIssue.raisedbyempid });
                recipientMail.Add(new MifsMailAccountModel { email = currentIssue.collaboratoremail, name = currentIssue.collaboratorname, empId = "" });

                foreach (var row in recipientMail)
                {
                    if (!string.IsNullOrEmpty(row.email))
                    {
                        param[0] = topic;
                        param[1] = p.responseby;
                        param[2] = currentIssue.issuedetails;
                        param[3] = topic;
                        param[4] = currentIssue.issueId.ToString(); ;
                        param[5] = currentIssue.issuedetails;
                        param[6] = currentIssue.actionplan;
                        param[7] = p.messagedetails;
                        param[8] = DateTime.Now.Date.ToShortDateString();
                        param[9] = currentIssue.hashcode;
                        param[10] = row.email;
                        param[11] = p.youtubeUrl;
                        param[12] = row.name;
                        param[13] = row.empId;

                        mailParam.recipientsMail = row.email;
                        // mailParam.ccMail = currentIssue.raisedbyrepresentativemail + ';' + currentIssue.responrepresentativemail;
                        mailParam.mailTemplatePath = "mailTemplate//mifs.response.report.youtube.html";
                        mailParam.sendername = p.responseby;
                        mailParam.subject = "[paloUp] Issue on " + topic; ;
                        mailParam.param = param;
                        mailParam.createdby = "paloUp";
                        procedure.EmailNotification(mailParam);
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK, qCurrentResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [UserAuthorize]
        [ActionName("post-response-with-file")]
        public HttpResponseMessage postResponseWithFile(mifs_response p)
        {
            List<core_attachments> attachs = new List<core_attachments>();
            int iresponse;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var iresult = db.Sproc_mifsSetResponse(0, p.issueId,
                        p.messagedetails, p.responseby,
                        p.responsebyempid, DateTime.Now, p.responsebyemail);

                iresponse = int.Parse(iresult.First().Value.ToString());
                var fileDriveUploadTemp = db.core_drivepath.Where(aa => aa.id == 9).FirstOrDefault().drive_path;//get temporary path   
                var fileDriveDistination = db.core_drivepath.Where(aa => aa.id == 8).FirstOrDefault().drive_path;//get distination path     
                var issueInfo = db.mifs_issues.Where(m => m.issueId == p.issueId).FirstOrDefault();//get issue details
                string specificPath = fileDriveDistination + "PaloUp\\" + issueInfo.topicId.ToString() + "\\" + issueInfo.issueId + "\\";//set specific path distination
                if (!Directory.Exists(specificPath))//check if the directory is not exist
                    Directory.CreateDirectory(specificPath);//create directory


                var tempFiles = db.core_temp_attachment.Where(m => m.appId == 19 && m.userId == UserParam.userId && m.status == "A" && m.refmodule == "response").ToList();

                foreach (var row in tempFiles)
                {
                    File.Move(row.filepath + row.newfilename + row.extension, specificPath + row.newfilename + row.extension);
                    //File.Delete(specificPath + row.newfilename);
                   
                    core_attachments attach = new core_attachments();
                    attach.transactionId = iresponse;
                    attach.attachtype = "R";
                    attach.filename = row.filename + row.extension;
                    attach.newfilename = row.newfilename;
                    attach.filetype = row.extension;
                    attach.uploadtype = "O";
                    attach.created_by = row.createdby;
                    attach.create_date = row.createdate;


                    attachs.Add(attach);
                    row.status = "M";

                    
                    if (File.Exists(specificPath + row.newfilename + row.extension))
                    {
                       
                        File.Delete(specificPath + row.newfilename);
                    }

                }


                db.core_attachments.AddRange(attachs);
                db.SaveChanges();
               

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-issue-status-counter")]
        public HttpResponseMessage getIssueStatusCounter(int id)
        {
            MifsDashboard dashboardLst = new MifsDashboard();
            try
            {

                var qIssueCount = db.mifs_vIssues.Where(wr => wr.CurrentTopicId == id).ToList();

                if (qIssueCount.Count == 0)
                {
                    dashboardLst.topicId = id;
                    dashboardLst.topicDes = string.Empty;
                    dashboardLst.iOpen = 0;
                    dashboardLst.iPark = 0;
                    dashboardLst.iClose = 0;
                    dashboardLst.iDrop = 0;
                }
                else
                {
                    dashboardLst.topicId = id;
                    dashboardLst.topicDes = qIssueCount.First().CurrentTopicDescription;
                    dashboardLst.iOpen = qIssueCount.Where(wr => wr.IssueStatus == "O").Count();
                    dashboardLst.iPark = qIssueCount.Where(wr => wr.IssueStatus == "P").Count();
                    dashboardLst.iClose = qIssueCount.Where(wr => wr.IssueStatus == "C").Count();
                    dashboardLst.iDrop = qIssueCount.Where(wr => wr.IssueStatus == "D").Count();
                }

                return Request.CreateResponse(HttpStatusCode.OK, dashboardLst);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-issue-status-counter")]
        public HttpResponseMessage getIssueStatusCounter(int id,int iyr)
        {
            MifsDashboard dashboardLst = new MifsDashboard();
            try
            {

                var qIssueCount = db.mifs_vIssues.Where(wr => wr.CurrentTopicId == id && wr.CreateDate.Year==iyr).ToList();

                if (qIssueCount.Count == 0)
                {
                    dashboardLst.topicId = id;
                    dashboardLst.topicDes = string.Empty;
                    dashboardLst.iOpen = 0;
                    dashboardLst.iPark = 0;
                    dashboardLst.iClose = 0;
                    dashboardLst.iDrop = 0;
                }
                else
                {
                    dashboardLst.topicId = id;
                    dashboardLst.topicDes = qIssueCount.First().CurrentTopicDescription;
                    dashboardLst.iOpen = qIssueCount.Where(wr => wr.IssueStatus == "O").Count();
                    dashboardLst.iPark = qIssueCount.Where(wr => wr.IssueStatus == "P").Count();
                    dashboardLst.iClose = qIssueCount.Where(wr => wr.IssueStatus == "C").Count();
                    dashboardLst.iDrop = qIssueCount.Where(wr => wr.IssueStatus == "D").Count();
                }

                return Request.CreateResponse(HttpStatusCode.OK, dashboardLst);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-my-topic-start-structured")]
        public HttpResponseMessage getMyTopicStartStruct(string memberId, int userId)
        {
            List<TopicModel> topicModel = new List<TopicModel>();


            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var userProfile = db.core_users.Where(m => m.userId == userId).FirstOrDefault();

                //select all topic if the user role is equal to admin
                if (userProfile.userrole.ToUpper() == "ADMIN")
                {
                    var qSourceMember = db.mifs_topic
                        .Where(m => m.status == "A" && m.isvisible == 1)
                          .OrderBy(or => or.description).ToList()
                          .Select(m => new
                          {
                              m.aliascode,
                              m.topicId,
                              m.description,
                              m.hashcode,
                              m.occurence,
                              m.bgcolor,
                              countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == m.topicId).Count(),
                              countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == m.topicId).Count(),
                              countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == m.topicId).Count(),
                              countMeeting = db.mifs_meeting.Where(w => w.topicId == m.topicId && w.status == "C").Count()
                          });

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicAlias = row.aliascode,
                            TopicDescription = row.description,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = row.countOpen,
                            countClose = row.countClose,
                            countPark = row.countPark,
                            countMeeting = row.countMeeting
                        });
                    }

                }
                else
                {
                    //select specific topic
                    var qSourceMember = from ss in db.mifs_topicmembers
                            where ss.memberId == memberId
                            join ff in db.mifs_topic on ss.topicId equals ff.topicId
                            select new
                            {
                                ff.aliascode,
                                ff.topicId,
                                TopicDescription = ff.description,
                                ff.hashcode,
                                ff.occurence,
                                ff.status,
                                ff.bgcolor,
                                ff.isvisible,
                                countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == ff.topicId).Count(),
                                countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == ff.topicId).Count(),
                                countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == ff.topicId).Count(),
                                countMeeting = db.mifs_meeting.Where(w => w.topicId == ff.topicId && w.status == "C").Count()
                            };

                    //var qSourceMember = db.mifs_vTopicMembers
                    //       .Where(wr => wr.memberId == memberId)
                    //       .OrderBy(or => or.TopicDescription).ToList()
                    //       .Select(m => new
                    //       {
                    //           m.topicId,
                    //           m.TopicDescription,
                    //           m.hashcode,
                    //           m.occurence,
                    //           m.
                    //           m.bgcolor,
                    //           countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == m.topicId).Count(),
                    //           countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == m.topicId).Count(),
                    //           countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == m.topicId).Count(),
                    //           countMeeting = db.mifs_meeting.Where(w => w.topicId == m.topicId && w.status == "C").Count()
                    //       });

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember.Where(mm=>mm.isvisible == 1 && mm.status == "A"))
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicAlias = row.aliascode,
                            TopicDescription = row.TopicDescription,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = row.countOpen,
                            countClose = row.countClose,
                            countPark = row.countPark,
                            countMeeting = row.countMeeting
                        });
                    }

                    //get topicid equal to 0
                    var qTopicPaloin = db.mifs_topic.Where(m => m.topicId == 0).ToList();

                    //fill topic paloin in the source
                    foreach (var row in qTopicPaloin)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicDescription = row.description,
                            TopicAlias = row.aliascode,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == row.topicId).Count(),
                            countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == row.topicId).Count(),
                            countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == row.topicId).Count(),
                            countMeeting = db.mifs_meeting.Where(w => w.topicId == row.topicId && w.status == "C").Count()
                        });
                    }

                }


                return Request.CreateResponse(HttpStatusCode.OK, topicModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [ActionName("get-my-topic")]
        public HttpResponseMessage getMyTopic(string memberId, int userId)
        {
            List<TopicModel> topicModel = new List<TopicModel>();


            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var userProfile = db.core_users.Where(m => m.userId == userId).FirstOrDefault();

                //select all topic if the user role is equal to admin
                if (userProfile.userrole.ToUpper() == "ADMIN")
                {
                    var qSourceMember = db.mifs_topic
                        .Where(m => m.status == "A")
                          .OrderBy(or => or.description).ToList()
                          .Select(m => new
                          {
                              m.aliascode,
                              m.topicId,
                              m.description,
                              m.hashcode,
                              m.occurence,
                              m.bgcolor,
                              countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == m.topicId).Count(),
                              countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == m.topicId).Count(),
                              countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == m.topicId).Count(),
                              countMeeting = db.mifs_meeting.Where(w => w.topicId == m.topicId && w.status == "C").Count()
                          });

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicDescription = row.description,
                            TopicAlias = row.aliascode,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = row.countOpen,
                            countClose = row.countClose,
                            countPark = row.countPark,
                            countMeeting = row.countMeeting
                        }); ;
                    }

                }
                else
                {
                    //select specific topic
                    var qSourceMember = db.mifs_vTopicMembers
                           .Where(wr => wr.memberId == memberId)
                           .OrderBy(or => or.TopicDescription).ToList()
                           .Select(m => new
                           {
                               aliascode = db.mifs_topic.Where(mm => mm.topicId == m.topicId).Select(ss => ss.aliascode).FirstOrDefault(),
                               m.topicId,
                               m.TopicDescription,
                               m.hashcode,
                               m.occurence,
                               m.bgcolor,
                               countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == m.topicId).Count(),
                               countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == m.topicId).Count(),
                               countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == m.topicId).Count(),
                               countMeeting = db.mifs_meeting.Where(w => w.topicId == m.topicId && w.status == "C").Count()
                           });

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicDescription = row.TopicDescription,
                            TopicAlias = row.aliascode,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = row.countOpen,
                            countClose = row.countClose,
                            countPark = row.countPark,
                            countMeeting = row.countMeeting
                        });
                    }

                    //get topicid equal to 0
                    var qTopicPaloin = db.mifs_topic.Where(m => m.topicId == 0).ToList();

                    //fill topic paloin in the source
                    foreach (var row in qTopicPaloin)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicDescription = row.description,
                            TopicAlias = row.aliascode,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                            countOpen = db.mifs_vIssues.Where(w => w.IssueStatus == "O" && w.CurrentTopicId == row.topicId).Count(),
                            countClose = db.mifs_vIssues.Where(w => w.IssueStatus == "C" && w.CurrentTopicId == row.topicId).Count(),
                            countPark = db.mifs_vIssues.Where(w => w.IssueStatus == "P" && w.CurrentTopicId == row.topicId).Count(),
                            countMeeting = db.mifs_meeting.Where(w => w.topicId == row.topicId && w.status == "C").Count()
                        });
                    }

                }


                return Request.CreateResponse(HttpStatusCode.OK, topicModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [ActionName("get-my-topic-nocount")]//Version 2 get-my-topic --No Counting of OPEN,CLOSE,PARK,DROP ISSUES
        public HttpResponseMessage getMyTopicNoCount(string memberId, int userId)
        {
            List<TopicModel> topicModel = new List<TopicModel>();


            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var userProfile = db.core_users.Where(m => m.userId == userId).FirstOrDefault();

                //select all topic if the user role is equal to admin
                if (userProfile.userrole.ToUpper() == "ADMIN")
                {
                    var qSourceMember = db.mifs_topic
                        .Where(m => m.status == "A")
                          .OrderBy(or => or.description).ToList()
                          .Select(m => new
                          {
                              m.aliascode,
                              m.topicId,
                              m.description,
                              m.hashcode,
                              m.occurence,
                              m.bgcolor
                            
                          });

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember)
                    {
                        topicModel.Add(new TopicModel
                        {
                            topicId = row.topicId,
                            TopicDescription = row.description,
                            TopicAlias = row.aliascode,

                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor,
                         
                        });
                    }

                }
                else
                {
                    //select specific topic
                    var qSourceMember = db.mifs_vTopicMembers
                           .Where(wr => wr.memberId == memberId)
                           .OrderBy(or => or.TopicDescription).ToList()
                           .Select(m => new
                           {
                               aliascode = db.mifs_topic.Where(mm=>mm.topicId == m.topicId).Select(ss=>ss.aliascode).FirstOrDefault(),
                               m.topicId,
                               m.TopicDescription,
                               m.hashcode,
                               m.occurence,
                               m.bgcolor
                           });;

                    //fill topic source where user has this topic
                    foreach (var row in qSourceMember)
                    {
                        topicModel.Add(new TopicModel
                        {
                            TopicAlias = row.aliascode,
                            topicId = row.topicId,
                            TopicDescription = row.TopicDescription,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor
                          
                        });
                    }

                    //get topicid equal to 0
                    var qTopicPaloin = db.mifs_topic.Where(m => m.topicId == 0).ToList();

                    //fill topic paloin in the source
                    foreach (var row in qTopicPaloin)
                    {
                        topicModel.Add(new TopicModel
                        {
                            TopicAlias = row.aliascode,
                            topicId = row.topicId,
                            TopicDescription = row.description,
                            hashcode = row.hashcode,
                            occurence = row.occurence,
                            bgcolor = row.bgcolor
                        });
                    }

                }


                return Request.CreateResponse(HttpStatusCode.OK, topicModel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [ActionName("get-response")]
        public HttpResponseMessage getResponse(int id)
        {
            try
            {
                var qResponse = db.mifs_vResponse
                      .Where(wr => wr.issueId == id)
                      .OrderBy(or => or.responsedate)
                      .ToList()
                      .Select(aa => new
                      {
                          aa,
                          Attachment = getAttachment(aa.responseId)
                      });

                return Request.CreateResponse(HttpStatusCode.OK, qResponse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [ActionName("post-bulk-drop")]
        [HttpPost]
        public HttpResponseMessage PostBulkDropIssue(List<BulkIssueForDrop> bulkissuefordrop)
        {
            MifsIssues n = new MifsIssues();
            string[] param = new string[20];
            string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;
            string fromStatus, toStatus;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;


                foreach (var i in bulkissuefordrop)
                {
                    foreach (var j in i.issuemodel)
                    {
                     
                        var issue = db.mifs_issues.Where(mm => mm.issueId == j.issueId).FirstOrDefault();
                        issue.status = "D";
                        issue.newstatus = String.Empty;
                        issue.closedate = DateTime.Now;

                        //commit Changes
                        db.SaveChanges();

                        var iresult = db.Sproc_mifsSetResponse(0, j.issueId, i.approvername + " agreed to change status from " + n.MifsStatus(j.status) + " to " + "DROP" + " requested by " + i.requestorname + " with the Ticket No : # " + i.ticketno
                                      , i.dropbyemployeename, i.dropbyemployeeId, DateTime.Now, i.dropbyemployeeemail).First().Value;

                    }

                }



                //var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                //fromStatus = n.MifsStatus(qIssue.status);
                //toStatus = n.MifsStatus(qIssue.newstatus);

                //if (qIssue.acknowledgedbysource == 0)
                //{
                //    recipientMail = qIssue.responemail;
                //    recipientName = qIssue.responsible;
                //    senderName = qIssue.raisedby;

                //    if (reason == "undefined" || reason == null || reason == "")
                //    {

                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                          , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                //    }


                //}
                //else
                //{
                //    recipientMail = qIssue.raisedbyemail;
                //    recipientName = qIssue.raisedby;
                //    senderName = qIssue.responsible;
                //    if (reason == "undefined" || reason == null || reason == "")
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                //   , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                //    }
                //    else
                //    {
                //        var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                //                     , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                //    }

                //}

                //string newStatus = qIssue.newstatus;
                //qIssue.acknowledgebyrespondent = 0;
                //qIssue.acknowledgedbysource = 0;
                //qIssue.status = newStatus;
                //qIssue.newstatus = string.Empty;

                ////set close date if the status is Close or Drop
                //if (newStatus == "C" || newStatus == "D")
                //    qIssue.closedate = DateTime.Now;

                ////commit Changes
                //db.SaveChanges();
                //if (reason == "undefined" || reason == null || reason == "")
                //{
                //DataTable dt = new DataTable();
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //sb.AppendLine("<html>");
                //sb.AppendLine("<body>");
                //sb.AppendLine("<table class=" + "blueTable" + ">" +
                //"<thead>" +

                //"<tr>" +
                //"<th>No.</th >" +
                //"<th>Issue Id</th>" +
                //"<th>Details</th>" +
                //"<th>Action Plan</th>" +
                //"<th>Raised By / Tollgater</th>" +
                //"<th>Responsible</th>" +
                //"<th>Issue Date</th>" +
                //"</tr>" +
                //"</thead>");


                //foreach (DataRow dr in bulkissuefordrop)

                //{
                //    sb.Append("<tr>");
                //    foreach (DataColumn dc in dt.Columns)
                //    {
                //        string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                //        sb.AppendFormat("<td>{0}</ td>", cellValue);
                //    }
                //    sb.AppendLine("</tr>");
                //}
                //sb.AppendLine("</table>");

                //sb.ToString();




                //Begin: sent notification
                param[0] = bulkissuefordrop.FirstOrDefault().approvername;
                param[1] = bulkissuefordrop.FirstOrDefault().requestorname;
                param[2] = bulkissuefordrop.FirstOrDefault().ticketno.ToString();
                param[3] = GetHtmlTable(bulkissuefordrop);
                param[4] =  bulkissuefordrop.FirstOrDefault().dropbyemployeename;
                mailParams.recipientsMail = bulkissuefordrop.FirstOrDefault().requestoremail;
                mailParams.ccMail = bulkissuefordrop.FirstOrDefault().approveremail + "," + bulkissuefordrop.FirstOrDefault().dropbyemployeeemail;
                mailParams.mailTemplatePath = "mailTemplate//mifs.bulk-drop-issue.html";
                mailParams.sendername = bulkissuefordrop.FirstOrDefault().dropbyemployeename;
                mailParams.subject = "[paloUp] Force Drop Issues";
                mailParams.param = param;
                mailParams.createdby = "paloUp";
                procedure.EmailNotification(mailParams);
                //}
                //else
                //{
                //    param[0] = recipientName;
                //    param[1] = senderName;
                //    param[2] = fromStatus;
                //    param[3] = toStatus;
                //    param[4] = qIssue.issueId.ToString();
                //    param[5] = qIssue.issuedetails;
                //    param[6] = qIssue.actionplan;
                //    param[7] = reason;
                //    mailParams.recipientsMail = recipientMail;
                //    mailParams.ccMail = qIssue.raisedbyemail;
                //    mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                //    mailParams.sendername = senderName;
                //    mailParams.subject = "[paloUp] Acknowledge Issue";
                //    mailParams.param = param;
                //    mailParams.createdby = "paloUp";
                //    procedure.EmailNotification(mailParams);
                //}




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }










        [HttpGet]
        public HttpResponseMessage Acknowledge(int id, string reason)
        {
            MifsIssues n = new MifsIssues();
            string[] param = new string[20];
            string recipientMail = string.Empty, recipientName = string.Empty, senderName = string.Empty;
            string fromStatus, toStatus;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                fromStatus = n.MifsStatus(qIssue.status);
                toStatus = n.MifsStatus(qIssue.newstatus);


                if(toStatus != "")
                {
                    if (qIssue.acknowledgedbysource == 0)
                    {
                        recipientMail = qIssue.responemail;
                        recipientName = qIssue.responsible;
                        senderName = qIssue.raisedby;

                        if (reason == "undefined" || reason == null || reason == "")
                        {
                            var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                                               , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                        }
                        else
                        {
                            var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.raisedby + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                                              , qIssue.raisedby, qIssue.raisedbyempid, DateTime.Now, qIssue.raisedbyemail).First().Value;

                        }


                    }
                    else
                    {
                        recipientMail = qIssue.raisedbyemail;
                        recipientName = qIssue.raisedby;
                        senderName = qIssue.responsible;
                        if (reason == "undefined" || reason == null || reason == "")
                        {
                            var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + ""
                       , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                        }
                        else
                        {
                            var iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " agreed to change status from " + fromStatus + " to " + toStatus + " with the following reason:" + reason
                                         , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                        }

                    }
                    var qCurrentResponse = db.mifs_vResponse.Where(wr => wr.issueId == qIssue.issueId).ToList().Select(aa => new
                    {
                        aa,
                        Attachment = getAttachment(aa.responseId)
                    }).ToList();
                    string newStatus = qIssue.newstatus;
                    qIssue.acknowledgebyrespondent = 0;
                    qIssue.acknowledgedbysource = 0;
                    qIssue.status = newStatus;
                    qIssue.newstatus = string.Empty;

                    //set close date if the status is Close or Drop
                    if (newStatus == "C" || newStatus == "D")
                        qIssue.closedate = DateTime.Now;

                    //commit Changes
                    db.SaveChanges();
                    if (reason == "undefined" || reason == null || reason == "")
                    {


                        //Begin: sent notification
                        param[0] = recipientName;
                        param[1] = senderName;
                        param[2] = fromStatus;
                        param[3] = toStatus;
                        param[4] = qIssue.issueId.ToString();
                        param[5] = qIssue.issuedetails;
                        param[6] = qIssue.actionplan;
                        param[7] = ""; //Notes
                        param[8] = "agree";
                        mailParams.recipientsMail = recipientMail;
                        // mailParams.ccMail = currentIssue.raisedbyrepresentativemail + ';' + currentIssue.responrepresentativemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                        mailParams.sendername = senderName;
                        mailParams.subject = "[paloUp] Acknowledge Issue";
                        mailParams.param = param;
                        mailParams.createdby = "paloUp";
                        procedure.EmailNotification(mailParams);
                    }
                    else
                    {
                        param[0] = recipientName;
                        param[1] = senderName;
                        param[2] = fromStatus;
                        param[3] = toStatus;
                        param[4] = qIssue.issueId.ToString();
                        param[5] = qIssue.issuedetails;
                        param[6] = qIssue.actionplan;
                        param[7] = reason;
                        param[8] = "agree";
                        mailParams.recipientsMail = recipientMail;
                        mailParams.ccMail = qIssue.raisedbyemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                        mailParams.sendername = senderName;
                        mailParams.subject = "[paloUp] Acknowledge Issue";
                        mailParams.param = param;
                        mailParams.createdby = "paloUp";
                        procedure.EmailNotification(mailParams);
                    }


                    return Request.CreateResponse(HttpStatusCode.OK, qCurrentResponse);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Please Refresh!! Issue is Already Updated Status to "+ fromStatus);
                }
            


                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public dynamic AcknowledgeHascode(string hashcode, int issueId)
        {
            try
            {
                MifsResponse n = new MifsResponse();
                n.acknowledgeHascode(hashcode, issueId);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }


        [HttpGet]
        public dynamic desagreeHascode(string id)
        {
            try
            {
                MifsResponse n = new MifsResponse();
                n.desagreeHascode(id);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [ActionName("set-defer")]
        public dynamic setDefer(int id, string reason)
        {
            try
            {
                MifsIssues n = new MifsIssues();
                return Json(new { hasError = false, data = n.setDefer(id, reason) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        [ActionName("cancel-acknowledgement")]
        public HttpResponseMessage cancelAcknowledgement(int id, string reason)
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

                db.Configuration.LazyLoadingEnabled = false;

                var qIssue = db.mifs_issues.Where(wr => wr.issueId == id).FirstOrDefault();

                fromStatus = n.MifsStatus(qIssue.status);
                toStatus = n.MifsStatus(qIssue.newstatus);

                if (toStatus != "")
                {
                    strSender = qIssue.responsible;
                    strRecever = qIssue.raisedby;
                    

                  //  iresult = db.Sproc_mifsSetResponse(0, id, reason, qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;
                    if (reason == "undefined" || reason == null || reason == "")
                    {
                        iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " cancel the request to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + ""
                                                             , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;


                    }
                    else
                    {
                        iresult = db.Sproc_mifsSetResponse(0, id, qIssue.responsible + " cancel the request to change status from " + fromStatus + " to " + toStatus + " with the following reason: " + reason
                                                             , qIssue.responsible, qIssue.responempid, DateTime.Now, qIssue.responemail).First().Value;

                    }

                    var qResponse = db.mifs_vResponse
                                  .Where(wr => wr.issueId == qIssue.issueId)
                                  .OrderBy(or => or.responsedate)
                                  .ToList()
                                  .Select(aa => new
                                  {
                                      aa,
                                      Attachment = getAttachment(aa.responseId)
                                  });
                    qIssue.newstatus = string.Empty;
                    qIssue.acknowledgedbysource = 0;
                    qIssue.acknowledgebyrespondent = 0;

                    db.SaveChanges();

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
                        param[8] = "cancel";
                        mailParams.recipientsMail = qIssue.raisedbyemail;
                        mailParams.ccMail = qIssue.responemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                        mailParams.sendername = strSender;
                        mailParams.subject = "[paloUp] Cancel Request Issue Status Change";
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
                        param[8] = "cancel";
                        mailParams.recipientsMail = qIssue.raisedbyemail;
                        mailParams.ccMail = qIssue.responemail;
                        mailParams.mailTemplatePath = "mailTemplate//mifs.acknowledge.notification.html";
                        mailParams.sendername = strSender;
                        mailParams.subject = "[paloUp] Cancel Request Issue Status Change";
                        mailParams.param = param;
                        mailParams.createdby = "paloUp";
                        procedure.EmailNotification(mailParams);
                    }



                    return Request.CreateResponse(HttpStatusCode.OK, qResponse);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Please Refresh!! Issue Status is Already Cancelled");
                }

              
            }
            catch (Exception ex)
            {
                //return Json(new { hasError = true, errorMessage = ex.Message });
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
  
        [HttpPost]
        public HttpResponseMessage sendToEmail(object param)
        {
            // string[] param = new string[20];
            JObject jResult = null;
            string jsonString = param.ToString();
            try
            {

                //  int responseId,string recipientEmail,string otherMsg
                jResult = JObject.Parse(jsonString);
                int responseId = (int)jResult["responseId"];
                string recipientEmail = jResult["recipientEmail"].ToString();
                string otherMsg = jResult["otherMsg"].ToString();
                string replyEmail = jResult["replyEmail"].ToString();

                var qresponse = db.mifs_response.Where(m => m.responseId == responseId).FirstOrDefault();

                MailParamsModel mailParam = new MailParamsModel();
                //param[0] = topic;
                //param[1] = p.responseby;
                //param[2] = currentIssue.issuedetails;
                //param[3] = topic;
                //param[4] = currentIssue.issueId.ToString(); ;
                //param[5] = currentIssue.issuedetails;
                //param[6] = currentIssue.actionplan;
                //param[7] = p.messagedetails;
                //param[8] = DateTime.Now.Date.ToShortDateString();
                //param[9] = currentIssue.hashcode;
                //param[10] = row.email;
                //param[11] = row.name;
                //param[12] = row.empId;

                //mailParam.recipientsMail = row.email;
                //// mailParam.ccMail = currentIssue.raisedbyrepresentativemail + ';' + currentIssue.responrepresentativemail;
                //mailParam.mailTemplatePath = "mailTemplate//mifs.response.report.html";
                //mailParam.sendername = p.responseby;
                //mailParam.subject = "[paloUp] Issue on " + topic;
                //mailParam.param = param;
                //mailParam.createdby = "paloUp";
                //procedure.EmailNotification(mailParam);
                return Request.CreateResponse(HttpStatusCode.OK, "Message has been successfully forwarded ");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region "Report"

        [HttpGet]
        [ActionName("get-issue-report")]
        public dynamic getIssueReport(string status, string userEmail)
        {
            try
            {
                MifsReport n = new MifsReport();
                return Json(new { hasError = false, data = n.getMifsIssue(status, userEmail) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }


        }

        [HttpGet]
        [ActionName("get-issue-report-overdue")]
        public dynamic getIssueReportOverdue(int pg, int tk, string userEmail)
        {
            try
            {
                MifsReport n = new MifsReport();
                return Json(new { hasError = false, data = n.getMifsIssueOverdue(pg, tk, userEmail) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }


        }

        /// <summary>
        /// Paloup User Dashboard
        /// </summary>
        /// <param name="id">paloup email</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("get-my-dashboard")]
        public dynamic getMyDashboard(string id)
        {
            try
            {
                MifsReport n = new MifsReport();
                return Json(new { hasError = false, data = n.getMyDashboard(id) });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region "Meeting"

        [HttpPost]
        public HttpResponseMessage postMeeting(mifs_meeting p)
        {
            string[] param = new string[20];
            try
            {

                db.Configuration.LazyLoadingEnabled = false;
                string hashcode = string.Empty;
                hashcode = DateTime.Now.Ticks.ToString();
                hashcode = phpLib.generatePassword(hashcode).ToString();

                p.hashcode = phpLib.generatePassword(hashcode).ToString();


                p.datestart = DateTime.Now;

                var meetingId = db.Sproc_mifsSetMeeting(p.meetingId, p.hashcode, p.topicId, p.agenda, p.datestart, p.dateend, p.venue, p.facilitator, p.created_by, DateTime.Now, p.status, p.scheduledatestart, p.scheduledateend).First().Value;
                var qMeeting = db.mifs_meeting.Where(wr => wr.meetingId == meetingId).FirstOrDefault();


                var topicMembers = db.mifs_vTopicMembers.Where(m => m.topicId == qMeeting.topicId).ToList();

                foreach (var rwMember in topicMembers)
                {

                    //add to meeting attendance
                    db.Sproc_mifsSetAttendance(0, (int)meetingId, rwMember.memberId,
                   rwMember.MemberName, rwMember.MemberEmailAddress,
                   rwMember.MemberMobileNumber,
                   1, 0, p.created_by, DateTime.Now);

                    param[0] = qMeeting.datestart.ToString("dddd, dd MMMM yyyy HH:mm");
                    param[1] = qMeeting.venue;
                    param[2] = qMeeting.facilitator;
                    param[3] = qMeeting.agenda;
                    param[4] = meetingId.ToString();
                    param[5] = rwMember.MemberEmailAddress;
                    //set mail Parameter value

                    mailParams.mailTemplatePath = "mailTemplate//mifs.meeting.schedule.html";
                    mailParams.recipientsMail = rwMember.MemberEmailAddress;
                    mailParams.sendername = "Paloap System";
                    mailParams.subject = "Schedule Meeting Ref#: " + meetingId.ToString();
                    mailParams.param = param;
                    mailParams.createdby = "Paloup";
                    procedure.EmailNotification(mailParams);

                }






                return Request.CreateResponse(HttpStatusCode.OK, qMeeting);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage postMeetingToFPMA(mifs_meeting p)
        {
            string[] param = new string[20];
            try
            {

                db.Configuration.LazyLoadingEnabled = false;
               

                var topicMembers = db.mifs_vTopicMembers.Where(m => m.topicId == p.topicId).ToList();
                
                foreach (var rwMember in topicMembers)
                {
                    var empbranch = db.fpma_member.Where(mm => mm.EmpId == rwMember.memberId).Select(mm=>mm.sitecode).FirstOrDefault();
                   
                    if (empbranch != null)
                    {


                        db.Sproc_FPMADailyTaskFunction(0, rwMember.memberId, 0, p.agenda, p.scheduledatestart, DateTime.Now, DateTime.Now, 0, 0, "", "", p.scheduledateend, empbranch.ToString(), rwMember.memberId, "VSC", null, 1);

                    }
                    else {

                        db.Sproc_FPMADailyTaskFunction(0, rwMember.memberId, 0, p.agenda, p.scheduledatestart, DateTime.Now, DateTime.Now, 0, 0, "", "", p.scheduledateend, null,rwMember.memberId, "VSC", null, 1);

                    }
                }

                  // db.Sproc_FPMADailyTaskFunction(fpma_task.Id, fpma_task.EmployeeId, tasktodayID, fpma_task.Task, fpma_task.PlannedDate, DateTime.Now, DateTime.Now, fpma_task.projectId, fpma_task.activityId, fpma_task.status, fpma_task.remarks, fpma_task.plannedDateEnd, fpma_task.sitecode, fpma_task.isImpediments, 1)





                return Request.CreateResponse(HttpStatusCode.OK, topicMembers);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        
        [HttpGet]
        public HttpResponseMessage finishMeeting(string id)
        {
            try
            {
                var qMeeting = db.mifs_meeting.Where(wr => wr.hashcode == id).FirstOrDefault();
                var qAttendees = db.mifs_attendance.Where(wr => wr.meetingId == qMeeting.meetingId).ToList();

                qMeeting.status = "C";
                qMeeting.dateend = DateTime.Now;
                db.SaveChanges();

                HttpWebRequest request = null;
                HttpWebResponse response = null;
                request = (HttpWebRequest)WebRequest.Create("https://coreapi.fastlogistics.com.ph/Report/Minutes/" + qMeeting.meetingId);
                request.Method = "GET";


                // get response data
                response = (HttpWebResponse)request.GetResponse();
                request = null;

                string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();

                // show results
                //  Response.Write(responseData);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseData);

                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
                //string examineee = p.sex == "MALE" ? "Mr." : "Ms.";
                //examineee += p.firstname + " " + p.lastname;

                string EmailBoody = bodyNode.InnerHtml;
                using (PaloUpMailEntities dbcon2 = new PaloUpMailEntities())
                {
                    foreach (var item in qAttendees)
                    {
                        core_mailer coreEmail = new core_mailer
                        {

                            senderDisplayEmail = "no-reply@fastgroup.biz",
                            senderDisplayName = "Paloup",
                            recipient = item.attendeemail,
                            mailSubject = "Paloup",
                            mailBody = EmailBoody,
                            mailStatus = "q",
                            mailFormat = "HTML",
                            createdBy = "Paloup",
                            created = DateTime.Now.Date
                        };
                        dbcon2.core_mailer.Add(coreEmail);
                        dbcon2.SaveChanges();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage resendFinishMeeting(object param)
        {

            JObject jsonObj = null;

            try
            {
                jsonObj = JObject.Parse(param.ToString());
                string id = jsonObj["hashcode"].ToString();
                var attendeesMailList = jsonObj["attendeesMailList"].ToList();


                var qMeeting = db.mifs_meeting.Where(wr => wr.hashcode == id).FirstOrDefault();

                qMeeting.status = "C";
                qMeeting.dateend = DateTime.Now;
                db.SaveChanges();

                HttpWebRequest request = null;
                HttpWebResponse response = null;

                request = (HttpWebRequest)WebRequest.Create("https://coreapi.fastlogistics.com.ph/Report/Minutes/" + qMeeting.meetingId);

                request.Method = "GET";


                // get response data
                response = (HttpWebResponse)request.GetResponse();
                request = null;

                string responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();

                // show results
                //  Response.Write(responseData);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseData);

                HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");
                //string examineee = p.sex == "MALE" ? "Mr." : "Ms.";
                //examineee += p.firstname + " " + p.lastname;

                string EmailBoody = bodyNode.InnerHtml;
                using (PaloUpMailEntities dbcon2 = new PaloUpMailEntities())
                {
                    foreach (var item in attendeesMailList)
                    {
                        core_mailer coreEmail = new core_mailer
                        {

                            senderDisplayEmail = "no-reply@fastgroup.biz",
                            senderDisplayName = "Paloup",
                            recipient = item.ToString(),
                            mailSubject = "Paloup",
                            mailBody = EmailBoody,
                            mailStatus = "q",
                            mailFormat = "HTML",
                            createdBy = "Paloup",
                            created = DateTime.Now.Date
                        };
                        dbcon2.core_mailer.Add(coreEmail);
                        dbcon2.SaveChanges();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage putMeeting(mifs_meeting p)
        {
            try
            {
                db.Sproc_mifsSetMeeting(p.meetingId, p.hashcode, p.topicId, p.agenda, p.datestart, p.dateend, p.venue, p.facilitator, p.created_by, DateTime.Now, p.status,p.scheduledatestart,p.scheduledateend);

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage getMeeting(string memberId, string status)
        {
            try
            {
                TransList transList = new TransList();
                List<int> topicIdLst = new List<int>();
              //  int skip = tk * (pg - 1); // skip the record


                db.Configuration.LazyLoadingEnabled = false;

                var qMyTopic = db.mifs_vTopicMembers.Where(wr => wr.memberId == memberId).ToList();

                topicIdLst.Add(0);
                foreach (var row in qMyTopic)
                {
                    topicIdLst.Add(row.topicId);
                }

                var qMyTopicDtl = db.mifs_topic.Where(m => topicIdLst.Contains(m.topicId)).ToList();


                var qMeeting = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
                .Where(wr => wr.status == status)
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
                     aa.hashcode,
                     topicHashcode = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().hashcode,
                     topicDesc = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().description,
                     openIssue = countOpenMeetingIssue(aa.meetingId),
                     //parkIssue =countParkMeetingIssue(aa.meetingId),
                     //closeIssue =countCloseMeetingIssue(aa.meetingId),

                 })
                 ;

                transList.total_count = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
              .Where(wr => wr.status == status)
               .Count();
                transList.data = qMeeting;


                //if (string.IsNullOrEmpty(fnd))
                //{
                //    var qMeeting = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
                //  .Where(wr => wr.status == status)
                //  .OrderBy(od => od.meetingId)
                //   .Skip(skip)
                //   .Take(tk)
                //   .ToList()
                //   .Select(aa => new
                //   {
                //       aa.meetingId,
                //       aa.topicId,
                //       aa.status,
                //       aa.datestart,
                //       aa.dateend,
                //       aa.venue,
                //       aa.facilitator,
                //       aa.agenda,
                //       aa.hashcode,
                //       topicHashcode = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().hashcode,
                //       topicDesc = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().description,
                //       openIssue = countOpenMeetingIssue(aa.meetingId),
                //       parkIssue = countParkMeetingIssue(aa.meetingId),
                //       closeIssue = countCloseMeetingIssue(aa.meetingId),

                //   })
                //   ;

                //    transList.total_count = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
                //  .Where(wr => wr.status == status)
                //   .Count();
                //    transList.data = qMeeting;
                //}
                //else
                //{
                //    var qMeeting = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
                // .Where(wr => wr.status == status)
                //   .Where(wr => wr.agenda.Contains(fnd) || wr.venue.Contains(fnd) || wr.facilitator.Contains(fnd))
                // .OrderBy(od => od.meetingId)
                //  .Skip(skip)
                //  .Take(tk)
                //  .ToList()
                //  .Select(aa => new
                //  {
                //      aa.meetingId,
                //      aa.topicId,
                //      aa.status,
                //      aa.datestart,
                //      aa.dateend,
                //      aa.venue,
                //      aa.facilitator,
                //      aa.agenda,
                //      aa.hashcode,
                //      topicHashcode = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().hashcode,
                //      topicDesc = qMyTopicDtl.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().description,
                //      openIssue = countOpenMeetingIssue(aa.meetingId),
                //      parkIssue = countParkMeetingIssue(aa.meetingId),
                //      closeIssue = countCloseMeetingIssue(aa.meetingId)
                //  });

                //    transList.total_count = db.mifs_meeting.Where(wr => topicIdLst.Contains(wr.topicId))
                // .Where(wr => wr.status == status).Where(wr => wr.agenda.Contains(fnd) || wr.venue.Contains(fnd) || wr.facilitator.Contains(fnd)).Count();
                //    transList.data = qMeeting;

                //}

                return Request.CreateResponse(HttpStatusCode.OK, transList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        public HttpResponseMessage getMeetingPerTopic(string fnd, string status, int topicid)
        {
            try
            {
                TransList transList = new TransList();
                
                db.Configuration.LazyLoadingEnabled = false;

                var qTopic = db.mifs_topic.Where(wr => wr.topicId == topicid).ToList();


                if (string.IsNullOrEmpty(fnd))
                {
                    var qMeeting = db.mifs_meeting.Where(wr => wr.topicId == topicid)
                  .Where(wr => wr.status == status)
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
                       aa.hashcode,
                       topicHashcode = qTopic.Where(wr => wr.topicId == topicid).FirstOrDefault().hashcode,
                       topicDesc = qTopic.Where(wr => wr.topicId == topicid).FirstOrDefault().description,
                       openIssue = countOpenMeetingIssue(aa.meetingId),
                       parkIssue = countParkMeetingIssue(aa.meetingId),
                       closeIssue = countCloseMeetingIssue(aa.meetingId),

                   });

                    transList.total_count = qMeeting.Count();
                    transList.data = qMeeting.OrderByDescending(mm => mm.dateend);
                }
                else
                {
                    var qMeeting = db.mifs_meeting.Where(wr => wr.topicId == topicid)
                 .Where(wr => wr.status == status)
                   .Where(wr => wr.agenda.Contains(fnd) || wr.venue.Contains(fnd) || wr.facilitator.Contains(fnd))
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
                      aa.hashcode,
                      topicHashcode = qTopic.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().hashcode,
                      topicDesc = qTopic.Where(wr => wr.topicId == aa.topicId).FirstOrDefault().description,
                      openIssue = countOpenMeetingIssue(aa.meetingId),
                      parkIssue = countParkMeetingIssue(aa.meetingId),
                      closeIssue = countCloseMeetingIssue(aa.meetingId)
                  });

                    transList.total_count = qMeeting.Count();
                    transList.data = qMeeting.OrderByDescending(mm=>mm.dateend);

                }

                return Request.CreateResponse(HttpStatusCode.OK, transList);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpDelete]
        public HttpResponseMessage deleteMeeting(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                //remove issue
                var qIssue = db.mifs_issues.Where(wr => wr.meetingId == id).ToList();
                db.mifs_issues.RemoveRange(qIssue);

                //remove meeting attendance
                var qAttendance = db.mifs_attendance.Where(wr => wr.meetingId == id).ToList();
                db.mifs_attendance.RemoveRange(qAttendance);

                //remove meeting master
                var qMeeting = db.mifs_meeting.Where(wr => wr.meetingId == id).FirstOrDefault();
                db.mifs_meeting.Remove(qMeeting);
                db.SaveChanges();


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }



        }

        [HttpPost]
        [ActionName("post-meeting-note")]
        public HttpResponseMessage postMeetingNote(mifs_meeting p)
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;
                var qMeeting = db.mifs_meeting.Where(m => m.meetingId == p.meetingId).FirstOrDefault();
                qMeeting.notes = p.notes;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region "Attendance"

        [HttpGet]
        [ActionName("get-attendance")]
        public HttpResponseMessage getAttendance(string id)
        {
            object jsonResult = null;
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var meetingId = db.mifs_meeting.AsNoTracking().Where(wr => wr.hashcode == id).FirstOrDefault();

                    // var topicDtl = db.mifs_topic.Where(m => m.topicId == meetingId.topicId).FirstOrDefault();


                    jsonResult = db.mifs_attendance
                    .Where(m => m.meetingId == meetingId.meetingId).ToList();



                    return Request.CreateResponse(HttpStatusCode.OK, jsonResult);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [ActionName("get-Responsible-attendees")]
        public HttpResponseMessage getResponsibleAttendees(string id)
        {
            object jsonResult = null;
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var meetingId = db.mifs_meeting.AsNoTracking().Where(wr => wr.hashcode == id).FirstOrDefault();

                    var topicDtl = db.mifs_topic.Where(m => m.topicId == meetingId.topicId).FirstOrDefault();

                    if (topicDtl.topicscope == 1)
                    {
                        jsonResult = db.mifs_attendance
                        .Where(m => m.meetingId == meetingId.meetingId && m.ismember == 1).ToList();
                    }
                    else
                    {
                        jsonResult = db.mifs_attendance
                        .Where(m => m.meetingId == meetingId.meetingId).ToList();
                    }


                    return Request.CreateResponse(HttpStatusCode.OK, jsonResult);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [ActionName("post-attendance")]
        public HttpResponseMessage postAttendance(MifsAttendanceModel p)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                p.meetingId = db.mifs_meeting.Where(wr => wr.hashcode == p.hashcode).FirstOrDefault().meetingId;

                var iresult = db.Sproc_mifsSetAttendance(0, p.meetingId,
                      p.attendeeempid, p.attendeename,
                      p.attendeemail, p.attendeephone, p.ismember, p.ispresent,
                      p.created_by, DateTime.Now).First().Value;

                var qAttendance = db.mifs_attendance.AsNoTracking().Where(wr => wr.attendanceId == iresult).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qAttendance);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("put-attendance")]
        public HttpResponseMessage putAttendance(mifs_attendance p)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                db.Sproc_mifsSetAttendance(p.attendanceId, p.meetingId,
                       p.attendeeempid, p.attendeename,
                       p.attendeemail, p.attendeephone, p.ismember, p.ispresent,
                       p.created_by, DateTime.Now);

                var qAttendance = db.mifs_attendance.AsNoTracking().Where(wr => wr.attendanceId == p.attendanceId).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qAttendance);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [ActionName("delete-attendees")]
        public HttpResponseMessage RemoveAttendees(string email, string meetinghash)
        {
            try
            {
                var meetingId = db.mifs_meeting.Where(m => m.hashcode == meetinghash).FirstOrDefault().meetingId;
                var attendees = db.mifs_attendance.Where(m => m.attendeemail == email && m.meetingId == meetingId).FirstOrDefault();
                string message = "Successfully Removed " + attendees.attendeename;
                db.mifs_attendance.Remove(attendees);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, message);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("search-core-employee")]
        public HttpResponseMessage SearchEmployee(string param)
        {
            try
            {
                var model = db.mifs_members.Where(m => m.emailaddress.Contains(param) || m.fullname.Contains(param) || m.memberId.Contains(param)).Select(n => new
                {
                    n.emailaddress,
                    n.fullname,
                    n.mobileno,
                    n.nickname,
                    n.status,
                    n.memberId
                }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region "MyPalo"


        /// <summary> 
        /// get my paloUp issue
        /// </summary>
        /// <param name="userid">userid</param>
        /// <param name="itype">1:Responsible,2:toolgater,3 colaborator ,4 pending for acknowledge</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage getMypalo(string email, int itype)
        {
            MifsIssues n = new MifsIssues();
            object objResult = null;
            try
            {
                switch (itype)
                {
                    case 1:
                        //get issue where responsible is userid
                        var responsibleIssue = db.mifs_vIssues
                     .Where(m => m.responemail == email && string.IsNullOrEmpty(m.newstatus) == true)
                      .OrderBy(or => or.TopicAlias)
                     .ThenBy(or => or.PriorityFlag)
                     .OrderByDescending(or => or.DateIssued)
                     .OrderBy(or => or.issueId)
                     .ToList()
                     .Select(aa => new
                     {
                         aa,
                         canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                     });
                        objResult = responsibleIssue;

                        break;
                    case 2:
                        //get issue where tollgater is userid
                        var tollgaterIssue = db.mifs_vIssues
                     .Where(m => m.raisedbyemail == email && string.IsNullOrEmpty(m.newstatus) == true)
                      .OrderBy(or => or.TopicAlias)
                     .ThenBy(or => or.PriorityFlag)
                     .OrderByDescending(or => or.DateIssued)
                     .OrderBy(or => or.issueId)
                     .ToList()
                     .Select(aa => new
                     {
                         aa,
                         canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                     });
                        objResult = tollgaterIssue;

                        break;
                    case 3:
                        //get issue where colaborator is userid
                        var collaboratorIssue = db.mifs_vIssues
                     .Where(m => m.collaboratoremail == email)
                      .OrderBy(or => or.TopicAlias)
                     .ThenBy(or => or.PriorityFlag)
                     .OrderByDescending(or => or.DateIssued)
                     .OrderBy(or => or.issueId)
                     .ToList()
                     .Select(aa => new
                     {
                         aa,
                         canResponse = true
                     });
                        objResult = collaboratorIssue;

                        break;
                    case 4:
                        //get pending issue
                        var pendingIssue = db.mifs_vIssues.Where(m => (m.responemail == email || m.raisedbyemail == email || m.collaboratoremail == email) && string.IsNullOrEmpty(m.newstatus) == false)
                    .OrderBy(or => or.TopicAlias)
                     .ThenBy(or => or.PriorityFlag)
                     .OrderByDescending(or => or.DateIssued)
                     .OrderBy(or => or.issueId)
                   .ToList()
                   .Select(aa => new
                   {
                       aa,
                       canResponse = true
                   });
                        objResult = pendingIssue;
                        break;
                    case 5:
                        //get priority issue
                        var piorityIssue = db.mifs_vIssues.Where(m => (m.responemail == email || m.raisedbyemail == email || m.collaboratoremail == email) && (m.PriorityFlag >=1 && m.PriorityFlag != 99) && string.IsNullOrEmpty(m.newstatus) == true)
                    .OrderBy(or => or.TopicAlias)
                     .ThenBy(or => or.PriorityFlag)
                     .OrderByDescending(or => or.DateIssued)
                     .OrderBy(or => or.issueId)
                   .ToList()
                   .Select(aa => new
                   {
                       aa,
                       canResponse = n.isCanupdateEmail(aa.CurrentTopicId, aa.raisedbyemail, aa.responemail, email)
                   });
                        objResult = piorityIssue;
                        break;

                }

                return Request.CreateResponse(HttpStatusCode.OK, objResult);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region "Private function"




        private string findClassification(mifs_vIssues obj, string emppId)
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


        public int countOpenMeetingIssue(int meetingId)
        {
            int iresult = db.mifs_issues.Select(mm => new { mm.sourcetype, mm.meetingId, mm.status }).Where(m => m.sourcetype == "M" && m.meetingId == meetingId && m.status == "O").Count();
            return iresult;

        }

        public int countCloseMeetingIssue(int meetingId)
        {

            return db.mifs_issues.Select(mm => new { mm.sourcetype, mm.meetingId, mm.status }).Where(m => m.sourcetype == "M" && m.meetingId == meetingId && m.status == "C").Count();

        }

        public int countParkMeetingIssue(int meetingId)
        {

            return db.mifs_issues.Select(mm=> new { mm.sourcetype,mm.meetingId,mm.status}).Where(m => m.sourcetype == "M" && m.meetingId == meetingId && m.status == "P").Count();

        }


        private bool isCanupdateEmail(int topicId, string raisedMail, string responMail, string uEmail)
        {

            db.Configuration.LazyLoadingEnabled = false;

            if (uEmail == raisedMail || uEmail == responMail)
            {
                return true;
            }
            else
            {
                return false;
                //var qMemberSource = db.mifs_vTopicMembers.Where(wr => wr.MemberEmailAddress == uEmail && wr.topicId == topicId).FirstOrDefault();
                //if (qMemberSource.CanUpdateOtherRespondent == "No")
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
            }
        }


        public List<string> setDefaultColors()
        {
            List<string> colors = new List<string>();
            colors.Add("orange");
            colors.Add("purple");
            colors.Add("brown");
            colors.Add("green");
            return colors;
        }
        #endregion

        #region "Private Class"


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

        public class MifsMailAccountModel
        {
            public string empId { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }

        private class PaginationViewModel
        {
            public int total_count { get; set; }
            public List<dynamic> data { get; set; }
        }


        public class MifsDashboardViewModel
        {
            public StackColumnViewModel stackColumn { get; set; }
         //   public List<TreeMapViewModel> tollgaterTreeMap { get; set; }
            public List<TreeMapViewModel> responsibleTreeMap { get; set; }
        }



        public class StackColumnViewModel
        {
            public List<string> categories { get; set; }
            public List<StackColumnData> series { get; set; }
        }

        public class StackColumnData
        {
            public string name { get; set; }
            public List<int> data { get; set; }
        }

        public class TreeMapViewModel
        {
            public string id { get; set; }
            public string name { get; set; }
            public string color { get; set; }
            public int? value { get; set; }
            public string parent { get; set; }
        }

        public class customUserBranchModel
        {
            public string empid { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string userid { get; set; }
            public string branch { get; set; }
        }

        private class TopicModel
        {
            public int topicId { get; set; }
            public string TopicDescription { get; set; }
            public string TopicAlias { get; set; }
            public string hashcode { get; set; }
            public string occurence { get; set; }
            public string bgcolor { get; set; }
            public int countOpen { get; set; }
            public int countClose { get; set; }
            public int countPark { get; set; }
            public int countMeeting { get; set; }
        }

        #endregion

        #region "Calendar"

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
                            (m.responempid == empid ||
                            m.collaboratorempid == empid ||
                            m.raisedbyempid == empid) &&
                            ((m.lastsenddate >= startDate && m.lastsenddate <= endDate) ||
                            (m.IssueDateParked >= startDate && m.IssueDateParked <= endDate) ||
                            (m.DateClosed >= startDate && m.DateClosed <= endDate))
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
                        Calendar temp = new Calendar();
                        if (item.DateClosed >= startDate && item.DateClosed <= endDate)
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
                        else if (item.IssueDateParked >= startDate && item.IssueDateParked <= endDate)
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
                        else if (item.lastsenddate >= startDate && item.lastsenddate <= endDate)
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

        #endregion


        #region "Android"

        public HttpResponseMessage GetMypaloCountType(string id)
        {
            try
            {

                List<MypaloCountType> lstMypaloCountType = new List<MypaloCountType>();
                db.Configuration.LazyLoadingEnabled = false;

                lstMypaloCountType.Add(new MypaloCountType { });



                return Request.CreateResponse(HttpStatusCode.OK, lstMypaloCountType);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion


        #region "Dropdown Maintenance"

        public HttpResponseMessage GetDistinctDropdownCode()
        {
            try
            {

                var distinctdropdowncode = db.mifs_dropdowngroup.Where(mm=>mm.showinuser == 1).Select(mm=>mm.code).ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, distinctdropdowncode);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public HttpResponseMessage GetDistinctDisplay()
        {
            try
            {

                var distinctdropdowncode = db.mifs_dropdown.Where(mm=>mm.code != "schedtime" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, distinctdropdowncode);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public HttpResponseMessage GetAllByDropdownCode(string distinctcode)
        {
            try
            {

                var distinctdropdown = db.mifs_dropdown.Where(mm=>mm.code == distinctcode).ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, distinctdropdown);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }







        [HttpPost]
        public HttpResponseMessage PostBulkDropdownMaintenance(List<mifs_dropdown> dropdownmaintenance)
        {
            try
            {

              
                db.Configuration.LazyLoadingEnabled = false;
                foreach(var list in dropdownmaintenance)
                {
                    var dropdownlist = db.mifs_dropdown.Where(mm => mm.id == list.id).FirstOrDefault();
                    
                    if(list.id == 0)
                    {


                        list.createddate = DateTime.Now;
                        db.mifs_dropdown.Add(list);
                       


                     
                    }
                    else
                    {

                        CompareLogic compareLogic = new CompareLogic();

                        ComparisonResult result = compareLogic.Compare(list, dropdownlist);
                        if (!result.AreEqual)
                        {
                            dropdownlist.code = list.code;
                            dropdownlist.value = list.value;
                            dropdownlist.details = list.details;
                            dropdownlist.display = list.display;
                            dropdownlist.leadtimemin = list.leadtimemin;
                            dropdownlist.lookup = list.lookup;
                            dropdownlist.leadtimemax = list.leadtimemax;
                            dropdownlist.status = list.status;
                            dropdownlist.isdelete = list.isdelete;
                            dropdownlist.sortorder = list.sortorder;
                            //dropdownlist.modifiedby = list.createdby;
                            dropdownlist.modifieddate = DateTime.Now;




                            db.Entry(dropdownlist).State = EntityState.Modified;
                        }
                    }
                }
              
                db.SaveChanges();




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpPost]
        public HttpResponseMessage PostDropdownMaintenance(mifs_dropdown dropdownmaintenance)
        {
            try
            {


                db.Configuration.LazyLoadingEnabled = false;
            
                    var dropdownlist = db.mifs_dropdown.Where(mm => mm.id == dropdownmaintenance.id).FirstOrDefault();

                    if (dropdownmaintenance.id == 0)
                    {

                        dropdownmaintenance.status = "A";
                        dropdownmaintenance.createddate = DateTime.Now;
                        db.mifs_dropdown.Add(dropdownmaintenance);




                    }
                    else
                    {

                        CompareLogic compareLogic = new CompareLogic();

                        ComparisonResult result = compareLogic.Compare(dropdownmaintenance, dropdownlist);
                        if (!result.AreEqual)
                        {
                            dropdownlist.code = dropdownmaintenance.code;
                            dropdownlist.value = dropdownmaintenance.value;
                            dropdownlist.details = dropdownmaintenance.details;
                            dropdownlist.display = dropdownmaintenance.display;
                            dropdownlist.leadtimemin = dropdownmaintenance.leadtimemin;
                            dropdownlist.lookup = dropdownmaintenance.lookup;
                            dropdownlist.leadtimemax = dropdownmaintenance.leadtimemax;
                            dropdownlist.status = dropdownmaintenance.status;
                            dropdownlist.isdelete = dropdownmaintenance.isdelete;
                            dropdownlist.sortorder = dropdownmaintenance.sortorder;
                            dropdownlist.modifiedby = dropdownmaintenance.modifiedby;
                            dropdownlist.modifieddate = DateTime.Now;




                            db.Entry(dropdownlist).State = EntityState.Modified;
                        }
                    }
                

                db.SaveChanges();




                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public HttpResponseMessage GetSubkpiDropdown()
        {
            try
            {

                var subkpi = db.mifs_dropdown.Where(mm => mm.code == "subkpitagging" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, subkpi);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        public HttpResponseMessage GetCategoryDropdown()
        {
            try
            {

                var maintaincategory = db.mifs_dropdown.Where(mm=>mm.code == "issuecategory" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;

               



                return Request.CreateResponse(HttpStatusCode.OK, maintaincategory);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetTimeRangeDropdown()
        {
            try
            {

                var maintaincategory = db.mifs_dropdown.Where(mm => mm.code == "schedtime" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, maintaincategory);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetAllCategoryDropdown()
        {
            try
            {

                var maintaincategory = db.mifs_dropdown.ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, maintaincategory);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        public HttpResponseMessage GetIssueLevelDropdown()
        {
            try
            {

                var issuelevel = db.mifs_dropdown.Where(mm => mm.code == "issuelevel" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, issuelevel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        
        public HttpResponseMessage GetTopicLevelDropdown()
        {
            try
            {

                var topiclevel = db.mifs_dropdown.Where(mm => mm.code == "topiclevel" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, topiclevel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public HttpResponseMessage GetKpiTaggingDropdown()
        {
            try
            {

                var topiclevel = db.mifs_dropdown.Where(mm => mm.code == "kpitagging" && mm.isdelete == 0 && mm.status == "A").ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, topiclevel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public HttpResponseMessage GetAllEmp()
        {
            try
            {

                var topiclevel = db.core_employee.Where(mm => mm.type != "SEP").Select(mm=> new {employeename = mm.employeename2,employeeId = mm.empl_id,employeeemail = mm.emailadd  }).OrderBy(mm=>mm.employeename).ToList();
                db.Configuration.LazyLoadingEnabled = false;





                return Request.CreateResponse(HttpStatusCode.OK, topiclevel);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        #endregion


        #region "Palo Clousre Chart"

        public HttpResponseMessage GetPaloChartData(string useremail)
        {
            try
            {

            //var paloclosurechardata = db.Sproc_mifsClosureChart(useremail).ToList();
           
         
                db.Configuration.LazyLoadingEnabled = false;
                var palodailypendingapprovebyraiseddata = db.Database
                                  .SqlQuery<DailyPendingApprovedRaisedby>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 1));
                var paloweeklypendingapprovebyraiseddata = db.Database
                                  .SqlQuery<WeeklyPendingAprovedRaisedBy>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 2));
                var palodailyopenissuesdata = db.Database
                                  .SqlQuery<DailyOpenIssues>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 3));
                var paloweeklyclosedata = db.Database
                                  .SqlQuery<WeeklyCloseIssue>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 4));
                var paloweeklypendingapprovebyresponsedata = db.Database
                                  .SqlQuery<WeeklyPendingApprovedByResponsible>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 5));
                var palotollgaterapprovedata = db.Database
                                  .SqlQuery<RequestCloseTollgater>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 6));
                var palocollaboratorapprovedata = db.Database
                                  .SqlQuery<RequestCloseCollaborator>("[dbo].[Sproc_mifsClosureChart] @useremail,@param", new SqlParameter("@useremail", useremail), new SqlParameter("@param", 7));




                var paloclosuredatachart = new
                {
                    palodailypendingapprovebyraiseddata,
                    paloweeklypendingapprovebyraiseddata,
                    palodailyopenissuesdata,
                    paloweeklyclosedata,
                    paloweeklypendingapprovebyresponsedata,
                    palotollgaterapprovedata,
                    palocollaboratorapprovedata
                };








                return Request.CreateResponse(HttpStatusCode.OK, paloclosuredatachart);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }




        }



        #endregion


        #region Email Template
        private static string GetHtmlTable(List<BulkIssueForDrop> bulkissuefordrop)
        {
            DataTable dt = new DataTable();
          //  dt.Columns.Add("No.");
            dt.Columns.Add("IssueId");
            dt.Columns.Add("Details");
            dt.Columns.Add("ActionPlan");
            dt.Columns.Add("RaisedBy/Tollgater");
            dt.Columns.Add("Responsible");
            dt.Columns.Add("Issue Date");

         


            foreach (var issueitem in bulkissuefordrop.First().issuemodel.ToList())
                {
                    DataRow dr = dt.NewRow();
                 //   dr["No."] = issueitem;
                    dr["IssueId"] = issueitem.issueId;
                    dr["Details"] = issueitem.issuedetails;
                    dr["ActionPlan"] = issueitem.actionplan;
                    dr["RaisedBy/Tollgater"] = issueitem.raisedby;
                    dr["Responsible"] = issueitem.responsible;
                    dr["Issue Date"] = issueitem.issuedate.ToString("MMM dd, yyyy");
               
                    dt.Rows.Add(dr);
                }


        

            return ConvertToHtml(dt);
        }

        private static string ConvertToHtml(DataTable dt)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
      
            sb.AppendLine("<table style='border: 1px solid #1C6EA4;background-color: #EEEEEE;width: 100 %;text-align: center;border-collapse: collapse'>");
            foreach (DataColumn dc in dt.Columns)
            {
                sb.AppendFormat("<th style='background: #1C6EA4;background:-moz-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);background:-webkit-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);background: linear-gradient(to bottom, #5592bb 0%, #327cad 66%, #1C6EA4 100%);font-size: 15px;font-weight: bold;'>{0}</th>", dc.ColumnName);
            }

            foreach (DataRow dr in dt.Rows)

            {
                sb.Append("<tr>");
                foreach (DataColumn dc in dt.Columns)
                {
                    string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                    sb.AppendFormat("<td style='border: 1px solid #AAAAAA;padding: 3px 2px;'>{0}</td>", cellValue);
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
      
            return sb.ToString();
        }


        private static string GetHtmlTableTransfer(List<BulkIssueForTransfer> bulkissuefortransferraisedby)
        {
            DataTable dt = new DataTable();
            //  dt.Columns.Add("No.");
            dt.Columns.Add("IssueId");
            dt.Columns.Add("Details");
            dt.Columns.Add("ActionPlan");
            dt.Columns.Add("RaisedBy/Tollgater");
            dt.Columns.Add("Responsible");
            dt.Columns.Add("Issue Date");




            foreach (var issueitem in bulkissuefortransferraisedby.First().issuemodel.ToList())
            {
                DataRow dr = dt.NewRow();
                //   dr["No."] = issueitem;
                dr["IssueId"] = issueitem.issueId;
                dr["Details"] = issueitem.issuedetails;
                dr["ActionPlan"] = issueitem.actionplan;
                dr["RaisedBy/Tollgater"] = issueitem.raisedby;
                dr["Responsible"] = issueitem.responsible;
                dr["Issue Date"] = issueitem.issuedate.ToString("MMM dd, yyyy");

                dt.Rows.Add(dr);
            }




            return ConvertToHtmlTransfer(dt);
        }

        private static string ConvertToHtmlTransfer(DataTable dt)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("<table style='border: 1px solid #1C6EA4;background-color: #EEEEEE;width: 100 %;text-align: center;border-collapse: collapse'>");
            foreach (DataColumn dc in dt.Columns)
            {
                sb.AppendFormat("<th style='background: #1C6EA4;background:-moz-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);background:-webkit-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);background: linear-gradient(to bottom, #5592bb 0%, #327cad 66%, #1C6EA4 100%);font-size: 15px;font-weight: bold;'>{0}</th>", dc.ColumnName);
            }

            foreach (DataRow dr in dt.Rows)

            {
                sb.Append("<tr>");
                foreach (DataColumn dc in dt.Columns)
                {
                    string cellValue = dr[dc] != null ? dr[dc].ToString() : "";
                    sb.AppendFormat("<td style='border: 1px solid #AAAAAA;padding: 3px 2px;'>{0}</td>", cellValue);
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        #endregion


        #region QMS

        //QMS
        [HttpPost]
        [ActionName("post-externalsystems-issue")]
        public HttpResponseMessage postIssuebyOtherSystems(OtherSystemsIssue issueModel)
        {
            try
            {

                //check if responsible is member
                if (db.mifs_members.Where(x => x.memberId == issueModel.issue.responempid && x.status == "A").Count() == 0)
                {
                    mifs_members newMember = new mifs_members();
                    newMember.memberId = issueModel.issue.responempid;

                    postMember(newMember);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.responempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }

                else if (db.mifs_topicmembers.Where(x => x.topicId == issueModel.issue.topicId && x.memberId == issueModel.issue.responempid).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.responempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }

                //check if collaborator is member
                if (db.mifs_members.Where(x => x.memberId == issueModel.issue.collaboratorempid && x.status == "A").Count() == 0)
                {
                    mifs_members newMember = new mifs_members();
                    newMember.memberId = issueModel.issue.collaboratorempid;

                    postMember(newMember);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.collaboratorempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }

                else if (db.mifs_topicmembers.Where(x => x.topicId == issueModel.issue.topicId && x.memberId == issueModel.issue.collaboratorempid).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.collaboratorempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }

                //check if tollgater is member
                if (db.mifs_members.Where(x => x.memberId == issueModel.issue.raisedbyempid && x.status == "A").Count() == 0)
                {
                    mifs_members newMember = new mifs_members();
                    newMember.memberId = issueModel.issue.raisedbyempid;

                    postMember(newMember);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.raisedbyempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }

                else if (db.mifs_topicmembers.Where(x => x.topicId == issueModel.issue.topicId && x.memberId == issueModel.issue.raisedbyempid).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = issueModel.issue.topicId;
                    tm.memberId = issueModel.issue.raisedbyempid;
                    tm.created_by = issueModel.createdbyID;
                    postTopicMember(tm);
                }


                //post issue
                List<mifs_dropdown> paloDrop = db.mifs_dropdown.Where(x => x.code == "issuecategory").ToList();
                issueModel.issue.lastsenddate = DateTime.Now.AddDays(issueModel.issue.followfrequency);
                issueModel.issue.create_date = DateTime.Now;


                if (issueModel.issue.issuecategory == null)
                {
                    int daysCompletion = Convert.ToInt32((issueModel.issue.targetcompletiondate - issueModel.issue.issuedate).TotalDays.ToString());
                    //Operational(value:1 Days:1-21)
                    //Tactical(value:2 Days:22-90)
                    //Strategic(value:3 Days:91-365)

                    foreach (mifs_dropdown drop in paloDrop)
                    {
                        if (daysCompletion >= drop.leadtimemin && daysCompletion <= drop.leadtimemax)
                        {
                            issueModel.issue.issuecategory = drop.id.ToString();
                        }
                    }

                }
                postIssues(issueModel.issue);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-qms-issue")]
        public HttpResponseMessage postIssuebyQMS(QMSIssue qmsissue)
        {
            try
            {
                //check the correct topic ID
                int CARtopicID = 1399;
                int GWPtopicID = 1398;
                int IARTStopicID = 1437;

                int topicID = 0;

                if (qmsissue.type == "CAR")
                {
                    topicID = CARtopicID;
                }
                if (qmsissue.type == "GWP")
                {
                    topicID = GWPtopicID;
                }
                if (qmsissue.type == "IARTS")
                {
                    topicID = IARTStopicID;
                }

                //check if responsible is memeber
                if (db.mifs_members.Where(x => x.memberId == qmsissue.Responsible.memberId && x.status == "A").Count() == 0)
                {
                    postMember(qmsissue.Responsible);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Responsible.memberId;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }
                else if (db.mifs_topicmembers.Where(x => x.topicId == topicID && x.memberId == qmsissue.Responsible.memberId).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Responsible.memberId;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }

                //check if collaborator is memeber
                if (db.mifs_members.Where(x => x.memberId == qmsissue.Issue.collaboratorempid && x.status == "A").Count() == 0)
                {
                    mifs_members newMember = new mifs_members();
                    newMember.memberId = qmsissue.Issue.collaboratorempid;
                    postMember(newMember);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Issue.collaboratorempid;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }
                else if (db.mifs_topicmembers.Where(x => x.topicId == topicID && x.memberId == qmsissue.Issue.collaboratorempid).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Issue.collaboratorempid;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }

                //check if collaborator is memeber
                if (db.mifs_members.Where(x => x.memberId == qmsissue.Issue.raisedbyempid && x.status == "A").Count() == 0)
                {
                    mifs_members newMember = new mifs_members();
                    newMember.memberId = qmsissue.Issue.raisedbyempid;
                    postMember(newMember);
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Issue.raisedbyempid;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }
                else if (db.mifs_topicmembers.Where(x => x.topicId == topicID && x.memberId == qmsissue.Issue.raisedbyempid).Count() == 0)
                {
                    mifs_topicmembers tm = new mifs_topicmembers();
                    tm.create_date = DateTime.Now;
                    tm.topicId = topicID;
                    tm.memberId = qmsissue.Issue.raisedbyempid;
                    tm.created_by = qmsissue.Responsible.created_by;
                    postTopicMember(tm);
                }

                //post issue
                string branHRISresp = db.core_employee.Where(x => x.empl_id == qmsissue.Issue.responempid).FirstOrDefault().branch;
                qmsissue.Issue.branchcode = db.core_branchmst.Where(x => x.hrisbranchcode == branHRISresp).FirstOrDefault().brancode;
                qmsissue.Issue.lastsenddate = DateTime.Now.AddDays(qmsissue.Issue.followfrequency);
                qmsissue.Issue.issuedate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                qmsissue.Issue.create_date = DateTime.Now;
                qmsissue.Issue.topicId = topicID;

                if (qmsissue.Issue.issuecategory == null)
                {
                    List<mifs_dropdown> paloDrop = db.mifs_dropdown.Where(x => x.code == "issuecategory").ToList();
                    double daysCompletion = (qmsissue.Issue.targetcompletiondate - qmsissue.Issue.issuedate).TotalDays;
                    //Operational(value:1 Days:1-21)
                    //Tactical(value:2 Days:22-90)
                    //Strategic(value:3 Days:91-365)

                    foreach (mifs_dropdown drop in paloDrop)
                    {
                        if (daysCompletion >= drop.leadtimemin && daysCompletion <= drop.leadtimemax)
                        {
                            qmsissue.Issue.issuecategory = drop.id.ToString();
                        }
                    }

                }
                postIssues(qmsissue.Issue);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
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