using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;
using System.Data.Entity;


namespace PaloUpAPI.SQLData
{
    public class MifsResponse
    {
  

       
 

        public void acknowledgeHascode(string hashcode,int id)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var qIssue = db.mifs_issues.Where(wr => wr.hashcode == hashcode && wr.issueId==id).FirstOrDefault();
                    if ((qIssue.acknowledgedbysource + qIssue.acknowledgebyrespondent) == 1)
                    {
                        string newStatus = qIssue.newstatus;
                        qIssue.acknowledgebyrespondent = 0;
                        qIssue.acknowledgedbysource = 0;
                        qIssue.status = newStatus;
                        qIssue.newstatus = string.Empty;
                        db.SaveChanges();
                    }
                    else
                    {
                        Exception trap=new Exception("The Issue is ready Acknowledge!");
                        throw trap;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void desagreeHascode(string id)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var qIssue = db.mifs_issues.Where(wr => wr.hashcode == id).FirstOrDefault();
                    if ((qIssue.acknowledgedbysource + qIssue.acknowledgebyrespondent) == 1)
                    {
                      
                        qIssue.acknowledgebyrespondent = 0;
                        qIssue.acknowledgedbysource = 0;    
                        qIssue.newstatus =string.Empty;
                        db.SaveChanges();
                    }
                    else
                    {
                        Exception trap = new Exception("No pending request found!");
                        throw trap;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private mifs_members getRepresentativeEmail(string memberId)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    var qRepresentative = db.mifs_members.Where(wr => wr.memberId == memberId).FirstOrDefault();

                    return qRepresentative;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

     

 
    }
}