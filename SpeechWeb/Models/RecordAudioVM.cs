using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Models
{
    public class RecordAudioVM
    {
        public string SampleText { get; set; }
        public string Language { get; set; }
        public int Encoding { get; set; }
        public string Result { get; set; }

     //   public object AudioSrc { get; set; }
        public string AudioFileName { get; set; }
    }
}
