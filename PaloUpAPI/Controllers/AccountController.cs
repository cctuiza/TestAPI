using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using PaloUpAPI.Models;
using System.Net.Mail;
using PaloUpAPI.SQLData;
using System.Web.Http.Description;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Data;
using System.Net.Http;
using PaloUpAPI.ActionFilters;

namespace PaloUpAPI.Controllers
{
    [AuthorizationRequired]
    public class AccountController : ApiController
    {
        clsprocedure procedure = new clsprocedure();
        PALOUPEntities db = new PALOUPEntities();
        //HRISDBEntities iedb = new HRISDBEntities();

        /// <summary>
        /// Fast login centralize login any apps in fastlogistics
        /// </summary>
        /// <param name="userid">Userid or email</param>
        /// <param name="password">password</param>
        /// <param name="appname">application name</param>
        /// <returns></returns>
        //[HttpGet]
        //public clsFastLogin Fastlogin(string userid, string password, string appname)
        //{
        //    AccountRights acccountRights = new AccountRights();
        //    return acccountRights.getFastLogin(userid, password, appname);
        //}

        [HttpGet]
        public HttpResponseMessage FastloginHascode(string hascode, string appname)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var qUserProfile = db.core_users.Where(m => m.hashcode == hascode).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qUserProfile);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult getAlias(int id)
        {
            try
            {
                AccountRights acccountRights = new AccountRights();
                return Json(new
                {
                    hasError = false ,
                    alias = acccountRights.getAlias(id)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    hasError = true,
                    errorMessage = ex.Message
                });
            }
        }

        [HttpGet]
        public IHttpActionResult setAlias(int id,string alias)
        {
            try
            {
                AccountRights acccountRights = new AccountRights();
                 acccountRights.setAlias(id, alias);
                return Json(new
                {
                    hasError = false
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    hasError = true,
                    errorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Sent Mail
        /// </summary>
        /// <param name="subject">subject</param>
        /// <param name="mailbody">mailbody</param>
        /// <param name="tomail">to</param>
        /// <param name="ccmail">cc</param>
        /// <param name="bbcmail">bbc</param>
        /// <returns>boolen</returns>
        [HttpGet]
        public clsSendMail SentMail(string tomail,string subject, string mailbody,string mailer, string mailerName)
        {

            clsSendMail clsmail = new clsSendMail();

            try
            {

                MailMessage msg = new MailMessage();

                if (!string.IsNullOrEmpty(mailer))
                {
                    if (!string.IsNullOrEmpty(mailerName))
                    {
                        msg.From = new MailAddress(mailer, mailerName);
                    }
                    else {
                        msg.From = new MailAddress(mailer);
                    }
                   
             
                }
                else {
                    msg.From = new MailAddress("no-reply@fastgroup.biz");
                }
                

                string[] mailto = tomail.Split(';');

                foreach (string strmail in mailto)
                {
                    msg.To.Add(strmail);
                    
                }


                msg.Subject = subject;

                msg.Body = mailbody;
                msg.IsBodyHtml = true;
                SmtpClient sc = new SmtpClient("mail.flcprivate.dns");

                sc.Port = 587;

                sc.Credentials = new NetworkCredential("flc", "flc213");

                sc.Send(msg);
                clsmail.haserror = false;
                return clsmail;
            }
            catch (Exception ex)
            {
                clsmail.haserror = true;
                clsmail.error_message = ex.Message;
                return clsmail;
            }
        }

        /// <summary>
        /// Sent SMS
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="smsMsg"></param>
        /// <param name="appname"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SentSMS(SmsParam param) {
            try
            {

            
                SMS.SendSms(param.mobile, param.smsMsg, param.appname, param.smsSender);
                return Json(new { hasError = false, successMessage="SMS message has been sent." });
            }
            catch (Exception ex) {
                return Json(new { hasError = true, errorMessage = ex.Message });
            
            }
        }


        /// <summary>
        /// Get Employee Profile
        /// </summary>
        /// <param name="userid">employee ID</param>
        /// <returns></returns>
        [HttpGet]
        public clsProfile Profile(string userid)
        {
            clsProfile mypro = new clsProfile();
            try
            {
                //    var q_emp_profile = from aa in iedb.V_hrisEMPLOYEE
                //                            where aa.EMPL_ID == userid
                //                            select aa;

                //        if (q_emp_profile.Count() > 0)
                //        {
                //            List<clsProfileMst> ProMst = new List<clsProfileMst>();
                //            List<clsinfo> employee_details = new List<Models.clsinfo>();
                //            List<clsinfo> employment_details = new List<Models.clsinfo>();
                //            List<clsinfo> Immediate_details = new List<Models.clsinfo>();
                //            Age countage = new Age(DateTime.Parse(q_emp_profile.First().DATEHIRED.ToString()),DateTime.Now);
                //            int hyr = countage.Years;
                //            int hMonth = countage.Months;
                //            int hdays = countage.Days;
                //            DateTime today = DateTime.Today;
                //            int age = today.Year - DateTime.Parse(q_emp_profile.First().BIRTHDAY.ToString()).Year;

                //            if (DateTime.Parse(q_emp_profile.First().BIRTHDAY.ToString()) > today.AddYears(-age)) age--;

                //            employee_details.Add(new clsinfo { fieldname = "Employee No.", value = q_emp_profile.First().EMPL_ID });
                //            employee_details.Add(new clsinfo { fieldname = "Email Address", value = q_emp_profile.First().EMAILADD });
                //            employee_details.Add(new clsinfo { fieldname = "Birthday", value = DateTime.Parse(q_emp_profile.First().BIRTHDAY.ToString()).ToShortDateString() });
                //            employee_details.Add(new clsinfo { fieldname = "Age", value = age.ToString() });
                //            employee_details.Add(new clsinfo { fieldname = "Civil Status", value = q_emp_profile.First().CIVILSTATUSDESC });
                //            employee_details.Add(new clsinfo { fieldname = "Address", value = q_emp_profile.First().EmployeePresentAddress });
                //            employee_details.Add(new clsinfo { fieldname = "TIN", value = q_emp_profile.First().TIN });
                //            employee_details.Add(new clsinfo { fieldname = "SSS", value = q_emp_profile.First().SSS });
                //            employee_details.Add(new clsinfo { fieldname = "HDMF", value = q_emp_profile.First().HDMF });
                //            employee_details.Add(new clsinfo { fieldname = "PHILHEALTH", value = q_emp_profile.First().PHILHEALTH });
                //            ProMst.Add(new clsProfileMst { userDtlHeader = "Employee Information", userDtl = employee_details });

                //            employment_details.Add(new clsinfo { fieldname = "Date Hired", value = DateTime.Parse(q_emp_profile.First().DATEHIRED.ToString()).ToShortDateString() });
                //            employment_details.Add(new clsinfo { fieldname = "Years Employed", value = hyr.ToString() + " Year(s) & " + hMonth + " Month(s)"}); // + hdays + " Day(s)" });
                //            employment_details.Add(new clsinfo { fieldname = "SBU", value = q_emp_profile.First().CorporateName });
                //            employment_details.Add(new clsinfo { fieldname = "Branch", value = q_emp_profile.First().BRANCHNAME });
                //            employment_details.Add(new clsinfo { fieldname = "Position", value = q_emp_profile.First().POSITIONNAME });
                //            employment_details.Add(new clsinfo { fieldname = "Department", value = q_emp_profile.First().DEPARTMENTNAME });
                //            employment_details.Add(new clsinfo { fieldname = "Section", value = q_emp_profile.First().SECTIONNAME });
                //            employment_details.Add(new clsinfo { fieldname = "WTax Status", value = q_emp_profile.First().WTAXSTATUS });
                //            ProMst.Add(new clsProfileMst { userDtlHeader = "Employment Details", userDtl = employment_details });

                //            //Immediate_details.Add(new clsinfo { fieldname = "I.S. Employee ID: ", value = q_emp_profile.First().ImmediateID });
                //            Immediate_details.Add(new clsinfo { fieldname = "Name", value = q_emp_profile.First().ImmediateName });
                //            Immediate_details.Add(new clsinfo { fieldname = "Position", value = q_emp_profile.First().ImmediatePosition });
                //            Immediate_details.Add(new clsinfo { fieldname = "Email Address", value = q_emp_profile.First().ImmediateEmail });
                //            ProMst.Add(new clsProfileMst { userDtlHeader = "Immediate Superior", userDtl = Immediate_details });
                //            mypro.ProMst = ProMst;
                //        }




                return mypro;
            }
            catch (Exception ex)
            {
                //mypro.haserror = true;
                //mypro.error_message = ex.Message;
                return mypro;
            }
        }

    
        /// <summary>
        /// Get all employee
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="size">sire per row</param>
        /// <returns>IEnumerable</returns>
        [HttpGet]
        [ActionName("get-all-employee")]
        public EmployeeModel GetAllEmployee(int page,int size) {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();

                    int ipage = 1;

                    if (page == 0)
                    {
                        ipage = 1;//set default page to 1 if the page is equal to 0
                    }
                    else {
                        ipage = page;
                    }

                    int total = db.core_employee.Where(p => p.type != "SEP").Select(p => p.empl_id).Count();

                    int pageSize = size; // set your page size, which is number of records per page
 
                    int skip = pageSize * (ipage - 1); // skip the record

                    bool canPage = skip < total;// bool

                    int pageCount=total / size;


                    var q_emplist = (from aa in db.core_employee
                                     where aa.type != "SEP"
                                     select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                    foreach (var row in q_emplist)
                    {
                        empAccount.Add(new EmployeeAccount
                        {
                            EmpID = row.empl_id,
                            SBU = row.corporate,
                            EmpName = row.employeename2,
                            EmpEmail = row.emailadd,
                            Position = row.positionname,
                            UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                            ContactNo=row.employeepresentcontact ?? string.Empty 
                        });
                    }


                    empModel.empAccount = empAccount;
                    empModel.ipage = page;
                    empModel.canPage = canPage;
                    empModel.pageCount = pageCount;
                    empModel.hasError = false;
                

              
              
                return empModel;
            }
            catch (Exception ex) {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }

        /// <summary>
        /// Search Employee
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="size">sire per row</param>
        /// <param name="searchParam">search parameter</param>
        /// <returns>return list of search result</returns>
        [HttpGet]
        [ActionName("get-search-employee")]
        public EmployeeModel GetSearchEmployee(int page,int size,string searchParam)
        {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();


              
                    int ipage = 1;

                    if (page == 0)
                    {
                        ipage = 1;//set default page to 1 if the page is equal to 0
                    }
                    else
                    {
                        ipage = page;
                    }

                int total = db.core_employee.Where(p => p.type != "SEP").Select(p => p.empl_id).Count();

                int pageSize = size; // set your page size, which is number of records per page

                    int skip = pageSize * (ipage - 1); // skip the record

                    bool canPage = skip < total;// bool

                    int pageCount = total / size;

             
                var q_emplist = (from aa in db.core_employee
                                 where aa.type != "SEP" && (aa.employeename2.Contains(searchParam) || aa.empl_id.Contains(searchParam) || aa.emailadd.Contains(searchParam))
                                 select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                foreach (var row in q_emplist)
                {
                    empAccount.Add(new EmployeeAccount
                    {
                        EmpID = row.empl_id,
                        SBU = row.corporate,
                        EmpName = row.employeename2,
                        EmpEmail = row.emailadd,
                        Position = row.positionname,
                        UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                        ContactNo = row.employeepresentcontact ?? string.Empty
                    });
                }


                empModel.empAccount = empAccount;
                    empModel.ipage = page;
                    empModel.canPage = canPage;
                    empModel.pageCount = pageCount;
                    empModel.hasError = false;
                



                return empModel;
            }
            catch (Exception ex)
            {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }

        /// <summary>
        /// GEt appmenu
        /// </summary>
        /// <param name="sysid">system ID</param>
        /// <param name="userid">employee ID</param>
        /// <returns>list of menu</returns>
        [HttpGet]
        [ActionName("get-app-menu")]
        public AppsMenuModel GetAppMenu(int sysid, int userid)
        {
            AccountRights accountRights = new AccountRights();
            return accountRights.getAppsMenu(sysid, userid);
           
           
        }

        /// <summary>
        /// GEt appmenu
        /// </summary>
        /// <param name="sysid">system ID</param>
        /// <param name="userid">employee ID</param>
        /// <returns>list of menu</returns>
        [HttpGet]
        [ActionName("get-app-menu-v2")]
        public HttpResponseMessage GetAppMenuv2(int sysId, int userId)
        {
            try
            {
                AccountRights accountRights = new AccountRights();

                return Request.CreateResponse(HttpStatusCode.OK, accountRights.getAppsMenuV2(sysId, userId));
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //[HttpGet]
        //[ActionName("get-employee-benefits")]
        //public BenefitsModel getEmpBenefits(string empid)
        //{
        //    Benefits benefits = new Benefits();
        //    return benefits.getBenefitsModel(empid);
        //}

        public HttpResponseMessage GetUserProfileMaster(int id)
        {
            AccountRights n = new AccountRights();
            UserMasterModel userMasterModel=new UserMasterModel();
            List<UserSystemModel> userSys = new List<UserSystemModel>();
            var coreUsers = db.core_users.Where(aa => aa.userId == id).FirstOrDefault();

            var qSysList = from aa in db.core_appsystem
                           where aa.isactive == true && aa.showwebapps == true
                           select aa;


            foreach (var row in qSysList)
                userSys.Add(new UserSystemModel
                {

                    SYSID = row.sysId,
                    SYSNAME = row.sysname,
                    IsCheck = n.IsSystemGrant(row.sysId, id),
                    UserMenu = n.ListUserMenu(id, row.sysId)
                });
            

            userMasterModel.coreUsers = coreUsers;
            userMasterModel.userSys = userSys;

            
            return Request.CreateResponse(HttpStatusCode.OK, userMasterModel);
          
        }

       

        [HttpGet]
        [ActionName("validate-emp-email")]
        public IHttpActionResult ValidateEmployeeEmail(string email)
        {
            try
            {
                AccountRights n = new AccountRights();
               return Json(new { hasError = false, data = n.ValidateEmployeeEmail(email) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
                db.Dispose();
                db.Dispose();
               // iedb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
