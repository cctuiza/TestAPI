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
    
    public partial class mifs_topic
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public mifs_topic()
        {
            this.mifs_meeting = new HashSet<mifs_meeting>();
            this.mifs_topicmembers = new HashSet<mifs_topicmembers>();
        }
    
        public int topicId { get; set; }
        public int groupId { get; set; }
        public string hashcode { get; set; }
        public string aliascode { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public int topiclevel { get; set; }
        public int fupfrequency { get; set; }
        public string occurence { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public byte topicscope { get; set; }
        public string bgcolor { get; set; }
        public string classification { get; set; }
        public string profile { get; set; }
        public int statchangecounterlimit { get; set; }
        public string branchcode { get; set; }
        public string districtcode { get; set; }
        public string modifiedby { get; set; }
        public Nullable<System.DateTime> modifieddate { get; set; }
        public byte isvisible { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mifs_meeting> mifs_meeting { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<mifs_topicmembers> mifs_topicmembers { get; set; }
        public virtual mifs_topicgroup mifs_topicgroup { get; set; }
    }
}
