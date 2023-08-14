using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;
using System.Data.Entity;

namespace PaloUpAPI.SQLData
{
    public class MifsSourceType
    {
        Library phpLib = new Library();

       


        public void post(mifs_topic p)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    if (p.topicId !=-1)
                    {
                        p.groupId = 1;
                        p.modifieddate = DateTime.Now.Date;
                        db.Entry(p).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        string hashcode = DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz").ToString();
                        p.hashcode = phpLib.generatePassword(hashcode).ToString();
                        p.create_date = DateTime.Now.Date;
                        p.groupId = 1;
                        db.Entry(p).State = EntityState.Added;
                        db.SaveChanges();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void delete(int p)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    mifs_topic n = db.mifs_topic.Where(wr => wr.topicId == p).FirstOrDefault();
                    db.mifs_topic.Remove(n);
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
