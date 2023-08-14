using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.Models;
using PaloUpAPI.SQLData;

namespace PaloUpAPI.Controllers
{
    public class AccessController : ApiController
    {
        Response response = new Response();
        TokenGenerator tokenGenerator = new TokenGenerator();
        clsprocedure procedure = new clsprocedure();
        //connection to databae

        PALOUPEntities db = new PALOUPEntities();
       // ItsekData itsekData = new ItsekData();
        AccountRights accountRights = new AccountRights();
        #region "Corporate User"

        [HttpGet]
        [Route("api/GetUser")]
        public HttpResponseMessage GetUser(string loginname, string password)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                Library phpLib = new Library();

                string UserType = string.Empty;
                string error_message = string.Empty;
                string UserName = string.Empty;
                string empid = string.Empty;
                string IncPassword = string.Empty;
                response.status = "FAILURE";

                //check core user if the alias is apply in login parameter
                var qUserAllias = db.core_users.Where(aa => aa.nickname == loginname);
                //check the password is equal to debug password
                var coreSystem = db.core_system.Where(wr => wr.debugpassword == password);

                //check if the alias is exist
                if (qUserAllias.Count() > 0) // set userid equla to useremail if the alias is true
                {
                    loginname = qUserAllias.First().username;//set login name to username
                }


                //check if debug password has been apply
                if (coreSystem.Count() > 0)
                {
                    IncPassword = db.core_users.Where(m => m.username == loginname).FirstOrDefault().userpass;
                }
                else
                {
                    IncPassword = phpLib.generatePassword(password).ToString();
                }

                //select user table
                var qUserProfile = db.core_users.Where(m => m.username == loginname & m.userpass == IncPassword)
                    .Select(m => new { m.userId, m.username, m.userrole, m.lastname, m.firstname, m.middlename, m.usertype, m.emplId, m.emailaddress, m.mobileno, m.hashcode });

                //check if the account is exist in user table
                if (qUserProfile.Count() > 0)
                {
                    response.objParam1 = qUserProfile.FirstOrDefault();
                    response.stringParam2 = phpLib.generatePassword(qUserProfile.FirstOrDefault().userId.ToString()).ToString();
                    response.status = "SUCCESS";
                    response.stringParam1 = tokenGenerator.Encrypt(loginname) + ":" + tokenGenerator.Encrypt(password);
                }
                else
                {
                    response.status = "FAILURE";
                    response.message = "Invalid Email or Username!";


                }

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

        }


        [HttpPost]
        [Route("api/Access/LoginUser")]
        public HttpResponseMessage LoginUser(Login param)
        {

            Library phpLib = new Library();

            string jsonString = param.ToString();
            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                string loginname = param.loginname;
                string password = param.password;
                string UserType = string.Empty;
                string error_message = string.Empty;
                string UserName = string.Empty;
                string empid = string.Empty;
                string IncPassword = string.Empty;
                response.status = "FAILURE";

                //check core user if the alias is apply in login parameter
                var qUserAllias = db.core_users.Where(aa => aa.nickname == loginname);
                //check the password is equal to debug password || updated admin list
                var coreSystem = db.core_system.Where(wr => wr.debugpassword == password && wr.Id >= 1);

                //check if the alias is exist
                if (qUserAllias.Count() > 0) // set userid equla to useremail if the alias is true
                {
                    loginname = qUserAllias.First().username;//set login name to username
                }


                //check if debug password has been apply
                if (coreSystem.Count() > 0)
                {
                    IncPassword = db.core_users.Where(m => m.username == loginname && m.status == "A").FirstOrDefault().userpass;
                }
                else
                {
                    IncPassword = phpLib.generatePassword(password).ToString();
                
                }

                //select user table
                var qUserProfile = db.core_users.Where(m => m.username == loginname & m.userpass == IncPassword && m.status == "A")
                    .Select(m => new
                    {
                        m.userId,
                        m.username,
                        m.userrole,
                        m.lastname,
                        m.firstname,
                        m.middlename,
                        m.usertype,
                        emplId =  db.core_employee.Where(mm=>mm.emailadd == m.emailaddress && mm.type != "SEP").FirstOrDefault().empl_id,
                        emailaddress = db.core_employee.Where(mm => mm.emailadd == m.emailaddress && mm.type != "SEP").FirstOrDefault().emailadd,
                        m.mobileno,
                        m.hashcode,
                        m.status,
                        departmentcode = db.core_employee.Where(r=> r.empl_id==m.emplId).FirstOrDefault().department ?? ""
                    });



                //check if the account is exist in user table
                if (qUserProfile.Count() > 0)
                {
                    if (qUserProfile.FirstOrDefault().status == "I" && qUserProfile.FirstOrDefault().userrole == "USER")
                    {
                        response.message = "Invalid to sign-in this account is inactive!";
                        response.status = "FAILURE";
                        return Request.CreateResponse(HttpStatusCode.OK, response);
                    }

                    response.objParam1 = qUserProfile.FirstOrDefault();
                    response.stringParam2 = phpLib.generatePassword(qUserProfile.FirstOrDefault().userId.ToString()).ToString();
                    response.status = "SUCCESS";
                    response.stringParam1 = tokenGenerator.Encrypt(loginname) + ":" + tokenGenerator.Encrypt(password);
                }
                else
                {
                    var checker = db.core_users.Where(m => m.username == loginname && m.status == "A");
                    //return error
                    if (checker.Count() == 0)
                    {

                        response.message = "Invalid Username!";
                    }
                    else
                    {
                        response.message = "Invalid Password!";
                    }
                    response.status = "FAILURE";
                }


                if (param.syscode == "ITsek")
                {
                //    response.objParam2 = itsekData.getMyItsekbranch(qUserProfile.First().userId);
                    response.objParam3 = accountRights.getAppsMenuV2(22, qUserProfile.First().userId);
                }
                else if (param.syscode == "AMOA")
                {
                    response.objParam2 = accountRights.getAppsMenuV2(23, qUserProfile.First().userId);
                }
                else if (param.syscode == "AQUILA")
                {
                    response.objParam2 = accountRights.getAppsMenuV2(25, qUserProfile.First().userId);
                }

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

        }

        [HttpPost]
        [Route("api/AdminPinAccess")]
        public HttpResponseMessage AdminPinAccess(dynamic val)
        {
            string pinCode = val.pinCode;
            bool isResult = false;

            try
            {
                var modAdminAccess = db.core_system.Where(m => m.mobadminpincode == pinCode);

                if(modAdminAccess.Count()> 0)
                {
                    isResult = true;
                }

                return Request.CreateResponse(HttpStatusCode.OK, isResult);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [Route("api/ResetPassword")]
        public HttpResponseMessage ResetPassword(dynamic val)
        {
            string uuId = val.uuId;
            string uPassword = val.password;
            Library phpLib = new Library();
            MailParamsModel mailParam = new MailParamsModel();
            string[] param = new string[20];//declare email parameter holder
            try
            {

                var coreLink = db.core_link.Where(m => m.uuid == uuId).FirstOrDefault();

                if (coreLink.expirydate < DateTime.Now)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Forgot password session expiry!");
                }
                else if (coreLink.isused == true)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Forgot password session posted!");
                }

                var coreUser = db.core_users.Where(m => m.emailaddress == coreLink.recipientemail.ToString()).FirstOrDefault();
                coreUser.userpass = phpLib.generatePassword(uPassword).ToString();
                coreLink.isused = true;
                db.SaveChanges();


                param[0] = coreUser.firstname + " " + coreUser.lastname;
                param[1] = coreLink.recipientemail;
                param[2] = uPassword;

                //set mailparam set value
                mailParam.recipientsMail = coreLink.recipientemail;
                mailParam.mailTemplatePath = "mailTemplate//account.reset.password.complete.html";
                mailParam.sendername = "CORE";
                mailParam.subject = "FSC APPS Change Password";
                mailParam.param = param;
                mailParam.createdby = "Core";
                procedure.EmailNotification(mailParam);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        /// <summary>
        /// Forgot password api
        /// </summary>
        /// <param name="id">Email Addredd</param>
        /// <returns>Http Response message</returns>
        /// 
        [HttpPost]
        [Route("api/Access/ForgotPass")]
        public HttpResponseMessage ForgotPass(dynamic val)
        {
            MailParamsModel mailParam = new MailParamsModel();
            string[] param = new string[20];//declare email parameter holder
            string link_id = Guid.NewGuid().ToString();
            string email = val.uemail;
            string appLink = val.appLink;

            try
            {

                var userAccount = db.core_users.Where(m => m.emailaddress == email && m.status == "A").ToList();

                //check email in repo
                if (userAccount.Count() == 0)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "This Email is not found please contact your ADMINISTRATOR!");

                //add link history
                core_link coreLink = new core_link
                {
                    linkId = 0,
                    uuid = link_id,
                    description = "Forgot password",
                    expirydate = DateTime.Now.AddHours(24),
                    isused = false,
                    recipientemail = email,
                    sourceapplication = appLink,
                    createdate = DateTime.Now
                };

                db.core_link.Add(coreLink);
                db.SaveChanges();


                param[0] = userAccount.First().firstname + " " + userAccount.First().lastname;
                param[1] = appLink;
                param[2] = link_id;

                //set mailparam set value
                mailParam.recipientsMail = val.uemail;
                mailParam.mailTemplatePath = "mailTemplate//account.forgot.password.html";
                mailParam.sendername = "CORE";
                mailParam.subject = "FSC APPS Reset Password";
                mailParam.param = param;
                mailParam.createdby = "Core";
                procedure.EmailNotification(mailParam);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


 

        #endregion

        #region "Aquila User"

        [HttpGet]
        [Route("get-user-access")]
        public HttpResponseMessage getUserWarehouseAccess(string param)
        {
            try
            {
                //Login
                TouchmoveLoginObj model = new TouchmoveLoginObj();
                List<WarehoueCodeVm> list = new List<WarehoueCodeVm>();
                if (!String.IsNullOrEmpty(param))
                {
                    var userInfo = db.core_users.Where(m => m.nickname == param || m.username == param).ToList();
                    int userid = 0;
                    if (userInfo.Count() > 0)
                    {
                        userid = userInfo.FirstOrDefault().userId;
                    }
                    else
                    {
                        model.message = "Please make sure you inputted the correct username";
                        model.success = false;
                        model.list = list;
                        return Request.CreateResponse(HttpStatusCode.OK, model);
                    }

                    //db.Configuration.LazyLoadingEnabled = false;
                    //var userWarehouseList = db.wms_userserverwarehouse.Where(m => m.userId == userid && m.status == "A").ToList().OrderBy(m => m.wms_serverwarehouses.wms_servers.servername).ThenBy(m => m.wms_serverwarehouses.warehousecode);
                    var userWarehouseList = db.wms_userserverwarehouse.Where(m => m.userId == userid && m.status == "A").ToList();
                    foreach (var item in userWarehouseList)
                    {
                        var info = db.wms_serverwarehouses.Where(m => m.serverwarehouseId == item.serverwarehouseId).FirstOrDefault();
                        list.Add(new WarehoueCodeVm
                        {
                            serverwarehouseId = item.serverwarehouseId,
                            servername = info.wms_servers.servername,
                            warehousecode = info.warehousecode,
                            serverId = info.wms_servers.serverId,
                            storageidentity = info.storageidentity,
                            description = item.description,
                            api = info.wms_servers.webaddress
                        });
                    }
                    //var qServerWarehouse = db.wms_serverwarehouses.Where(m => warehouseId.Contains(m.serverwarehouseId)).ToList();

                    model.message = list.Count() == 0 ? "There are no warehouse connected to your username" : "Please choose your warehouse";
                    model.success = true;
                }
                else
                {
                    model.message = "Please make sure you inputted the correct username";
                    model.success = false;
                }

                model.list = list.OrderBy(m => m.servername).ThenBy(m => m.warehousecode).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        #endregion

        public class DropdownInt
        {
            public int value { get; set; }
            public string text { get; set; }
        }

        public class WarehoueCodeVm
        {
            public int serverwarehouseId { get; set; }
            public int serverId { get; set; }
            public string servername { get; set; }
            public string warehousecode { get; set; }
            public string description { get; set; }
            public string storageidentity { get; set; }
            public string api { get; set; }
        }

        public class TouchmoveLoginObj
        {
            public bool success { get; set; }
            public string message { get; set; }
            public List<WarehoueCodeVm> list { get; set; }
        }
        #region "dispose"

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion


    }
}
