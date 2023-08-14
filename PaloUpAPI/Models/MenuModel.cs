using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class MenuModel
    {
        public string name { get; set; }
        public string type { get; set; }
        public string stage { get; set; }
        public string icon { get; set; }
        public List<MenuModel> pages { get; set; }
    }


   
}