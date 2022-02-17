using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Models
{
    public class FullFunctionVM
    {

        public int FormIndex { get; set; }

        [Required]
        public string InputText { get; set; }
        public string OutputFile { get; set; }
        public string Gender { get; set; }
        public string language { get; set; }
        ///

       
        public string Result { get; set; }
        public string AudioFileName { get; set; }
        //
        public IFormFile FileToUpload { get; set; }
    }
}
