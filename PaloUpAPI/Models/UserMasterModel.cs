 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class UserMasterModel
    {
        public core_users coreUsers { get; set; }
        public List<UserSystemModel> userSys { get; set; }
    }

 
   
}