using PaloUpAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Class
{
    //class for dynamic posting of issue by other systems
    public class OtherSystemsIssue
    {
        public mifs_issues issue { get; set; }
        public string system { get; set; }
        public string createdbyID { get; set; }

    }

    //class for QMS issue posting
    public class QMSIssue
    {
        public mifs_members Responsible { get; set; }
        public mifs_issues Issue { get; set; }
        public string type { get; set; }
    }



}