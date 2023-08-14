using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PaloUpAPI.Models
{
    public class AppsImageModel
    {
        public int? interval { get; set; }
        public List<ImgFilename> filePath { get; set; }
    }
    
    public class ImgFilename
    {
        public int? fileid { get; set; }
        public string filename { get; set; }
        public string fileUrl { get; set; }
        public string fileOriginalUrl { get; set; }
        public bool fileselect { get; set; }
    }
}