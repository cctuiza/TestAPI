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
    
    public partial class core_temp_attachment
    {
        public int attachmentId { get; set; }
        public Nullable<int> appId { get; set; }
        public string refmodule { get; set; }
        public string filename { get; set; }
        public string newfilename { get; set; }
        public string extension { get; set; }
        public string filepath { get; set; }
        public Nullable<int> userId { get; set; }
        public string status { get; set; }
        public string createdby { get; set; }
        public System.DateTime createdate { get; set; }
    }
}
