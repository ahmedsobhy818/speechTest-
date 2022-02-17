using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Models
{
    public class CheckFileVM
    {
        public string SampleText { get; set; }
        public string Language { get; set; }
        public int Encoding { get; set; }
        public string Result { get; set; }

        public IFormFile FileToUpload { get; set; }
    }
}
