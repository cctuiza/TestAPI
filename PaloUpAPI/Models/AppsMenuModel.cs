using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;


namespace PaloUpAPI.Models
{
    public class AppsMenuModel
    {
        public bool hasError { get; set; }
        public string errorMsg { get; set; }
        public List<core_UserMenuList> MenuList { get; set; }
    }
}