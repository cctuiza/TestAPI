using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using PaloUpAPI.Class;
using PaloUpAPI.Models;

namespace PaloUpAPI.Controllers
{
    public class PaloUpNotMemberController : ApiController
    {
        private PALOUPEntities db = new PALOUPEntities();

        // GET: api/PaloUpNotMember
   
        //public IQueryable<NotMembers> Getmifs_vNotMember()
        //{
        //    return db.mifs_vNotMember.Select(mm => new NotMembers  { EmpID = mm.empl_id, EmpName = mm.employeename2,  EmpEmail = mm.emailadd, Position = mm.positionname,SBU = mm.corporate, ContactNo = mm.employeepresentcontact });
        //}

        [HttpGet]
        [Route("api/PaloUpNotMember")]
        public HttpResponseMessage Getmifs_vNotMember()
        {
            try
            {
                var palonotmembers = db.mifs_vNotMember.Select(mm => new NotMembers { EmpID = mm.empl_id, EmpName = mm.employeename2, EmpEmail = mm.emailadd, Position = mm.positionname, SBU = mm.corporate, ContactNo = mm.employeepresentcontact });










                return Request.CreateResponse(HttpStatusCode.OK, palonotmembers.ToList());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("get-view-data")]
        public HttpResponseMessage getviewdata()
        {
            try
            {

                var sql = ((System.Data.Entity.Core.Objects.ObjectQuery)db.core_vEmployeeNotUser.Select(mm => new { mm.emailadd }))
           .ToTraceString();

                return Request.CreateResponse(HttpStatusCode.OK, sql);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);


            }

        }



        // GET: api/PaloUpNotMember/5
        [ResponseType(typeof(mifs_vNotMember))]
        public IHttpActionResult Getmifs_vNotMember(string id)
        {
            mifs_vNotMember mifs_vNotMember = db.mifs_vNotMember.Find(id);
            if (mifs_vNotMember == null)
            {
                return NotFound();
            }

            return Ok(mifs_vNotMember);
        }

        // PUT: api/PaloUpNotMember/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putmifs_vNotMember(string id, mifs_vNotMember mifs_vNotMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mifs_vNotMember.empl_id)
            {
                return BadRequest();
            }

            db.Entry(mifs_vNotMember).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mifs_vNotMemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/PaloUpNotMember
        [ResponseType(typeof(mifs_vNotMember))]
        public IHttpActionResult Postmifs_vNotMember(mifs_vNotMember mifs_vNotMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.mifs_vNotMember.Add(mifs_vNotMember);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (mifs_vNotMemberExists(mifs_vNotMember.empl_id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = mifs_vNotMember.empl_id }, mifs_vNotMember);
        }

        // DELETE: api/PaloUpNotMember/5
        [ResponseType(typeof(mifs_vNotMember))]
        public IHttpActionResult Deletemifs_vNotMember(string id)
        {
            mifs_vNotMember mifs_vNotMember = db.mifs_vNotMember.Find(id);
            if (mifs_vNotMember == null)
            {
                return NotFound();
            }

            db.mifs_vNotMember.Remove(mifs_vNotMember);
            db.SaveChanges();

            return Ok(mifs_vNotMember);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool mifs_vNotMemberExists(string id)
        {
            return db.mifs_vNotMember.Count(e => e.empl_id == id) > 0;
        }
    }
}