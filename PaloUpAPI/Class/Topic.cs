using PaloUpAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Class
{
    public class Topic
    {
        public int topicId { get; set; }
        public int groupId { get; set; }
        public string hashcode { get; set; }
        public string aliascode { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public int topiclevel { get; set; }
        public int fupfrequency { get; set; }
        public string occurence { get; set; }
        public string created_by { get; set; }
        public System.DateTime create_date { get; set; }
        public byte topicscope { get; set; }
        public string bgcolor { get; set; }
        public string classification { get; set; }
        public string profile { get; set; }
        public int statchangecounterlimit { get; set; }


    }


    public class TopicList
    {
        public int topicId { get; set; }
        public string status { get; set; }
        public int year { get; set; }

    }

    public class TopicMembers
    {
        public List<mifs_topicmembers> topicmembersdetails { get; set; }
        public string created_by { get; set; }
        public int topicId { get; set; }

        public string memberId { get; set; }
    }
}