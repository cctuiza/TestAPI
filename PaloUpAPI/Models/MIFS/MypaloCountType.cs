using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models.MIFS
{
    public class MypaloCountType
    {
        public string strType { get; set; }
        public int iOpen { get; set;}
        public int iClose { get; set; }
        public int iPark { get; set; }
        public int iDrop { get; set; }
    }
}