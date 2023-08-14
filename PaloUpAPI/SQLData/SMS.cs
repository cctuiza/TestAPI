using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;

namespace PaloUpAPI.SQLData
{
    public class SMS
    {
        public static void SendSms(string mobile, string smsMsg, string appname,string smssender)
        {
            try {

                if (mobile[0].ToString() == "0") {
                    mobile = "+63" + mobile.Remove(0, 1);
                }

                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {

                    dbcontext.Sproc_SendSms(mobile, smsMsg, appname, smssender);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}