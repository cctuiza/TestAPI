using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class EmployeeModel
    {
        public bool hasError { get; set; }
        public int ipage { get; set; }
        public bool canPage { get; set; }
        public int pageCount { get; set; }
        public List<EmployeeAccount> empAccount { get; set; }
        public string error_message { get; set; }
    }

  

    public class EmployeeAccount
    {
        public string EmpID {get;set;}
        public string SBU { get; set; }
        public string EmpName {get;set;}
        public string EmpEmail { get; set; }
        public string Position { get; set; }
        public string UrlID { get; set; }
        public string ContactNo { get; set; }
    }

    public class EmployeePMSDtl
    {
        public string empId { get; set; }
        public string empName { get; set; }
        public string department { get; set; }
        public string position { get; set; }
    }
}