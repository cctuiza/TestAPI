using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.SQLData;
using PaloUpAPI.ActionFilters;
using PaloUpAPI.Models;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
 

namespace PaloUpAPI.Controllers
{
    [AuthorizationRequired]
    [Compress]
    public class UserController : ApiController
    {

        PALOUPEntities db = new PALOUPEntities();
     
        Library phpLib = new Library();
        clsprocedure procedure = new clsprocedure();


        [HttpGet]
        public HttpResponseMessage isUserpassEmpty(string email)
        {

            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qUser = db.core_users.Where(m => m.username == email).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qUser);

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }


        [HttpPut]
        public HttpResponseMessage updateNickname(object param)
        {
            JObject jResult = null;
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                jResult = JObject.Parse(param.ToString());
                string email = jResult["email"].ToString();
                string nickname = jResult["nickname"].ToString();

                var qUser = db.core_users.Where(m => m.emailaddress == email).FirstOrDefault();
                qUser.nickname = nickname;
                db.SaveChanges();


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage updatePassword(object param)
        {
            JObject jResult = null;

            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                jResult = JObject.Parse(param.ToString());
                int userId = Convert.ToInt32(jResult["userId"].ToString());
                string password = jResult["userpassword"].ToString();

                var qUser = db.core_users.Where(m => m.userId == userId).FirstOrDefault();
                qUser.userpass = phpLib.generatePassword(password).ToString();
                db.SaveChanges();


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage validatemobile(object param)
        {
            JObject jResult = null;

            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                jResult = JObject.Parse(param.ToString());
                string email = jResult["email"].ToString(); ;
                string mobile = jResult["mobile"].ToString();
                string confirmationcode = procedure.GenerateCode();

                core_confirmation n = new core_confirmation()
                {
                    confirmtype = "S",
                    create_date = DateTime.Now,
                    confirmcode = confirmationcode,
                    referencecode = mobile,
                    isconfirm = false,
                    expirydate = DateTime.Now.AddMinutes(10)

                };

                db.core_confirmation.Add(n);
                db.SaveChanges();

                SMS.SendSms(mobile, confirmationcode + " is your fast account confirmation code, please enter within 10 minutes.", "All", null);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage confirmMobile(object param)
        {
            JObject jResult = null;

            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                jResult = JObject.Parse(param.ToString());
                string email = jResult["email"].ToString();
                string mobile = jResult["mobile"].ToString();
                string confirmationcode = jResult["confirmationcode"].ToString();

                var qconfirm = db.core_confirmation.Where(m => m.referencecode == mobile && m.confirmcode == confirmationcode && m.expirydate > DateTime.Now);
                if (qconfirm.Count() > 0)
                {
                    qconfirm.FirstOrDefault().isconfirm = true;
                    db.SaveChanges();

                    var qUser = db.core_users.Where(m => m.emailaddress == email).FirstOrDefault();
                    qUser.mobileno = mobile;
                    db.SaveChanges();

                    //update paloup sytem member details
                    //update toolgater
                    var qToolgater = db.mifs_issues.Where(m => m.raisedbyemail == email).ToList();
                    foreach (var row in qToolgater)
                    {
                        row.raisedbyphone = mobile;
                        db.SaveChanges();

                    }


                    //update responsible
                    var qResponsible = db.mifs_issues.Where(m => m.responemail == email).ToList();
                    foreach (var row in qResponsible)
                    {
                        row.responphone = mobile;
                        db.SaveChanges();
                    }

                    //update member
                    var qCollaborator = db.mifs_members.Where(m => m.emailaddress == email).ToList();
                    foreach (var row in qCollaborator)
                    {
                        row.mobileno = mobile;
                        db.SaveChanges();
                    }

                    //update core employee
                    var qCoreEmployee = db.core_employee.Where(m => m.emailadd == email).ToList();
                    foreach (var row in qCoreEmployee)
                    {
                        row.homecontact = mobile;
                        db.SaveChanges();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid Confirmation code!");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-all-users")]
        public HttpResponseMessage GetAllUsers(int page, int size)
        {

            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_users.Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;


                var qUsers = db.core_users.OrderBy(m => m.firstname)
                    .Skip(skip)
                    .Take(pageSize);

                return Request.CreateResponse(HttpStatusCode.OK, qUsers);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-search-users")]
        public HttpResponseMessage GetSearchUsers(int page, int size, string searchParam)
        {

            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_users.Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;


                //var qUsers = db.core_users.OrderBy(m => m.firstname)
                //    .Where(m => searchParam.Contains(m.firstname) || searchParam.Contains(m.lastname) || searchParam.Contains(m.middlename) ||
                //    searchParam.Contains(m.userId.ToString()) || searchParam.Contains(m.username) || searchParam.Contains(m.emplId) || searchParam.Contains(m.emailaddress))
                //    .Skip(skip)
                //    .Take(pageSize);

                var qUsers = db.core_users.OrderBy(m => m.firstname)
                  .Where(m => m.firstname.Contains(searchParam) || m.lastname.Contains(searchParam)  || m.middlename.Contains(searchParam)  ||
                    m.username.Contains(searchParam) || m.emplId.Contains(searchParam) || m.emailaddress.Contains(searchParam) || m.nickname.Contains(searchParam))
                  .Skip(skip)
                  .Take(pageSize);

                return Request.CreateResponse(HttpStatusCode.OK, qUsers);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// get employee list that not in core user table
        /// </summary>
        /// <returns>IEnumerable<core_employee></returns>
        [HttpGet]
        public HttpResponseMessage GetEmployeeListNotUser(string search)
        {
            try
            {
                int ipage = 1;
                int skip = 200 * (ipage - 1); // skip the record

                if (string.IsNullOrEmpty(search) || search == "undefined")
                {
                    var qcoreEmployee = db.core_vEmployeeNotUser.ToList().Skip(skip).Take(200);
                    return Request.CreateResponse(HttpStatusCode.OK, qcoreEmployee);
                }
                else
                {
                    var qcoreEmployee = db.core_vEmployeeNotUser.Where(m => search.Contains(m.empl_id)
                    || search.Contains(m.fname) || search.Contains(m.lname) || search.Contains(m.emailadd))
                        .ToList().Skip(skip).Take(200);
                    return Request.CreateResponse(HttpStatusCode.OK, qcoreEmployee);
                }


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [ActionName("delete-user")]
        public HttpResponseMessage DeleteUser(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                //Delete User Menu
         
                db.core_appmenuuser.RemoveRange(db.core_appmenuuser.Where(m => m.userId == id));

                //Delete User System
                db.core_appsystemuser.RemoveRange(db.core_appsystemuser.Where(m => m.userId == id));

                //Delete User Warehouse
                db.core_userswarehouse.RemoveRange(db.core_userswarehouse.Where(m => m.userId == id));

                //Delete rms
                db.rms_useranalysisgroupaccess.RemoveRange(db.rms_useranalysisgroupaccess.Where(m => m.userId == id));

                //Delete PISM

                //other
                db.wms_userserverwarehouse.RemoveRange(db.wms_userserverwarehouse.Where(m => m.userId == id));

                //Delete User Profile
                db.core_users.Remove(db.core_users.Where(m => m.userId == id).FirstOrDefault());
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage GetUserProfileMaster(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                AccountRights n = new AccountRights();
                UserMasterModel userMasterModel = new UserMasterModel();
                List<UserSystemModel> userSys = new List<UserSystemModel>();
                var coreUsers = db.core_users.Where(aa => aa.userId == id);

                var qSysList = from aa in db.core_appsystem
                               where aa.isactive == true && aa.showwebapps == true
                               select aa;




                if (coreUsers.Count() > 0)
                {
                    userMasterModel.coreUsers = coreUsers.FirstOrDefault();
                }
               


                foreach (var row in qSysList)
                    userSys.Add(new UserSystemModel
                    {

                        SYSID = row.sysId,
                        SYSNAME = row.sysname,
                        IsCheck = n.IsSystemGrant(row.sysId, id),
                        UserMenu = n.ListUserMenu(id, row.sysId)
                    });
                userMasterModel.userSys = userSys;


                return Request.CreateResponse(HttpStatusCode.OK, userMasterModel);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Get user warehouse
        /// </summary>
        /// <param name="userId">userId</param>
        /// <param name="sysId">system Id</param>
        /// <returns>List<UserBranchModel></returns>
        [HttpGet]
        public HttpResponseMessage getUserWarehouse(int userId, int sysId)
        {
            UserWarehouse userwarehouse = new UserWarehouse();
            List<UserBranchModel> userBranchModel = new List<UserBranchModel>();

            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var qcoreBranch = db.core_branchmst.Where(m => m.withpism == 1).ToList();
                var qcoreUserWarehouse = db.core_userswarehouse.Where(m => m.sysId == sysId && m.userId == userId).ToList();

                foreach (var row in qcoreBranch)
                {
                    bool isdef = false;
                    var coreuser = qcoreUserWarehouse.Where(m => m.brancode == row.brancode);
                    if(coreuser.Count() != 0)
                    {
                        isdef = coreuser.FirstOrDefault().isdefault;
                    }
                    userBranchModel.Add(new UserBranchModel
                    {
                        brancode = row.brancode,
                        branname = row.branname,
                        branaddress = row.address,
                        isCheck = qcoreUserWarehouse.Where(m => m.brancode == row.brancode).Count() == 1 ? true : false, //check if the branch is exist in user warehouse table
                        isdefault = isdef
                    });

                }

                userwarehouse.userId = userId;
                userwarehouse.sysId = sysId;
                userwarehouse.userBranch = userBranchModel;

                return Request.CreateResponse(HttpStatusCode.OK, userwarehouse);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Set user Warehouse
        /// </summary>
        /// <param name="param">UserWarehouse Object</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage setUserWarehouse(UserWarehouse param)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                //delete user warehouse where userId and sysId is equal to param
                db.core_userswarehouse.RemoveRange(db.core_userswarehouse.Where(aa => aa.userId == param.userId && aa.sysId == param.sysId));
                db.SaveChanges();


                foreach (var row in param.userBranch)
                {
                    //only selected warehouse were save in userwarehouse table
                    if (row.isCheck == true)
                    {
                        core_userswarehouse userwarehouse = new core_userswarehouse
                        {
                            sysId = param.sysId,
                            userId = param.userId,
                            brancode = row.brancode,
                            create_date = DateTime.Now,
                            created_by = param.created_by,
                            isdefault = row.isdefault 
                        };

                        db.core_userswarehouse.Add(userwarehouse);
                        db.SaveChanges();
                    }
                }


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage UpdateUserPass(core_users user)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;

                var quser = db.core_users.Where(m => m.userId == user.userId).FirstOrDefault();
                quser.userpass = phpLib.generatePassword(user.userpass).ToString();
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, user.userpass);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage SaveUserChanges(UserMasterModel userModel)
        {
            try
            {
                var coreUsers = userModel.coreUsers;

                string hashcode = string.Empty;

                //check if the menu is exist set isNew val to true
                if (db.core_users.Where(aa => aa.userId == coreUsers.userId).Count() == 0)
                {
                    coreUsers.create_date = DateTime.Now;
                    coreUsers.userpass= phpLib.generatePassword(userModel.coreUsers.userpass).ToString();
                    db.Entry(coreUsers).State = EntityState.Added;
                    db.SaveChanges();

                }
                else
                {
                   
                    db.Entry(coreUsers).State = EntityState.Modified;
                    db.SaveChanges();
                }


                //set user hashcode
                hashcode = phpLib.generatePassword(coreUsers.userId.ToString()).ToString();
                var qUser = db.core_users.Where(m => m.userId == coreUsers.userId).FirstOrDefault();
                qUser.hashcode = hashcode;
                db.SaveChanges();


                /*
                 * Start Clean
                 * Clean Apps_User_System
                 * Clean Apps_User_Menu
                 */

                db.core_appsystemuser.RemoveRange(db.core_appsystemuser.Where(aa => aa.userId == coreUsers.userId));
                db.core_appmenuuser.RemoveRange(db.core_appmenuuser.Where(aa => aa.userId == coreUsers.userId));
                db.SaveChanges();

                foreach (var sysrw in userModel.userSys)
                {
                    if (sysrw.IsCheck == true)
                    {


                        db.Sproc_CoreSetAppSystemUser(
                             sysrw.SYSID,
                            coreUsers.userId,
                            coreUsers.created_by);

                        foreach (var menurw in sysrw.UserMenu)
                        {
                            if (menurw.canOpen == true)
                            {
                                db.Sproc_CoreSetAppMenuUser(
                                    menurw.menuId,
                                    coreUsers.userId,
                                    menurw.canAdd,
                                    menurw.canEdit,
                                    menurw.canDelete,
                                    menurw.canView,
                                    menurw.canOpen);
                            }
                        }
                    }
                }


                /*
                 * Clean first and save user sites
                 */
                //db.Sproc_PismDeleteUserWarehouse(coreUsers.userId);

                //foreach (var siterw in userModel.userSites) {
                //    if (siterw.isCheck==true)
                //    {
                //        db.Sproc_Save_UserWarehouse(userModel.USERID,
                //            siterw.SiteCode, siterw.isCheck == true ? byte.Parse("2") : byte.Parse("1"));
                //    }
                //}
                //   db.Sproc_PismUpdateUserWarehouse(userModel.USERID);


                /*
                 * End Update User Details
                 */

                return Request.CreateResponse(HttpStatusCode.OK, "Save Complete...");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        /// <summary>
        /// get-user-menu
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>JSON</returns>
        [HttpGet]
        [ActionName("get-user-menu")]
        public IHttpActionResult getUserMenu(int id)
        {
            try
            {
                UserData n = new UserData();

                return Json(new { hasError = false, data = n.getUser(id) });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

    

        [HttpGet]
        [ActionName("get-user-sites")]
        public HttpResponseMessage getUserSites(int userId, int sysId)
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                var qSites = (from aa in db.core_userswarehouse
                              join bb in db.core_branchmst on aa.brancode equals bb.brancode
                              where aa.sysId == sysId && aa.userId == userId
                              select new { aa.brancode, bb.branname });


                return Request.CreateResponse(HttpStatusCode.OK, qSites);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-user-menu-list")]
        public HttpResponseMessage getUserMenuList(string id)
        {
            object qMenulist = null;
            try
            {
                UserData n = new UserData();

                string[] newParams = id.Split(',');

                string menucode = newParams[0].ToString();
                int userId = int.Parse(newParams[1].ToString());

                qMenulist = db.core_UserMenuList.Where(wr => wr.userId == userId && wr.menucode == menucode).FirstOrDefault();


                return Request.CreateResponse(HttpStatusCode.OK, qMenulist);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-alias")]
        public HttpResponseMessage getAlias(int id)
        {
            try
            {
                UserData n = new UserData();

                return Request.CreateResponse(HttpStatusCode.OK, n.getAlias(id));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ActionName("post-alias")]
        public IHttpActionResult postAlias(int id, string alias)
        {
            try
            {
                UserData n = new UserData();
                n.postAlias(id, alias);
                return Json(new { hasError = false });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage SyncUserList()
        {
            try
            {
                db.Sproc_SyncEmployee();

                return Request.CreateResponse(HttpStatusCode.OK, "User list is refreshed");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage getUserProfile(string id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var userProfile = db.core_users.Where(m => m.emplId == id).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, userProfile);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage getUserProfilebyEmail(string email)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var userProfile = db.core_users.Where(m => m.emailaddress == email).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, userProfile);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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