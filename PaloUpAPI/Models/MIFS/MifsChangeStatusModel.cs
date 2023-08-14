using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsChangeStatusModel
    {
        public int issueId { get; set; }
        public byte acknowledgedbysource { get; set; }
        public byte acknowledgebyrespondent { get; set; }
        public string newstatus { get; set; }
        public string reason { get; set; }
        public DateTime? parkdate { get; set; }
        public DateTime? resumedate { get; set; }

        public string responseid { get; set; }
        public string responseName { get; set; }

        public string responseId { get; set; }
       
        public string responseEmail { get; set; }


    }
}