using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using SpeechWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechWeb.Controllers
{
    public class FullFunctionController : Controller
    {
        IWebHostEnvironment _hostingEnvironment;
        IConfiguration _Configuration;
        public FullFunctionController(IWebHostEnvironment hostingEnvironment, IConfiguration Configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _Configuration = Configuration;
        }
        //public IActionResult Index()
        //{
        //    return View();
        //}
        [HttpGet]
        public IActionResult Index()
        {
            GetLanguages();
            GetOutputFile(null);
            ViewBag.FormIndex = "";
            return View();
        }
        [HttpPost]
        public IActionResult Index(FullFunctionVM model)
        {

            if (ModelState.IsValid)
            {
                if (model.FormIndex == 1)
                {
                    string fileName = ReadText(model.InputText, model.language, model.Gender);
                    model.OutputFile = fileName;
                }
                if (model.FormIndex == 2)
                {
                    string result = AnalyzeAudio(model.AudioFileName, model.language, 0, model.InputText);
                    model.Result = result;
                    GetResult(model);
                    GetAudioSrc(model);
                }
                if (model.FormIndex == 3)
                {
                    string result = AnalyzeFile(model.FileToUpload, model.language, 0, model.InputText );
                    model.Result = result;
                    GetResult(model);
                }

                    ViewBag.FormIndex = model.FormIndex;
            }
            GetLanguages();
            GetOutputFile(model);

            

            return View(model);
        }

        string ReadText(string inputText, string language, string Gender)
        {
            // string jsonPath = _hostingEnvironment.ContentRootPath + @"\astral-comfort-339113-ffabf3f1a626.json";
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
                SsmlGender = Gender == "F" ? SsmlVoiceGender.Female : SsmlVoiceGender.Male
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Write the response to the output file
            //string FileName = @"wwwroot\output\" + inputText + "_" + language + "_" + Gender + ".mp3";
            string FileName = @"wwwroot\output\" + Guid.NewGuid().ToString() + ".mp3";

            using (var output = System.IO.File.Create(FileName))
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
        void GetOutputFile(FullFunctionVM model)
        {
            string file = "";
            if (model != null)
                file = model.OutputFile;

            if (file != null && file != "")
                ViewBag.OutputFile = file.Replace(@"wwwroot\", "");
            else
                ViewBag.OutputFile = "";

        }

        void GetResult(FullFunctionVM model)
        {
            string result = model.Result;

            ViewBag.Result = result;
            ViewBag.alert = "success";
            if (model.Result != "Perfect")
                ViewBag.alert = "danger";
        }
        void GetAudioSrc(FullFunctionVM model)
        {
            string src = "";
            if (model.AudioFileName != "" && model.AudioFileName != null)
                src = "/audio/" + model.AudioFileName;

            ViewBag.src = src;
        }

        string AnalyzeAudio(string AudioFileName, string Language, int Encoding, string SampleText)
        {
            if (AudioFileName == null)
                return "";
            string json = _Configuration.GetSection("MyCustomSettings").GetSection("CredentialJSONFile").Value;
            string jsonPath = _hostingEnvironment.ContentRootPath + @"\" + json;
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            ///
            var speech = SpeechClient.Create();

            var config = new RecognitionConfig
            {
                // Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                Encoding = (RecognitionConfig.Types.AudioEncoding)Encoding,
                //SampleRateHertz = 16000,
                LanguageCode = Language

            };
            //
            string audioDir = Path.Combine(_hostingEnvironment.WebRootPath, "audio");
            string filePath = Path.Combine(audioDir, AudioFileName);

            var audio = RecognitionAudio.FromFile(filePath);


            RecognizeResponse response = null;
            try
            {
                response = speech.Recognize(config, audio);
            }
            catch (Exception ex)
            {
                //    System.IO.File.Delete(filePath);
                return ex.Message;
            }
            string data = "";
            foreach (var result in response.Results)
            {
                //foreach (var alternative in result.Alternatives)
                //{
                //    MessageBox.Show(alternative.Transcript);
                //}
                string s = result.Alternatives.OrderByDescending(c => c.Confidence).First().Transcript;
                data = s;
            }

            ///
            //System.IO.File.Delete(filePath);
            return data.ToLower().Trim() == SampleText.ToLower().Trim() ? "Perfect" : "Wrong Data:'" + data + "'";

        }

        string AnalyzeFile(IFormFile FileToUpload, string Language, int Encoding, string SampleText)
        {
            string json = _Configuration.GetSection("MyCustomSettings").GetSection("CredentialJSONFile").Value;
            string jsonPath = _hostingEnvironment.ContentRootPath + @"\" + json;
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            ///
            var speech = SpeechClient.Create();

            var config = new RecognitionConfig
            {
                //Encoding = RecognitionConfig.Types.AudioEncoding.,
                Encoding = (RecognitionConfig.Types.AudioEncoding)Encoding,
                //// SampleRateHertz = 16000,
                LanguageCode = Language

            };
            //
            string inputs = Path.Combine(_hostingEnvironment.WebRootPath, "input");
            string filePath = Path.Combine(inputs, FileToUpload.FileName);
            Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            //fileStream.Position = 0;
            fileStream.Flush();
            FileToUpload.CopyTo(fileStream);
            fileStream.Close();
            //var audio = RecognitionAudio.FromStorageUri("gs://cloud-samples-tests/speech/brooklyn.flac");
            var audio = RecognitionAudio.FromFile(filePath);


            RecognizeResponse response = null;
            try
            {
                response = speech.Recognize(config, audio);
            }
            catch (Exception ex)
            {
                System.IO.File.Delete(filePath);
                return ex.Message;
            }
            string data = "";
            foreach (var result in response.Results)
            {
                //foreach (var alternative in result.Alternatives)
                //{
                //    MessageBox.Show(alternative.Transcript);
                //}
                string s = result.Alternatives.OrderByDescending(c => c.Confidence).First().Transcript;
                data = s;
            }

            ///
            System.IO.File.Delete(filePath);
            return data.ToLower().Trim() == SampleText.ToLower().Trim() ? "Perfect" : "Wrong Data:'" + data + "'";

        }
    }

}
