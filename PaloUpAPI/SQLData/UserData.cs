using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;

namespace PaloUpAPI.SQLData
{
    public class UserData
    {
        public IEnumerable<UserRightsModel> getUser(int id)
        {
            string iconName = string.Empty;
            string userId = string.Empty;

            List<UserRightsModel> userRightsModel = new List<UserRightsModel>();

            using (PALOUPEntities db = new PALOUPEntities())
            {
                var qSystem = db.core_UserMenuList.Where(aa => aa.userId == id && aa.forweb == true && aa.sysId != 13).Select(aa => new { aa.sysId, aa.sysname }).Distinct().OrderBy(aa => aa.sysname);

                foreach (var rowSys in qSystem)
                {
                    List<UserMenuList> menuList = new List<UserMenuList>();

                    /*
                     @ select none admin menu
                     */
                    if (rowSys.sysId != 1)
                    {
                        //var qMenu = db.V_MENULIST.Where(aa => aa.SYSID == rowSys.SYSID && aa.USERID == id && aa.FOR_WEB == true).OrderBy(aa => aa.ParentMenuOrder).ThenBy(aa=> aa.MENUORDER).Select(aa => new { aa.PRTMENUID, aa.MENUPARENT }).Distinct().ToList();
                        if (rowSys.sysId != 13)
                        {
                            var qMenu = db.core_UserMenuList
                                .Where(aa => aa.sysId == rowSys.sysId && aa.userId == id && aa.forweb == true)
                                .Select(aa => new { aa.prtmenucode, aa.MENUPARENT, aa.ParentMenuOrder, aa.menuorder })
                                .Distinct()
                                .OrderBy(aa => aa.ParentMenuOrder)
                                .ThenBy(aa => aa.menuorder)
                                .ToList();



                            foreach (var rowMenu in qMenu.Select(aa => new { aa.MENUPARENT, aa.prtmenucode }).Distinct())
                            {
                                List<UserChildMenuList> childMenuList = new List<UserChildMenuList>();

                                childMenuList = getChildMenu(id, rowSys.sysId, rowMenu.prtmenucode);


                                menuList.Add(new UserMenuList
                                {
                                    parentID = rowMenu.prtmenucode,
                                    parentMenu = rowMenu.MENUPARENT,
                                    userChildMenuList = childMenuList
                                });
                            }
                        }
                    }
                    /*
                @ select admin menu
                */
                    else
                    {
                        var qMenu = db.core_UserMenuList.Where(aa => aa.sysId == rowSys.sysId && aa.userId == id && aa.forweb == true).OrderBy(aa => aa.menuorder).ThenBy(aa => aa.menuorder).Select(aa => new { menuID = aa.menuId,aa.menucode, menuName = aa.menuname, aa.prtmenucode, aa.MENUPARENT }).ToList();

                        foreach (var rowMenu in qMenu)
                        {
                            menuList.Add(new UserMenuList
                            {
                                menuID = rowMenu.menuID,
                                menuCode=rowMenu.menucode,
                                menuName = rowMenu.menuName,
                                controller = rowSys.sysId == 1 ? "Admin" : rowMenu.prtmenucode,
                                parentMenu = rowMenu.MENUPARENT
                            });
                        }

                    }

                    switch (rowSys.sysId)
                    {
                        case 1:
                            iconName = "glyphicons glyphicons-fire";
                            break;
                        case 2:
                            iconName = "glyphicons glyphicons-hdd";
                            break;
                        case 14:
                            iconName = "glyphicons glyphicons-hdd";
                            break;
                        case 17:
                            iconName = "glyphicons glyphicons-cargo";
                            break;
                        default:
                            iconName = "glyphicons glyphicons-hdd";
                            break;
                    }

                    userRightsModel.Add(new UserRightsModel
                    {
                        userID = userId,
                        sysID = rowSys.sysId,
                        sysName = rowSys.sysname,
                        menulist = menuList,
                        iconName = iconName
                    });
                }
                db.Dispose();
            }

            return userRightsModel;
        }


        public List<UserChildMenuList> getChildMenu(int userid, int sysID, string parentID)
        {

            List<UserChildMenuList> menuList = new List<UserChildMenuList>();
            using (PALOUPEntities db = new PALOUPEntities())
            {
                var qMenu = db.core_UserMenuList.Where(aa => aa.sysId == sysID && aa.userId == userid && aa.prtmenucode == parentID && aa.forweb == true).OrderBy(aa => aa.menuorder).Select(aa => new { menuID = aa.menuId, aa.menucode, menuName = aa.menuname, aa.prtmenucode, aa.MENUPARENT }).ToList().Distinct();

                foreach (var rowMenu in qMenu)
                {
                    menuList.Add(new UserChildMenuList
                    {
                        menuID = rowMenu.menuID,
                        menuCode=rowMenu.menucode,
                        menuName = rowMenu.menuName
                    });
                }

                db.Dispose();
            }
            return menuList;
        }


        public string getAlias(int id)
        {
            try
            {
                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {
                    var q = dbcontext.core_users.Where(aa => aa.userId == id).FirstOrDefault();
                    return q.nickname;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void postAlias(int id,string alias)
        {
            try
            {
                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {
                    var q = dbcontext.core_users.Where(aa => aa.userId == id).FirstOrDefault();
                    q.nickname = alias;
                    dbcontext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


       

   
        
    }
}