//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PaloUpAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class core_appmenu
    {
        public int menuId { get; set; }
        public int sysId { get; set; }
        public string menucode { get; set; }
        public string menuname { get; set; }
        public string prtmenucode { get; set; }
        public Nullable<int> menuorder { get; set; }
        public Nullable<bool> isactive { get; set; }
        public string viewtype { get; set; }
        public Nullable<bool> istransaction { get; set; }
        public string menutype { get; set; }
        public Nullable<int> accesslevel { get; set; }
        public Nullable<bool> defaultemployee { get; set; }
        public Nullable<bool> defaultcustomer { get; set; }
        public Nullable<bool> defaultsupplier { get; set; }
        public Nullable<bool> formobile { get; set; }
        public Nullable<bool> forweb { get; set; }
        public string classicon { get; set; }
        public Nullable<bool> mobiledefault { get; set; }
        public Nullable<bool> webdefault { get; set; }
    
        public virtual core_appsystem core_appsystem { get; set; }
    }
}
