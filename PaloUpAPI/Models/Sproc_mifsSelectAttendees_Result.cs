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
    
    public partial class Sproc_mifsSelectAttendees_Result
    {
        public int attendanceId { get; set; }
        public int meetingId { get; set; }
        public string description { get; set; }
        public string agenda { get; set; }
        public string venue { get; set; }
        public string facilitator { get; set; }
        public string attendeeempid { get; set; }
        public string attendeename { get; set; }
        public string attendeemail { get; set; }
        public string attendeephone { get; set; }
        public string created_by { get; set; }
        public byte isgenerated { get; set; }
    }
}
