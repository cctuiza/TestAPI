using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class UserMenuModel
    {
        public int sysId { get; set; }
        public int menuId { get; set; }
        public string menuCode { get; set; }
        public string menuName { get; set; }
        public string parentName { get; set; }
        public Nullable<bool> canAdd { get; set; }
        public Nullable<bool> canEdit { get; set; }
        public Nullable<bool> canDelete { get; set; }
        public Nullable<bool> canOpen { get; set; }
        public Nullable<bool> canView { get; set; }
        public Nullable<bool> isTrans { get; set; }


    }
}