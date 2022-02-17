using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Models
{
    public class ReadTextVM
    {
        [Required]
        public string InputText { get; set; }
        public string OutputFile { get; set; }
        public string Gender { get; set; }
        public string language { get; set; }
       
}
}
