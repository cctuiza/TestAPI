using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  PaloUpAPI.Models

{
    public class SmsParam
    {
        public string mobile { get; set; }
        public string smsMsg { get; set; }
        public string appname { get; set; }
        public string smsSender { get; set; }
    }
}