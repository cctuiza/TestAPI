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
    
    public partial class mifs_attendance
    {
        public int attendanceId { get; set; }
        public int meetingId { get; set; }
        public string attendeeempid { get; set; }
        public string attendeename { get; set; }
        public string attendeemail { get; set; }
        public string attendeephone { get; set; }
        public byte ismember { get; set; }
        public byte ispresent { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public Nullable<System.DateTime> timein { get; set; }
        public Nullable<System.DateTime> timeout { get; set; }
        public byte isgenerated { get; set; }
        public Nullable<byte> isremark { get; set; }
        public string remarks { get; set; }
        public byte isgoing { get; set; }
    
        public virtual mifs_meeting mifs_meeting { get; set; }
    }
}
