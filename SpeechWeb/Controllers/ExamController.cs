using Google.Cloud.TextToSpeech.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpeechWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Cloud.Speech.V1;
using System.Threading.Tasks;

namespace SpeechWeb.Controllers
{
    public class ExamController : Controller
    {
        IWebHostEnvironment _hostingEnvironment;
        IConfiguration _Configuration;
        public ExamController(IWebHostEnvironment hostingEnvironment, IConfiguration Configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _Configuration = Configuration;
        }
        public IActionResult Index()
        {
            GetError(null);
            GetCount(null);
            return View();
        }

        [HttpPost]
        public IActionResult Index(ExamVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string inputs = Path.Combine(_hostingEnvironment.WebRootPath, "input");
                    string filePath = Path.Combine(inputs, model.FileToUpload.FileName);
                    if (!filePath.ToLower().EndsWith (".txt"))
                        throw new Exception("Please upload text file");

                    Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    fileStream.Flush();
                    model.FileToUpload.CopyTo(fileStream);
                    fileStream.Close();

                    ///
                    model.language = model.FileToUpload.FileName.Split(new string[] { "." },StringSplitOptions.None ).First();
                    StreamReader sr = new StreamReader(filePath);
                    List<ExamItem> items = new List<ExamItem>();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        string[] ar = line.Split(new string[] { "|" }, StringSplitOptions.None);
                        items.Add(new ExamItem { language1=ar[0] , language2=ar[1], type=ar[2] });
                        
                    }
                    sr.Close();

                    if(items.Count==0)
                        throw new Exception("Input File is Empty");

                    model.Questions = items;
                    model.ExamStarted = true;
                }
                catch(Exception ex)
                {
                    model.Error = ex.Message;
                    model.ExamStarted = false;
                }

            }
            GetCount(model);
            GetError(model);
            return View(model);
         }

        void GetError(ExamVM model)
        {
            if (model == null) { 
                ViewBag.Error = null;
            return; 
            }
            string Error = "";
            if (model.Error  != "" && model.Error  != null)
                Error = model.Error ;

            ViewBag.Error = Error;
        }

        void GetCount(ExamVM model)
        {
            if (model == null)
            {
                ViewBag.Count = 0;
                return;
            }
            int count = 0;
            count = model.Questions.Count;

            ViewBag.Count = count;
        }

        [HttpPost]
        public string ReadText(string statement, string language, int index)
        {
            string jsonPath = _hostingEnvironment.ContentRootPath + @"\astral-comfort-339113-ffabf3f1a626.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);

            var client = TextToSpeechClient.Create();

            // The input to be synthesized, can be provided as text or SSML.
            var input = new SynthesisInput
            {
                Text = statement
            };

            // Build the voice request.
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = language,
                SsmlGender =  SsmlVoiceGender.Male
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Write the response to the output file
            string FileName = @"wwwroot\output\" + index + ".mp3";
            using (var output = System.IO.File.Create(FileName))
            {
                response.AudioContent.WriteTo(output);
            }
            return index + ".mp3";
        }

        [HttpPost]
        public  string ReadMicrophone( string chunks, string Language, string SampleText)
        {
            if (chunks == null)
                return "";
            string audioname = Guid.NewGuid().ToString() + ".mp3";

            string fileNameWitPath = Path.Combine(_hostingEnvironment.WebRootPath, "audio", audioname);

            using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(chunks);
                    bw.Write(data);
                    bw.Close();
                }
                fs.Close();
            }
            //return Content(chunks);//this is for testing - sends back full chunk on success, would probably just want some confirm all is good message
            return AnalyzeFile(audioname, Language, 0, SampleText);
        }


        string AnalyzeFile(string FileToUpload, string Language, int Encoding, string SampleText)
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
            string dir = Path.Combine(_hostingEnvironment.WebRootPath, "audio");
            string filePath = Path.Combine(dir, FileToUpload);
            //Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            //fileStream.Position = 0;
            //fileStream.Flush();
            //FileToUpload.CopyTo(fileStream);
            //fileStream.Close();
            ////var audio = RecognitionAudio.FromStorageUri("gs://cloud-samples-tests/speech/brooklyn.flac");
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
