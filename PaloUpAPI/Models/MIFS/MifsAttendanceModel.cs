using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsAttendanceModel
    {
        public string hashcode { get; set; }
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
    
    }
}