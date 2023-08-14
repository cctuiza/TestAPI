using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;

namespace PaloUpAPI.Core.BusinessServices
{
    public class UserServices : IUserServices
    {
    
        public string Authenticate(string userName, string password)
        {
            Library phpLib = new Library();
            string IncPassword = string.Empty;

          try
            {

                using (PALOUPEntities db = new PALOUPEntities())
                {



                    //check coporate useremployeewww if the user is exist

                    var userEmployee = db.core_users.Where(m => m.username == userName && m.status == "A");

                    //check the password is equal to debug password
                    var coreSystem = db.core_system.Where(m => m.debugpassword == password);

                    /*
                     * If the user used debug password in login page 
                     * then get actual password base on email account.
                     */
                    if (coreSystem.Count() > 0)
                    {
                        IncPassword = userEmployee.First().userpass;
                    }
                    else
                    {
                        IncPassword = phpLib.generatePassword(password).ToString();
                    }

                    var qUserProfile = db.core_users.Where(m => m.username == userName & m.userpass == IncPassword)
               .Select(m => new { m.userId, m.username, m.userrole, m.lastname, m.firstname, m.middlename, m.usertype, m.emplId, m.emailaddress, m.mobileno, m.hashcode });
                    if (qUserProfile.Count() > 0)
                    {
                        var user = qUserProfile.FirstOrDefault();
                        if (user != null)
                        {
                            return user.userId.ToString();
                        }
                    }


                    return null;

                }
            }    
            catch(Exception ex)
            {
                throw ex;
            }     
        }

    }
}