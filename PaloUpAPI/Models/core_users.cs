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
    
    public partial class core_users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public core_users()
        {
            this.core_appmenuuser = new HashSet<core_appmenuuser>();
            this.core_appsystemuser = new HashSet<core_appsystemuser>();
            this.core_userswarehouse = new HashSet<core_userswarehouse>();
            this.rms_useranalysisgroupaccess = new HashSet<rms_useranalysisgroupaccess>();
            this.wms_userserverwarehouse = new HashSet<wms_userserverwarehouse>();
        }
    
        public int userId { get; set; }
        public string username { get; set; }
        public string userrole { get; set; }
        public string userpass { get; set; }
        public string lastname { get; set; }
        public string middlename { get; set; }
        public string firstname { get; set; }
        public string nickname { get; set; }
        public string usertype { get; set; }
        public string emplId { get; set; }
        public string emailaddress { get; set; }
        public string mobileno { get; set; }
        public string status { get; set; }
        public string hashcode { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public bool canaccessaquila { get; set; }
        public bool ismailvalid { get; set; }
        public bool isphonevalid { get; set; }
        public Nullable<bool> isverify { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<core_appmenuuser> core_appmenuuser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<core_appsystemuser> core_appsystemuser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<core_userswarehouse> core_userswarehouse { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<rms_useranalysisgroupaccess> rms_useranalysisgroupaccess { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<wms_userserverwarehouse> wms_userserverwarehouse { get; set; }
    }
}
