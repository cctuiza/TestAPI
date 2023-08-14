using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class clsFastLogin
    {
        public bool haserror { get; set; }
        public bool validuser { get; set; }
        public string usertype { get; set; }
        public string empid { get; set; }
        public string hashEmpId { get; set; }
        public string userid { get; set; }
        public string username { get; set; }
        public string firstName { get; set; }
        public string departmentcode { get; set; }
        public string branchcode { get; set; }
        public List<menuid> menus { get; set; }
        public string error_message { get; set; }

    }

    public class menuid
    {
        public string MenuID { get; set; }
        public bool okCustomer { get; set; }
        public bool okEmployee { get; set; }
        public bool okSupplier { get; set; }
    }

    public class empProfile
    {
        public int? userlevel { get; set; }
        public string EmployeeName2 { get; set; }
        public string FNAME { get; set; }
    }

    public class clsSendMail
    {
        public bool haserror { get; set; }
        public string error_message { get; set; }
    }


    public class clsProfile
    {
        public bool haserror { get; set; }
        public List<clsProfileMst> ProMst { get; set; }
        public string error_message { get; set; }
    
    }

    public class clsinfo
    {
        public string fieldname { get; set; }
        public string value { get; set; }
    }

    public class clsProfileMst
    {
        
        public string userDtlHeader { get; set; }
        public List<clsinfo> userDtl { get; set; }
        
    }


}