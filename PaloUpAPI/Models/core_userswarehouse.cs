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
    
    public partial class core_userswarehouse
    {
        public string brancode { get; set; }
        public int userId { get; set; }
        public int sysId { get; set; }
        public bool isdefault { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> create_date { get; set; }
    
        public virtual core_appsystem core_appsystem { get; set; }
        public virtual core_users core_users { get; set; }
    }
}
