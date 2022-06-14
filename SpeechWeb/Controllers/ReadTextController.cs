using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using SpeechWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Controllers
{
    public class ReadTextController : Controller
    {
        IWebHostEnvironment _hostingEnvironment;
        IConfiguration _Configuration;
        public ReadTextController(IWebHostEnvironment hostingEnvironment, IConfiguration Configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _Configuration = Configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            GetLanguages();
            GetOutputFile(null);
            return View();
        }
        [HttpPost]
        public IActionResult Create(ReadTextVM model)
        {

            if (ModelState.IsValid)
            {
             string fileName=   ReadText(model.InputText, model.language, model.Gender);
                model.OutputFile = fileName;
            }
            GetLanguages();
            GetOutputFile(model);
            return View();
        }

        string  ReadText(string inputText,string language,string Gender)
        {
            //  string jsonPath = _hostingEnvironment.ContentRootPath + @"\astral-comfort-339113-ffabf3f1a626.json"; 
            string json = _Configuration.GetSection("MyCustomSettings").GetSection("CredentialJSONFile").Value;
            string jsonPath = _hostingEnvironment.ContentRootPath + @"\" + json;

            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);

            var client = TextToSpeechClient.Create();

            // The input to be synthesized, can be provided as text or SSML.
            var input = new SynthesisInput
            {
                Text = inputText
            };

            // Build the voice request.
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = language,
                SsmlGender = Gender=="F"? SsmlVoiceGender.Female:SsmlVoiceGender.Male
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Write the response to the output file
            string FileName = @"wwwroot\output\" + inputText + "_" + language + "_" + Gender+".mp3";
            using (var output = System.IO.File.Create( FileName ))
            {
                response.AudioContent.WriteTo(output);
            }
            return FileName;
        }

        void GetLanguages()
        {
            List<SelectListItem> langs = new()
            {
                new SelectListItem { Value = "en-US", Text = "English" },
                new SelectListItem { Value = "ar-EG", Text = "Arabic" }
            };
            
            ViewBag.langs = langs;
          
        }
        void GetOutputFile(ReadTextVM model)
        {
            string file = "";
            if (model != null)
                file = model.OutputFile;

            ViewBag.OutputFile = file.Replace (@"wwwroot\","");
        }
    }
}
