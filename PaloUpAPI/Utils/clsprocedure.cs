using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Data;
using System.Drawing;
using System.IO;
using HtmlAgilityPack;
namespace PaloUpAPI.Models
{
    public class clsprocedure
    {

        /// <summary>
        /// strToImage
        /// </summary>
        /// <param name="imgData">image data</param>
        /// <returns>Images</returns>
        public Image strToImage(string imgData)
        {
            //get a temp image from bytes, instead of loading from disk
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(imgData);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }

       /// <summary>
       /// SentMail
       /// </summary>
       /// <param name="subject">Subject</param>
       /// <param name="mailbody">mail body</param>
       /// <param name="tomail">Receipent</param>
       /// <returns>bool</returns>
        public bool SentMail(string subject, string mailbody, string tomail)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("no-reply@fastgroup.biz");

            string[] mailto = tomail.Split(';');

            foreach (string strmail in mailto)
            {
                msg.To.Add(strmail);
            }


            msg.Subject = subject;

            msg.Body = mailbody;
            msg.IsBodyHtml = true;
            SmtpClient sc = new SmtpClient("mail.flcprivate.dns");
            sc.Port = 587;

            sc.Credentials = new NetworkCredential("flc", "flc213");

            sc.Send(msg);
            return true;
        }


        public string StrSplit(string param, int maxchar)
        {
            string stresult = param;

            if (param.Length > 10)
            {
                stresult = stresult.Substring(0, maxchar) + "...";
            }

            return stresult;
        }


        public bool IsNumeric(string input)
        {
            try
            {
                int.Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public DataTable ToDataSet<T>(IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                t.Columns.Add(propInfo.Name, ColType);
            }

            //go through each property on T and add each value to the table
            foreach (T item in list)
            {
                DataRow row = t.NewRow();

                foreach (var propInfo in elementType.GetProperties())
                {
                    row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value;
                }

                t.Rows.Add(row);
            }

            return t;
        }

        public string formatNum(string strNo, int iDigit)
        {
            int i_length = strNo.Length;

            int dif = iDigit - i_length;

            string digit = string.Empty;

            for (int i = 1; i <= dif; i++)
            {
                digit += "0";
            }

            return digit + strNo;

        }

        private string formaCode(string pano, int noOfDigit)
        {
            int i_length = pano.Length;

            int dif = noOfDigit - i_length;

            string digit = string.Empty;

            for (int i = 1; i <= dif; i++)
            {
                digit += "0";
            }

            return digit + pano;

        }

        /// <summary>
        /// GetControlNo
        /// </summary>
        /// <param name="type">control type</param>
        /// <param name="code">control code</param>
        /// <param name="prefix">prefix</param>
        /// <returns>string</returns>
        public string GetControlNo(string type,string code,string prefix)
        {
            try
            {
                string result = string.Empty;

                using (PALOUPEntities dbcontext = new PALOUPEntities())
                {

                    decimal? lastdigit = dbcontext.app_control_no.Where(aa => aa.control_type == type && aa.control_code == code).First().last_seq_no;
                    string control_digit = dbcontext.app_control_no.Where(aa => aa.control_type == type && aa.control_code == code).First().control_digit;
                    lastdigit = lastdigit + 1;
                    result = prefix + formatNum(int.Parse(lastdigit.ToString()).ToString(), int.Parse(control_digit));
                }
                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

     

      

        public string get_monthname(int monthNo)
        {
            string str_result = string.Empty;

            switch (monthNo)
            {
                case 1:
                    str_result = "JAN";
                    break;
                case 2:
                    str_result = "FEB";
                    break;
                case 3:
                    str_result = "MAR";
                    break;
                case 4:
                    str_result = "APR";
                    break;
                case 5:
                    str_result = "MAY";
                    break;
                case 6:
                    str_result = "JUN";
                    break;
                case 7:
                    str_result = "JUL";
                    break;
                case 8:
                    str_result = "AUG";
                    break;
                case 9:
                    str_result = "SEP";
                    break;
                case 10:
                    str_result = "OCT";
                    break;
                case 11:
                    str_result = "NOV";
                    break;
                case 12:
                    str_result = "DEC";
                    break;
            }

            return str_result;
        }

        //public bool InetMenu(string param,string empid)
        //{
        //    List<string> user_menu = new List<string>();
        //    using (PALOUPEntities dbcontext = new PALOUPEntities())
        //    {
        //        var q_menu = from aa in dbcontext.core_appmenu
        //                     where aa.isactive==true
        //                     select aa;

        //        switch (param)
        //        {
        //            case "EMPLOYEE":
        //                q_menu = q_menu.Where(aa => aa.defaultemployee == true);

        //                break;
        //            case "CUSTOMER":
        //                q_menu = q_menu.Where(aa => aa.defaultcustomer == true);

        //                break;
        //        }

        //        foreach (var item in q_menu)
        //        {
        //            user_menu.Add(item.menucode);
        //        }

        //        //delete all user menu contain menu result in the top
        //        var q_user_menu = from aa in dbcontext.core_appmenuuser
        //                          where aa.userId == empid && user_menu.Contains(aa.MENUID)
        //                          select aa;
        //        dbcontext.core_appmenuuser.RemoveRange(q_user_menu);
        //        dbcontext.SaveChanges(); 

        //        //add new menu in the procedure
        //        foreach (var item in user_menu)
        //            dbcontext.Sproc_CoreSetAppMenuUser(empid,
        //        item,
        //        true,
        //        true,
        //        true,
        //        true,
        //        true);
        //    }

        //    return true;

        //}

        /// <summary>
        /// Type of User
        /// </summary>
        public enum TypeOfUser { employee, customer, supplier };

        /// <summary>
        /// get menu has access
        /// </summary>
        /// <param name="typeOfUser">Type of user</param>
        /// <param name="menuid">Menu ID</param>
        /// <returns>boolean</returns>
        //public bool menuHasAccess(TypeOfUser typeOfUser,string menuid)
        //{
        //    try
        //    {

        //        bool isResult = false;
        //        using (PALOUPEntities dbcontext = new PALOUPEntities())
        //        {

        //            var q_menu = (from aa in dbcontext.core_appmenu
        //                         where aa.MENUID == menuid
        //                         select aa).FirstOrDefault();

        //            switch (typeOfUser)
        //            {
        //                case TypeOfUser.employee:
        //                    if (q_menu.DEFAULT_EMPLOYEE==true)
        //                        isResult = true;
                            
        //                    break;
        //                case TypeOfUser.customer:
        //                    if (q_menu.DEFAULT_CUSTOMER == true)
        //                        isResult = true;
        //                    break;
        //                case TypeOfUser.supplier:
        //                    if (q_menu.DEFUALT_SUPPLIER == true)
        //                        isResult = true;
        //                    break;
        //            }
        //        }

        //        return isResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public string GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 4)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        public string enCodeString(string p)
        {
            try
            {
                string strOut = string.Empty;

                foreach (char a in p)
                {
                    strOut += alphaToNum(a.ToString());
                }

                return strOut;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string alphaToNum(string p)
        {
            try
            {
                string strOut=string.Empty;

                switch (p)
                {
                    case "0":
                       strOut= "z";
                        break;
                    case "1":
                        strOut= "q";
                        break;
                    case "2":
                        strOut= "y";
                        break;
                    case "3":
                        strOut= "t";
                        break;
                    case "4":
                        strOut= "v";
                        break;
                    case "5":
                        strOut= "b";
                        break;
                    case "6":
                        strOut= "t";
                        break;
                    case "7":
                        strOut= "d";
                        break;
                    case "8":
                        strOut= "a";
                        break;
                    case "9":
                        strOut= "s";
                        break;
                    default:
                        strOut= p;
                        break;

                }

                return strOut;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EmailNotification(MailParamsModel param)
        {

            string emailbody = string.Empty;
            try
            {

                //if required approval equal to true
                //set approval email to approval email
                if (string.IsNullOrEmpty(param.mailBody))
                {
                    string templatePath = HttpContext.Current.Request.PhysicalApplicationPath + param.mailTemplatePath;

                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(templatePath);

                    HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("/html/body");

                     emailbody = string.Format(bodyNode.InnerHtml, param.param);
                }
                else
                {
                    emailbody = param.mailBody;
                }
             
              
                

                using (PaloUpMailEntities db = new PaloUpMailEntities())
                {
                    core_mailer coreEmail = new core_mailer
                    {
                        senderDisplayEmail = "no-reply@fastgroup.biz",
                        senderDisplayName = param.sendername,
                        recipient = param.recipientsMail,
                        ccrecipient=param.ccMail,
                        mailSubject = param.subject,
                        mailBody = emailbody,
                        mailStatus = "q",
                        mailFormat = "HTML",
                        createdBy = param.createdby,
                        created = DateTime.Now
                    };

                    db.core_mailer.Add(coreEmail);
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}