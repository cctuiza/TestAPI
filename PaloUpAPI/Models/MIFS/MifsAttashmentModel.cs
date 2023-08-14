using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MifsAttashmentModel
    {
        public HttpPostedFileBase file { get; set; }
        public int issueresponseId { get; set; }
        public string created_by { get; set; }
    }
}