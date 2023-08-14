using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{

    public class UserWarehouse
    {
        public int sysId { get; set; }
        public int userId { get; set; }
        public string created_by { get; set; }
        public List<UserBranchModel> userBranch { get; set; }
    }
    public class UserBranchModel
    {
        public Nullable<bool> isCheck { get; set; }
        public string brancode { get; set; }
        public string  branname { get; set; }
        public string branaddress { get; set; }
        public bool isdefault { get; set; }
        
    }
}