using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Models
{
    public class ExamVM
    {
        [Required]
        public IFormFile FileToUpload { get; set; }
        
        [Range(0, 3, ErrorMessage = "Please enter a number  between 0 and 3")]
        public int nMistakes { get; set; }

        public bool Pronounciation { get; set; }
        public bool PronounciationCheck { get; set; }

        public string Error { get; set; }

        public string language { get; set; }

        public List<ExamItem> Questions { get; set; }

        public bool ExamStarted { get; set; }
    }

    public class ExamItem
    {
        public string language1 { get; set; }
        public string language2 { get; set; }
        public string type { get; set; }
    }
}
