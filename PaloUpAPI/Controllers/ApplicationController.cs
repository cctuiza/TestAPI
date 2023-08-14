using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.Models;
using System.Web.Http.Description;
using PaloUpAPI.SQLData;
using PaloUpAPI.ActionFilters;
 
using System.Data.Entity;

namespace PaloUpAPI.Controllers
{
    [AuthorizationRequired]
    [Compress]
    public class ApplicationController : ApiController
    {

        PALOUPEntities db = new PALOUPEntities();
        
        [HttpGet]
        public HttpResponseMessage GetApps()
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;
                var jsonOut = db.core_appsystem.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, jsonOut);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            

           
        }

        /// <summary>
        /// get-apps-details
        /// </summary>
        /// <param name="id">application id</param>
        /// <returns>APPS_SYSTEM</returns>
        [HttpGet]
        [ActionName("get-apps-details")]
        public HttpResponseMessage GetAppsDetails(int id)
        {
            try
            {
                db.Configuration.LazyLoadingEnabled = false;
                var qsystem = db.core_appsystem.Where(aa => aa.sysId == id).FirstOrDefault();

                return Request.CreateResponse(HttpStatusCode.OK, qsystem);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        [HttpGet]
        public HttpResponseMessage GetApplicationActive() {
            try
            {
                var system = db.core_appsystem.Where(aa => aa.isactive == true).Select(aa => new { aa.sysId, aa.sysname });
                return Request.CreateResponse(HttpStatusCode.OK, system);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
           
        }

     
        
        [HttpPost]
        [ActionName("save-application-details")]
        public HttpResponseMessage saveApplicationDetails(core_appsystem param) {
            try
            {
                if (db.core_appsystem.Where(aa => aa.sysId == param.sysId).Count() > 0)
                {
                    db.Entry(param).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(param).State = EntityState.Added;
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex) {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-image-list")]
        public IHttpActionResult getImageList(int id)
        {
            try
            {
                AppsImageModel n = new AppsImageModel();
                List<ImgFilename> n2 = new List<ImgFilename>();

                int? interval = 5000;
                string domain1 = db.core_derictory.Where(aa => aa.id == 2).First().domain;//thumbnail
                string domain2 = db.core_derictory.Where(aa => aa.id == 3).First().domain;//original
              
                var qImg = db.apps_file_map.Where(aa => aa.apps_sysid == id && aa.file_isdeleted==false).OrderBy(aa => aa.id).ToList();

                foreach (var row in qImg)
                {
                    n2.Add(new ImgFilename
                    {
                        fileid = row.id,
                        filename = row.file_name,
                        fileUrl = domain1  + row.file_name,
                        fileOriginalUrl=domain2  + row.file_name,
                        fileselect = false
                    });
                    
                }

                n.interval = interval;
                n.filePath = n2;

                return Json(new { hasError = false, data = n });
            }
            catch (Exception ex)
            {
                return Json(new { hasError = true, errorMessage = ex.Message });
            }
        }


       
        [HttpDelete]
        [ActionName("delete-apps")]
        public HttpResponseMessage delete(int id)
        {
            try
            {
                core_appsystem qSys = db.core_appsystem.Where(wr => wr.sysId == id).FirstOrDefault();
                db.core_appsystem.Remove(qSys);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-app-settings")]
        public HttpResponseMessage getAppSetting(int id)
        {
            try
            {
                var settingsList = db.core_appsettings.Where(m => m.appID == id).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, settingsList);

            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [ActionName("update-app-settings")]
        public HttpResponseMessage updateAppSetting(List<core_appsettings> param)
        {
            try
            {

                foreach(var setting in param)
                {
                    db.Entry(setting).State = EntityState.Modified;
                    db.SaveChanges();
                }
           

                return Request.CreateResponse(HttpStatusCode.OK, param);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

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
