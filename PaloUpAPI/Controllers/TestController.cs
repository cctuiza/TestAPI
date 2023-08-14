using PaloUpAPI.Class;
using PaloUpAPI.Models;
using PaloUpAPI.Service;
using PaloUpAPI.SQLData;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PaloUpAPI.Controllers
{
    //[Compress]
    //[CustomAuthorize]


    public class TestController : ApiController
    {
         PALOUPEntities db = new PALOUPEntities();
         clsprocedure procedure = new clsprocedure();
        public dynamic get(int topicId)
        {
           
           using(PALOUPEntities db =new PALOUPEntities())
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qToolgater = db.Sproc_mifsGetMemberSeparated(1).ToList();
                   var outTollgater= qToolgater.Where(m=> m.topicId==topicId).Select(m=> new { m.topicId}).Distinct();

                foreach(var row in outTollgater)
                {
                    //get all topic owner
                    var getOwner = db.mifs_topicmembers.Where(m => m.accesslevel == "O");

                    //check topic if has topic owner
                    if (getOwner.Count() > 0)
                    {
                        foreach(var support in getOwner)
                        {

                        }

                    }else
                    {
                        //if the topic is no topic owner then the system get system support personel
                        var supportemail = db.core_appsystem.Where(m => m.sysId == 19).FirstOrDefault().emailsupport;


                    }
                }


                return outTollgater;
;
            }
        }
        [HttpPost]
        [ActionName("export-to-excel")]
        public HttpResponseMessage Get(List<TopicList> topics)
        {

            //DataTable Dt= GetData();  Getdata from database in thr form d
            //Generate  html table sturcture
       
            List<IssueReport> IssueList = new List<IssueReport>();
            MifsReport n = new MifsReport();
        
            foreach (var topic in topics)
            {

              if(topic.year != 0)
                {
                    if (topic.status != "")
                    {
                        var tempIssueList = db.mifs_vIssueReport
                              .Where(mm => mm.IssueStatus == topic.status && mm.CurrentTopicId == topic.topicId && mm.DateIssued.Year == topic.year)
                              .OrderBy(mm => mm.DateIssued)
                              .ToList().Select(aa => new
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
                                  aa.FollowUpCounter,
                                  year = aa.DateIssued.Year

                              }).ToList();


                        foreach (var issuereport in tempIssueList)

                        {
                            IssueReport temp = new IssueReport();
                            temp.issueId = issuereport.issueId;
                            temp.pflag = issuereport.pflag;
                            temp.DateIssued = issuereport.DateIssued;
                            temp.Details = issuereport.Details;
                            temp.TopicAlias = issuereport.TopicAlias;
                            temp.actionplan = issuereport.actionplan;
                            temp.targetcompletiondate = issuereport.targetcompletiondate;
                            temp.raisedby = issuereport.raisedby;
                            temp.responsible = issuereport.responsible;
                            temp.update = issuereport.update;
                            temp.FollowUpCounter = issuereport.FollowUpCounter;

                            IssueList.Add(temp);
                        }




                    }
                    else
                    {
                        var tempIssueList = db.mifs_vIssueReport
                 .Where(mm => mm.CurrentTopicId == topic.topicId && mm.DateIssued.Year == topic.year)
                .OrderBy(mm => mm.DateIssued)
                .ToList().Select(aa => new
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
                    aa.FollowUpCounter,
                    year = aa.DateIssued.Year

                }).ToList();


                        foreach (var issuereport in tempIssueList)

                        {
                            IssueReport temp = new IssueReport();
                            temp.issueId = issuereport.issueId;
                            temp.pflag = issuereport.pflag;
                            temp.DateIssued = issuereport.DateIssued;
                            temp.Details = issuereport.Details;
                            temp.TopicAlias = issuereport.TopicAlias;
                            temp.actionplan = issuereport.actionplan;
                            temp.targetcompletiondate = issuereport.targetcompletiondate;
                            temp.raisedby = issuereport.raisedby;
                            temp.responsible = issuereport.responsible;
                            temp.update = issuereport.update;
                            temp.FollowUpCounter = issuereport.FollowUpCounter;

                            IssueList.Add(temp);
                        }




                    }
                }
                else
                {
                    if (topic.status != "")
                    {
                        var tempIssueList = db.mifs_vIssueReport
                              .Where(mm => mm.IssueStatus == topic.status && mm.CurrentTopicId == topic.topicId)
                              .OrderBy(mm => mm.DateIssued)
                              .ToList().Select(aa => new
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
                                  aa.FollowUpCounter,
                                  year = aa.DateIssued.Year

                              }).ToList();


                        foreach (var issuereport in tempIssueList)

                        {
                            IssueReport temp = new IssueReport();
                            temp.issueId = issuereport.issueId;
                            temp.pflag = issuereport.pflag;
                            temp.DateIssued = issuereport.DateIssued;
                            temp.Details = issuereport.Details;
                            temp.TopicAlias = issuereport.TopicAlias;
                            temp.actionplan = issuereport.actionplan;
                            temp.targetcompletiondate = issuereport.targetcompletiondate;
                            temp.raisedby = issuereport.raisedby;
                            temp.responsible = issuereport.responsible;
                            temp.update = issuereport.update;
                            temp.FollowUpCounter = issuereport.FollowUpCounter;

                            IssueList.Add(temp);
                        }




                    }
                    else
                    {
                        var tempIssueList = db.mifs_vIssueReport
                 .Where(mm => mm.CurrentTopicId == topic.topicId)
                .OrderBy(mm => mm.DateIssued)
                .ToList().Select(aa => new
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
                    aa.FollowUpCounter,
                    year = aa.DateIssued.Year

                }).ToList();


                        foreach (var issuereport in tempIssueList)

                        {
                            IssueReport temp = new IssueReport();
                            temp.issueId = issuereport.issueId;
                            temp.pflag = issuereport.pflag;
                            temp.DateIssued = issuereport.DateIssued;
                            temp.Details = issuereport.Details;
                            temp.TopicAlias = issuereport.TopicAlias;
                            temp.actionplan = issuereport.actionplan;
                            temp.targetcompletiondate = issuereport.targetcompletiondate;
                            temp.raisedby = issuereport.raisedby;
                            temp.responsible = issuereport.responsible;
                            temp.update = issuereport.update;
                            temp.FollowUpCounter = issuereport.FollowUpCounter;

                            IssueList.Add(temp);
                        }




                    }
                }
                








            }

            DataTable dt = new DataTable();
            dt = procedure.ToDataSet(IssueList);

            var grid = new GridView();
            grid.DataSource = dt;
            grid.DataBind();
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);
            string str = "<table><tr> <th>colume1</th></tr><tr> <td>abc</td> </tr></table>";
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(sw.ToString());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            result.Content.Headers.ContentDisposition.FileName = "data.xlsx";
            return result;
        }

        public HttpResponseMessage Get()
        {

            //DataTable Dt= GetData();  Getdata from database in thr form d
            //Generate  html table sturcture
            string str = "<table><tr> <th>colume1</th></tr><tr> <td>abc</td> </tr></table>";
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(str);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            result.Content.Headers.ContentDisposition.FileName = "data.xlsx";
            return result;
        }

        public dynamic getIssue()
        {
            MifsServices n = new MifsServices();

         //   n.postNotificationNotOverDue();
            return "Ok";
            //using (PALOUPEntities db = new PALOUPEntities())
            //{
            //    db.Configuration.LazyLoadingEnabled = false;

            //    var list = db.mifs_issues.Where(m => m.status == "O" && (m.acknowledgedbysource + m.acknowledgebyrespondent) == 0 && DbFunctions.TruncateTime(DateTime.Now) == DbFunctions.TruncateTime(m.lastsenddate)).ToList();
            //    // var qDailyIssue = db.mifs_issues
            //    //.Where(wr => wr.status == "O" && (wr.acknowledgedbysource + wr.acknowledgebyrespondent) == 0 && DateTime.Now == DbFunctions.TruncateTime(wr.lastsenddate))
            //    //.ToList();
            //    return list;
            //}
        }

    }



    public class tokenn
    {
        public static string serverId { get; set; }
      
    }
}
