using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class TransList
    {
        public bool hasError { get; set; }
        public int total_count { get; set; }
        public dynamic data { get; set; }
        public string errorMessage { get; set; }
    }
}