using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PaloUpAPI.Models;

namespace FscCore.Controllers
{
  
    public class PluginController : Controller
    {
        PALOUPEntities db = new PALOUPEntities();

        // GET: Plugin
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("new-announcement")]
        public ActionResult NewAnnouncement(string id)
        {
            string empname = string.Empty;

            empname = db.core_employee.Where(aa => aa.empl_id == id).First().employeename2;

            ViewBag.empid = id;
            ViewBag.empname = empname;

            return View();
        }

        #region "PaloUp"

        [ActionName("attend-paloup-meeting")]
        public ActionResult AttendPaloUpMeeting(string ans,int meetingId ,string uemail)
        {
            db.Configuration.LazyLoadingEnabled = false;
          
            var paloupAttendees = db.mifs_attendance.Where(m => m.meetingId == meetingId && m.attendeemail == uemail).FirstOrDefault();

            switch (ans)
            {
                case "YES":
                    paloupAttendees.isgoing = 1;
                    break;

                case "NO":
                    paloupAttendees.isgoing = 0;
                    break;
            }

            db.SaveChanges();
            return View();
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();

            }
            base.Dispose(disposing);
        }
    }
}