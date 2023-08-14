using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PaloUpAPI.Models;
using PaloUpAPI.SQLData;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using System.Data.Entity;
using PaloUpAPI.ActionFilters;
 
namespace PaloUpAPI.Controllers
{


    public class FilemapController : ApiController
    {
        PALOUPEntities db = new PALOUPEntities();



        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Uploadv2()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {

                var httpRequest = HttpContext.Current.Request;
                var drivePath = db.core_drivepath.Where(m => m.id ==9).FirstOrDefault();
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    object result = null;
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 8; //Size = 4 MB  

                       // IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                      

                        //if (!AllowedFileExtensions.Contains(extension))
                        //{

                        //    var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                        //    dict.Add("error", message);
                        //    return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        //}
                        if (!Request.Content.IsMimeMultipartContent())
                        {
                            this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            string filename = postedFile.FileName.Replace(extension, "");
                            string newfilename = DateTime.Now.Ticks.ToString();

                            // var filePath = HttpContext.Current.Server.MapPath("~/Userimage/" + postedFile.FileName + extension);
                            // var filePath = HttpContext.Current.Server.MapPath("~/upload/converted/" + newfilename + extension);

                            var filePath = drivePath.drive_path + newfilename + extension;
                            postedFile.SaveAs(filePath);

                            core_temp_attachment coreTemp = new core_temp_attachment
                            {
                                appId = int.Parse(httpRequest.Form["appId"]),
                                refmodule = httpRequest.Form["refmodule"],
                                userId = int.Parse(httpRequest.Form["userId"]),
                                filename = filename,
                                filepath = drivePath.drive_path,
                                status = "A",
                                createdby = httpRequest.Form["createdby"],
                                newfilename = newfilename,
                                extension = extension,
                                createdate = DateTime.Now
                            };
                            db.core_temp_attachment.Add(coreTemp);


                            if (File.Exists(filePath))
                            {
                                db.SaveChanges();
                                result = coreTemp;
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.NotFound, "File Not Found");
                            }
                           
                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> TestUpload()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {

                var httpRequest = HttpContext.Current.Request;
                var drivePath = db.core_drivepath.Where(m => m.id == 9).FirstOrDefault();
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    object result = null;
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 8; //Size = 4 MB  

                        // IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();



                        //if (!AllowedFileExtensions.Contains(extension))
                        //{

                        //    var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                        //    dict.Add("error", message);
                        //    return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        //}
                        if (!Request.Content.IsMimeMultipartContent())
                        {
                            this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");

                            dict.Add("error", message);
                            return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                        }
                        else
                        {
                            string filename = postedFile.FileName.Replace(extension, "");
                            string newfilename = DateTime.Now.Ticks.ToString();

                            // var filePath = HttpContext.Current.Server.MapPath("~/Userimage/" + postedFile.FileName + extension);
                            // var filePath = HttpContext.Current.Server.MapPath("~/upload/converted/" + newfilename + extension);

                            var filePath = drivePath.drive_path + newfilename + extension;
                            postedFile.SaveAs(filePath);

                            core_temp_attachment coreTemp = new core_temp_attachment
                            {
                                appId = int.Parse(httpRequest.Form["appId"]),
                                refmodule = httpRequest.Form["refmodule"],
                                userId = int.Parse(httpRequest.Form["userId"]),
                                filename = filename,
                                filepath = drivePath.drive_path,
                                status = "A",
                                createdby = httpRequest.Form["createdby"],
                                newfilename = newfilename,
                                extension = extension,
                                createdate = DateTime.Now
                            };
                            db.core_temp_attachment.Add(coreTemp);
                            db.SaveChanges();
                            result = coreTemp;
                        }
                    }

                    var message1 = string.Format("Image Updated Successfully.");
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                var res = string.Format("Please Upload a image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (Exception ex)
            {
                var res = string.Format("some Message");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }



        [HttpGet]
        public HttpResponseMessage getTempFile(int appId, string refmodule, int userId)
        {
            object tempFile = null;
            try
            {
                if (string.IsNullOrEmpty(refmodule))
                {
                    tempFile = db.core_temp_attachment.Where(m => m.appId == appId && m.userId==userId && m.status=="A").ToList();
                }
                else
                {
                    tempFile = db.core_temp_attachment.Where(m => m.appId == appId && m.refmodule == refmodule && m.userId == userId && m.status == "A").ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, tempFile);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        public HttpResponseMessage updateTempFile(core_temp_attachment coretempfile)
        {
            try
            {
                db.Entry(coretempfile).State = EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // You could extract these two private methods to a separate utility class since
        // they do not really belong to a controller class but that is up to you
        private MultipartFormDataStreamProvider GetMultipartProvider()
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    string otherFile = db.core_drivepath.Where(aa => aa.id == 8).FirstOrDefault().drive_path;

                    var root = otherFile;// HttpContext.Current.Server.MapPath(otherFile);
                    Directory.CreateDirectory(root);
                    return new MultipartFormDataStreamProvider(root);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
          
        }

        // Extracts Request FormatData as a strongly typed model
        private object GetFormData<T>(MultipartFormDataStreamProvider result)
        {
            if (result.FormData.HasKeys())
            {
                var unescapedFormData = Uri.UnescapeDataString(result.FormData.GetValues(0).FirstOrDefault() ?? String.Empty);
                if (!String.IsNullOrEmpty(unescapedFormData))
                    return JsonConvert.DeserializeObject<T>(unescapedFormData);
            }

            return null;
        }

        private string GetDeserializedFileName(MultipartFileData fileData)
        {
            var fileName = GetFileName(fileData);
            return JsonConvert.DeserializeObject(fileName).ToString();
        }

        public string GetFileName(MultipartFileData fileData)
        {
            return fileData.Headers.ContentDisposition.FileName;
        }


        #region "Remap attachfile"

        [HttpGet]
        public HttpResponseMessage Fixpath()
        {
            try
            {

                db.Configuration.LazyLoadingEnabled = false;

                string mifs_FileDrive = db.core_drivepath.Where(aa => aa.id == 9).FirstOrDefault().drive_path;

                var attachFile = Directory.GetFiles(mifs_FileDrive);

                foreach(var row in attachFile)
                {


                    string filename = Path.GetFileNameWithoutExtension(row);
                    string fileExtension = System.IO.Path.GetExtension(row);

                    var attachInfo = db.core_attachments.Where(m=> m.newfilename==filename);

                    if (attachInfo.Count() > 0)
                    {
                        int responseId = attachInfo.First().transactionId;
                        //get response details base from response id
                        var responseInfo = db.mifs_response.Where(m => m.responseId == responseId);

                        if(responseInfo.Count() > 0)
                        {
                            int issueId = responseInfo.First().issueId;
                            //get issue details 
                            var issueInfo = db.mifs_issues.Where(m => m.issueId == issueId).FirstOrDefault();

                            string specificPath = mifs_FileDrive + "PaloUp\\" + issueInfo.topicId.ToString() + "\\" + issueInfo.issueId + "\\";

                            if (!Directory.Exists(specificPath))//check if the directory is not exist
                                Directory.CreateDirectory(specificPath);//create directory

                            //move attach file
                            File.Move(row, specificPath + filename + fileExtension);
                        }

         
                    }

                }


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
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

