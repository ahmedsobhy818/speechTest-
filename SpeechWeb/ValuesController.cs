using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public object  Get()
        {
            return new string[] { "ahmed", "mohamed", "marwa" };
        }
        [HttpPost]
        public string Post()
        {
            return "";
        }
        [HttpPost]
        public string Post(object data)
        {
            return "";
        }

    }
}
