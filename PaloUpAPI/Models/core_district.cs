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
    
    public partial class core_district
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public core_district()
        {
            this.core_branchmst = new HashSet<core_branchmst>();
        }
    
        public string districtcode { get; set; }
        public string districtname { get; set; }
        public string districtheadname { get; set; }
        public string districtheadcontact { get; set; }
        public string districtheadid { get; set; }
        public string districtheademail { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> create_date { get; set; }
        public Nullable<int> sortorder { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<core_branchmst> core_branchmst { get; set; }
    }
}
