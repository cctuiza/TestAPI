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
using PaloUpAPI.Models;

namespace PaloUpAPI.Controllers
{
    public class MembersController : ApiController
    {
        private PALOUPEntities db = new PALOUPEntities();

        // GET: api/Members
        [HttpGet]
        [Route("api/Members")]
        public IQueryable<mifs_members> Getmifs_members()
        {
            return db.mifs_members.OrderBy(mm => mm.fullname);
        }

        // GET: api/Members/5
        [ResponseType(typeof(mifs_members))]
        public IHttpActionResult Getmifs_members(string id)
        {
            mifs_members mifs_members = db.mifs_members.Find(id);
            if (mifs_members == null)
            {
                return NotFound();
            }

            return Ok(mifs_members);
        }

        // PUT: api/Members/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putmifs_members(string id, mifs_members mifs_members)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mifs_members.memberId)
            {
                return BadRequest();
            }

            db.Entry(mifs_members).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mifs_membersExists(id))
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

        // POST: api/Members
        [ResponseType(typeof(mifs_members))]
        public IHttpActionResult Postmifs_members(mifs_members mifs_members)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.mifs_members.Add(mifs_members);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (mifs_membersExists(mifs_members.memberId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = mifs_members.memberId }, mifs_members);
        }

        // DELETE: api/Members/5
        [ResponseType(typeof(mifs_members))]
        public IHttpActionResult Deletemifs_members(string id)
        {
            mifs_members mifs_members = db.mifs_members.Find(id);
            if (mifs_members == null)
            {
                return NotFound();
            }

            db.mifs_members.Remove(mifs_members);
            db.SaveChanges();

            return Ok(mifs_members);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool mifs_membersExists(string id)
        {
            return db.mifs_members.Count(e => e.memberId == id) > 0;
        }



    }
}