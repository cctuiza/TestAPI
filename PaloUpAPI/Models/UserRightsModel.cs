using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class UserRightsModel
    {
        public string userID { get; set; }
        public int sysID { get; set; }
        public string sysName { get; set; }
        public string iconName { get; set; }
        public List<UserMenuList> menulist { get; set; }
    }

    public class UserMenuList
    {
        public int menuID { get; set; }
        public string menuCode { get; set; }
        public string menuName { get; set; }
        public string controller { get; set; }
        public string parentID { get; set; }
        public string parentMenu { get; set; }
        public List<UserChildMenuList> userChildMenuList { get; set; }
    }
    public class UserChildMenuList
    {
        public int menuID { get; set; }
        public string menuCode { get; set; }
        public string menuName { get; set; }
    }
}