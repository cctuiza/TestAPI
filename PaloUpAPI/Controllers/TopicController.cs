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
    public class TopicController : ApiController
    {
        private PALOUPEntities db = new PALOUPEntities();

        // GET: api/Topic
        //[HttpGet]
        //[Route("api/Topic")]
        //public IQueryable<mifs_topic> Getmifs_topic()
        //{
        //    return db.mifs_topic.OrderBy(mm => mm.aliascode);
        //}


        [HttpGet]
        [Route("api/Topic")]
        public HttpResponseMessage Getmifs_topic()
        {
            try
            {
                var palotopics = db.mifs_topic.Select(mm => new Topic {
                    topicId =   mm.topicId ,
                    groupId  =  mm.groupId,
                    hashcode =    mm.hashcode,
                    aliascode = mm.aliascode,
                    description = mm.description,
                    status = mm.status,
                    topiclevel = mm.topiclevel,
                    fupfrequency = mm.fupfrequency,
                    occurence =   mm.occurence,
                    created_by = mm.created_by,
                    create_date = mm.create_date,
                    topicscope = mm. topicscope,
                    bgcolor = mm.bgcolor,
                    classification = mm.classification,
                    profile =  mm.profile,
                    statchangecounterlimit = mm.statchangecounterlimit
                    })
                    .OrderBy(mm=>mm.aliascode).ToList();

      
       




                return Request.CreateResponse(HttpStatusCode.OK, palotopics);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }






        // GET: api/Topic/5
        [ResponseType(typeof(mifs_topic))]
        public IHttpActionResult Getmifs_topic(int id)
        {
            mifs_topic mifs_topic = db.mifs_topic.Find(id);
            if (mifs_topic == null)
            {
                return NotFound();
            }

            return Ok(mifs_topic);
        }

        // PUT: api/Topic/5
        [ResponseType(typeof(void))]
        public IHttpActionResult Putmifs_topic(int id, mifs_topic mifs_topic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mifs_topic.topicId)
            {
                return BadRequest();
            }

            db.Entry(mifs_topic).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mifs_topicExists(id))
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

        // POST: api/Topic
        [ResponseType(typeof(mifs_topic))]
        public IHttpActionResult Postmifs_topic(mifs_topic mifs_topic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.mifs_topic.Add(mifs_topic);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = mifs_topic.topicId }, mifs_topic);
        }

        // DELETE: api/Topic/5
        [ResponseType(typeof(mifs_topic))]
        public IHttpActionResult Deletemifs_topic(int id)
        {
            mifs_topic mifs_topic = db.mifs_topic.Find(id);
            if (mifs_topic == null)
            {
                return NotFound();
            }

            db.mifs_topic.Remove(mifs_topic);
            db.SaveChanges();

            return Ok(mifs_topic);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool mifs_topicExists(int id)
        {
            return db.mifs_topic.Count(e => e.topicId == id) > 0;
        }
    }
}