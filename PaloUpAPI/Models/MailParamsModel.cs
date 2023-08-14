using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class MailParamsModel
    {
        public string mailTemplatePath { get; set; }
        public string recipientsMail { get; set; }
        public string ccMail { get; set; }
        public string sendername { get; set; }
        public string subject { get; set; }
        public string createdby { get; set; }
        public string mailBody { get; set; }
        public string[] param { get; set; }
    }
}