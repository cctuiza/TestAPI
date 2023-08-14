using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;

namespace PaloUpAPI.SQLData
{
    public class AccountRights
    {
     
       clsprocedure procedure = new clsprocedure();
        
        public AppsMenuModel getAppsMenu(int sysid, int userid)
        {
            sysid = 19;
            AppsMenuModel appsMenuModel = new AppsMenuModel();
            
            try
            {
                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {
                    appsMenuModel.hasError = false;

                   
                    appsMenuModel.MenuList = dbcontext.core_UserMenuList
                        .Where(aa => aa.userId == userid && aa.sysId == sysid).ToList();
                }
                return appsMenuModel;
            }
            catch (Exception ex)
            {
                appsMenuModel.hasError = true;
                appsMenuModel.errorMsg = ex.Message;
                return appsMenuModel;
            }
        }

        public List<MenuModel> getAppsMenuV2(int sysid, int userid)
        {
            sysid = 19;
            //declare parent menu list
            List<MenuModel> parentMenuModel = new List<MenuModel>();

            try
            {
                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {
                    dbcontext.Configuration.LazyLoadingEnabled = false;

                    var userProfile = dbcontext.core_users.Where(m => m.userId == userid).FirstOrDefault() ;

                    dynamic listMenu = null;

                    //if the sysId = 1 (admin)
                    if (sysid == 1)
                    {
                        if (userProfile.userrole == "ADMIN")
                        {
                            //get all menu if the userrole is equal to admin
                            var parentMenu = dbcontext.core_appmenu
                                      .Where(m => m.sysId == sysid && m.isactive==true)
                                      .OrderBy(m => m.menuorder)
                                      .ToList()
                                      .Select(m=> new
                                      {
                                          parentId = m.menuId,
                                          parentName = m.menuname,
                                          parentmenuorder = m.menuorder,
                                          parenticon = m.classicon,
                                          m.sysId,
                                          m.menucode
                                      });

                            listMenu = parentMenu;
                        }
                        else
                        {
                            //get specific menu 
                            var parentMenu = dbcontext.core_vAdminMenu
                                     .Where(m => m.userId == userid && m.sysId == sysid)
                                     .OrderBy(m => m.parentmenuorder)
                                     .ToList();

                            listMenu = parentMenu;

                        }

                    }
                    else//get if the sysId not equal to 1
                    {
                        if (userProfile.userrole == "ADMIN")
                        {
                            //get all menu if the userrole is equal to admin
                            var parentMenu = dbcontext.core_appmenu
                                      .Where(m => m.sysId == sysid && m.isactive == true && m.prtmenucode=="0")
                                      .OrderBy(m =>  m.menuorder)
                                      .ToList()
                                      .Select(m => new
                                      {
                                          parentId = m.menuId,
                                          parentName = m.menuname,
                                          parentmenuorder = m.menuorder,
                                          parenticon = m.classicon,
                                          m.sysId,
                                          m.menucode
                                      });

                            listMenu = parentMenu;

                        }
                        else
                        {
                            //get specific menu 
                            var parentMenu = dbcontext.core_vParentMenu
                                  .Where(m => m.userId == userid && m.sysId == sysid)
                                  .OrderBy(m => m.parentmenuorder)
                                  .ToList();

                            listMenu = parentMenu;
                        }
                      
                    }


                    //get child menu base on parent menu value
                    foreach (var parentRow in listMenu)
                    {
                        string parentMenuID = parentRow.menucode;
                        //declare child menu list
                        List<MenuModel> childMenuModel = new List<MenuModel>();

                        if (userProfile.userrole == "ADMIN")
                        {
                            //get child menu from menulist where parentid equal in the loop
                            //   var childMenuParent = dbcontext.core_UserMenuList
                            //.Where(aa => aa.userId == userid && aa.sysId == sysid && aa.prtmenucode == parentMenuID)
                            //.Select(aa => new { aa.menuId, aa.menucode, aa.menuname, aa.menuorder, aa.ChildIcon })
                            //.Distinct()
                            //.OrderBy(or => or.menuorder).ToList();

                            var childMenuParent = dbcontext.core_appmenu
                        .Where(aa=> aa.sysId == sysid && aa.prtmenucode == parentMenuID && aa.isactive==true)
                        .Select(aa => new { aa.menuId, aa.menucode, aa.menuname, aa.menuorder, ChildIcon= aa.classicon })
                        .Distinct()
                        .OrderBy(or => or.menuorder).ToList();

                            //set child menu list
                            foreach (var childRow in childMenuParent)
                            {
                                childMenuModel.Add(new MenuModel
                                {
                                    name = childRow.menuname,
                                    type = "link",
                                    stage = childRow.menucode,
                                    icon = childRow.ChildIcon
                                });
                            }


                            //set parent menu data
                            parentMenuModel.Add(new MenuModel
                            {
                                name = parentRow.parentName,
                                type = "toggle",
                                pages = childMenuModel,
                                icon = parentRow.parenticon,
                                stage = parentMenuID

                            });

                        }
                        else
                        {
                            
                            //get child menu from menulist where parentid equal in the loop
                            var childMenuParent = dbcontext.core_UserMenuList
                         .Where(aa => aa.userId == userid && aa.sysId == sysid && aa.prtmenucode == parentMenuID)
                         .Select(aa => new { aa.menuId, aa.menucode, aa.menuname, aa.menuorder, aa.ChildIcon })
                         .Distinct()
                         .OrderBy(or =>  or.menuorder).ToList();

                            //set child menu list
                            foreach (var childRow in childMenuParent)
                            {
                                childMenuModel.Add(new MenuModel
                                {
                                    name = childRow.menuname,
                                    type = "link",
                                    stage = childRow.menucode,
                                    icon = childRow.ChildIcon
                                });
                            }


                            //set parent menu data
                            parentMenuModel.Add(new MenuModel
                            {
                                name = parentRow.parentName,
                                type = "toggle",
                                pages = childMenuModel,
                                icon = parentRow.parenticon,
                                stage = parentMenuID

                            });
                        }
                    }
                }
                return parentMenuModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get 
        /// </summary>
        /// <param name="userid">fast email / alias</param>
        /// <param name="password">given password / debug password for developer</param>
        /// <param name="appname">Determine if what application they used </param>
        /// <returns></returns>
        //public clsFastLogin getFastLogin(string username, string password, string appname)
        //{
        //    clsFastLogin clsfast = new clsFastLogin();
        //    List<menuid> menus = new List<menuid>();
        //    List<string> user_menu = new List<string>();
        //    Library phpLib = new Library();
        //    List<int?> LevellList = new List<int?>();
        //    empProfile empPro = new empProfile();
        //    string UserType = string.Empty;
        //    string error_message = string.Empty;
        //    string UserName = string.Empty;
        //    string empid = string.Empty;
        //    string IncPassword = string.Empty;

        //    bool hasuser = false;

        //    HRISDBEntities hrcontext = new HRISDBEntities();
        //    WEBEntities webcontext = new WEBEntities();
        //    PALOUPEntities db = new PALOUPEntities();

        //    try
        //    {
        //        //check core user if the alias is apply in login parameter
        //        var qUserAllias = db.core_users.Where(aa => aa.nickname == username);

        //        if (qUserAllias.Count() > 0) // set userid equla to useremail if the alias is true
        //        {
        //            username = qUserAllias.First().username;
        //        }


        //        //check coporate useremployeewww if the user is exist
        //         var q_user = webcontext.V_UserEmployeeWWW.Where(m=> m.EMAILADD== username);

        //         //check the password is equal to debug password
        //         var q_core = db.core_system.Where(wr => wr.debugpassword == password);


        //         if (q_user.Count() > 0)   //check if the user is enter correct email account or allias in login 
        //         {
        //            /*
        //             * If the user used debug password in login page 
        //             * then get actual password base on email account.
        //             */
        //             if (q_core.Count() > 0)
        //             {
        //                 IncPassword = q_user.First().UserPass;
        //             }
        //             else
        //             {
        //                 IncPassword = phpLib.generatePassword(password).ToString();
        //             }

        //             q_user = q_user.Where(aa => aa.UserPass == IncPassword);



        //             //check if the user is used correct password in if not move else statement                
        //             if (q_user.ToList().Count() > 0)
        //             {
        //                 hasuser = true;
        //                 UserType = "EMPLOYEE"; //q_user.First().UserType;
        //                 var employeeDetails = (from aa in webcontext.V_UserEmployeeWWW
        //                                        where aa.EMAILADD == username && aa.TYPE != "SEP" 
        //                                        select aa).First();

        //                 empid = employeeDetails.EMPL_ID;
        //                 clsfast.haserror = false;
        //                 clsfast.validuser = true;
        //                 clsfast.usertype = UserType[0].ToString().ToUpper();
        //                 clsfast.empid = empid;
        //                 clsfast.userid = userid;
        //                 clsfast.hashEmpId = q_user.First().UserCookie;
        //             }
        //             else
        //             {
        //                 //return error
        //                 clsfast.haserror = true;
        //                 clsfast.error_message = "The password you entered is incorrect.";
        //                 return clsfast;

        //             }
        //         }
        //         else
        //         {
        //             //return error
        //             clsfast.haserror = true;
        //             clsfast.error_message = "The email address you entered was not found.";
        //             return clsfast;
        //         }



        //        if (hasuser)
        //        {


        //                var coreemp=db.core_employee.Where(m=> m.empl_id==empid);

        //                if (db.core_users.Where(aa => aa.USERID == empid).Count() == 0 ? false : true)
        //                {
        //                    //check if user is inet false
        //                    if (!(db.core_user.Where(aa => aa.ISINIT == true && aa.USERID == empid).Count() == 1 ? true : false))
        //                    {


        //                        if (UserType == "EMPLOYEE")
        //                        {

        //                            //get the job level
        //                            for (int xlevel = 1; xlevel <= empPro.userlevel; xlevel++)
        //                            {
        //                                LevellList.Add(xlevel);
        //                            }


        //                            //select all apps ment system id = 5 contain access level result in the first script
        //                            var q_menu = from aa in db.core_appmenu
        //                                         where aa.SYSID == 5 && LevellList.Contains(aa.ACCESS_LEVEL) && aa.ISACTIVE==true
        //                                         select aa;


        //                            //loop menu result & add to user menu list
        //                            foreach (var item in q_menu)
        //                            {
        //                                menus.Add(new menuid { MenuID = item.MENUID });
        //                                user_menu.Add(item.MENUID);
        //                            }

        //                            db.Sproc_CoreDelMenu(empid, 5);  //remove all menu in belong to myfastlogistics 

        //                            foreach (var item in user_menu)
        //                                db.Sproc_CoreSetAppMenuUser(empid,
        //                            item,
        //                            true,
        //                            true,
        //                            true,
        //                            true,
        //                            true);


        //                            procedure.InetMenu(UserType, empid);  //Inetialize  menu

        //                            clsfast.username = empPro.EmployeeName2;//
        //                            clsfast.firstName=empPro.FNAME;
        //                            clsfast.menus = menus;

        //                        }
        //                        else if (UserType == "CUSTOMER")
        //                        {

        //                            procedure.InetMenu(UserType, empid);     //Inetialize  menu

        //                            var q_menu = from aa in db.core_UserMenuList
        //                                         where aa.USERID == empid && aa.SYSID == 5
        //                                         select aa;

        //                            foreach (var item in q_menu)
        //                            {
        //                                menus.Add(new menuid
        //                                {
        //                                    MenuID = item.MENUID,
        //                                    okEmployee = procedure.menuHasAccess(clsprocedure.TypeOfUser.employee, item.MENUID),
        //                                    okCustomer = procedure.menuHasAccess(clsprocedure.TypeOfUser.customer, item.MENUID),
        //                                    okSupplier = procedure.menuHasAccess(clsprocedure.TypeOfUser.supplier, item.MENUID)
        //                                });
        //                            }

        //                            // clsfast.username = empPro.EmployeeName;
        //                            clsfast.menus = menus;
        //                        }
        //                        //inetialize menu

        //                    }
        //                    else
        //                    {


        //                        var q_menu = from aa in db.core_UserMenuList
        //                                     where aa.USERID == empid && aa.SYSID == 5
        //                                     select aa;

        //                        foreach (var item in q_menu)
        //                        {
        //                            menus.Add(new menuid
        //                            {
        //                                MenuID = item.MENUID,
        //                                okEmployee = procedure.menuHasAccess(clsprocedure.TypeOfUser.employee, item.MENUID),
        //                                okCustomer = procedure.menuHasAccess(clsprocedure.TypeOfUser.customer, item.MENUID),
        //                                okSupplier = procedure.menuHasAccess(clsprocedure.TypeOfUser.supplier, item.MENUID)
        //                            });
        //                        }

        //                        clsfast.username = empPro.EmployeeName2;
        //                        clsfast.firstName = empPro.FNAME;
        //                        clsfast.menus = menus;

        //                    }
        //                }
        //                else
        //                {


        //                    db.Sproc_CoreSetUser(empid, "STATUS5", "STATUS34", true, "System Generate", DateTime.Now.Date, "System Generate", DateTime.Now.Date, "", "STATUS2", UserType, UserName, userid);
        //                    procedure.InetMenu(UserType, empid);     //Inetialize  menu

        //                    var q_menu = from aa in db.core_UserMenuList
        //                                 where aa.USERID == empid && aa.SYSID == 5
        //                                 select aa;

        //                    foreach (var item in q_menu)
        //                    {
        //                        menus.Add(new menuid
        //                        {
        //                            MenuID = item.MENUID,
        //                            okEmployee = procedure.menuHasAccess(clsprocedure.TypeOfUser.employee, item.MENUID),
        //                            okCustomer = procedure.menuHasAccess(clsprocedure.TypeOfUser.customer, item.MENUID),
        //                            okSupplier = procedure.menuHasAccess(clsprocedure.TypeOfUser.supplier, item.MENUID)
        //                        });
        //                        //    user_menu.Add(item.MENUID);
        //                    }

        //                    clsfast.username = empPro.EmployeeName2;
        //                    clsfast.firstName = empPro.FNAME;
        //                    clsfast.menus = menus;
        //                }

        //                db.Sproc_CoreSetAuditTrail( "Login API", "Login employee ID: " + empid + " appname: " + appname, empid, appname);

        //        }
        //        else
        //        {
        //            clsfast.haserror = true;
        //            clsfast.error_message = error_message;
        //        }

        //        return clsfast;
        //    }
        //    catch (Exception ex)
        //    {
        //        clsfast.haserror = true;
        //        clsfast.error_message = ex.Message;
        //        return clsfast;
        //    }
        //    finally
        //    {
        //        db.Dispose();
        //        webcontext.Dispose();
        //        hrcontext.Dispose();
        //    }
        //}

        public string getAlias(int id)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qUser = db.core_users.Where(wr => wr.userId == id).FirstOrDefault().nickname;
                    return qUser;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void setAlias(int id,string alias)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qUser = db.core_users.Where(wr => wr.userId == id).FirstOrDefault();
                    qUser.nickname = alias;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool? IsSystemGrant(int sysId, int userId)
        {
            sysId = 19;
            try
            {
                using(PALOUPEntities db=new PALOUPEntities())
                {

                    var qGrant = db.core_appsystemuser.Where(aa => aa.userId == userId && aa.sysId == sysId).Count();
                    if (qGrant > 0)
                    {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<UserMenuModel> ListUserMenu(int userID, int sysID)
        {
            sysID = 19;
            List<UserMenuModel> UserMenu = new List<UserMenuModel>();

            using (PALOUPEntities db = new PALOUPEntities()) {

                db.Configuration.LazyLoadingEnabled = false;

                if (sysID == 1)
                {
                    
                    var qMenuList = from aa in db.core_appmenu
                                where aa.sysId == sysID && aa.isactive == true
                                orderby aa.menuorder, aa.istransaction
                                select aa;

                    foreach (var row in qMenuList) {
                        var qUserMenuList = from aa in db.core_appmenuuser
                                            where aa.menuId == row.menuId && aa.userId == userID
                                            select aa;

                        if (qUserMenuList.Count() > 0)
                        {
                            foreach (var urw in qUserMenuList)
                            {
                                UserMenu.Add(new UserMenuModel
                                {
                                    sysId = row.sysId,
                                    menuId=row.menuId,
                                    menuCode = row.menucode,
                                    menuName = row.menuname,
                                    canAdd = urw.canadd,
                                    canEdit = urw.canedit,
                                    canDelete = urw.candelete,
                                    canOpen = urw.canopen,
                                    canView = urw.canview,
                                    isTrans = row.istransaction
                                });
                            }
                        }
                        else {
                            UserMenu.Add(new UserMenuModel
                            {
                                sysId = row.sysId,
                                menuId= row.menuId,
                                menuCode = row.menucode,
                                menuName = row.menuname,
                                canAdd = false,
                                canEdit = false,
                                canDelete = false,
                                canOpen = false,
                                canView = false,
                                isTrans = row.istransaction
                            });
                        }

                     
                    }
                }
                else {
                    var qMenuList = from aa in db.core_appmenu
                                where aa.sysId == sysID  && aa.prtmenucode!="0"
                                    orderby aa.menuorder, aa.istransaction
                                select aa;

                    foreach (var row in qMenuList)
                    {
                        var qUserMenuList = from aa in db.core_appmenuuser
                                            where aa.menuId == row.menuId && aa.userId == userID
                                            select aa;

                        if (qUserMenuList.Count() > 0)
                        {
                            UserMenu.Add(new UserMenuModel
                            {
                                sysId = row.sysId,
                                menuId=row.menuId,
                                menuCode = row.menucode,
                                menuName = row.menuname,
                                canAdd = qUserMenuList.First().canadd,
                                canEdit = qUserMenuList.First().canedit,
                                canDelete = qUserMenuList.First().candelete,
                                canOpen = qUserMenuList.First().canopen,
                                canView = qUserMenuList.First().canview,
                                isTrans = row.istransaction
                            });
                        }
                        else {
                            UserMenu.Add(new UserMenuModel
                            {
                                sysId = row.sysId,
                                menuId=row.menuId,
                                menuCode = row.menucode,
                                menuName = row.menuname,
                                canAdd =false,
                                canEdit = false,
                                canDelete = false,
                                canOpen = false,
                                canView = false,
                                isTrans = row.istransaction
                            });
                        }                       
                    }
                }             
            }

            return UserMenu;
        }




     
        public dynamic ValidateEmployeeEmail(string email)
        {
            string strMobile = string.Empty;

            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    var qexaminee = db.ole_examinee.Where(wr => wr.email == email);
                    if (qexaminee.Count() > 0)
                    {
                        strMobile = qexaminee.FirstOrDefault().mobile;
                    }



                    var qvalidateEmail = db.core_employee
                        .Where(wr => wr.emailadd == email && wr.type != "SEP");

                    if (qvalidateEmail.Count() == 0)
                    {
                        Exception e = new Exception("Invalid Email!");
                        throw e;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strMobile))
                        {
                            qvalidateEmail.First().employeepresentcontact = strMobile;
                        }
                        return qvalidateEmail.ToList();
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