using Google.Cloud.Speech.V1;
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
    public class CheckPronunciationController : Controller
    {
        IWebHostEnvironment _hostingEnvironment;
        IConfiguration _Configuration;
        public CheckPronunciationController(IWebHostEnvironment hostingEnvironment, IConfiguration Configuration)
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
            var model = new CheckFileVM { SampleText="hello i am at home"};
            GetLanguages();
            GetEncodings();
            GetResult(model);
            return View(model );
        }
        [HttpPost]
        public IActionResult Create(CheckFileVM model)
        {

            if (ModelState.IsValid)
            {
                string result =  AnalyzeFile(model.FileToUpload , model.Language, model.Encoding ,model.SampleText );
                model.Result  = result;
            }
            GetLanguages();
            GetEncodings();
            GetResult(model );
            return View(model );
        }
         string AnalyzeFile( IFormFile FileToUpload , string Language, int Encoding , string SampleText )
        {
            string json = _Configuration.GetSection("MyCustomSettings").GetSection("CredentialJSONFile").Value;
            string jsonPath = _hostingEnvironment.ContentRootPath + @"\" + json ;
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonPath);
            ///
            var speech = SpeechClient.Create();

            var config = new RecognitionConfig
            {
                 //Encoding = RecognitionConfig.Types.AudioEncoding.,
                Encoding = (RecognitionConfig.Types.AudioEncoding) Encoding,
              //// SampleRateHertz = 16000,
                LanguageCode = Language

            };
            //
            string inputs = Path.Combine(_hostingEnvironment.WebRootPath ,"input");
            string filePath = Path.Combine(inputs, FileToUpload.FileName);
            Stream fileStream = new FileStream(filePath,FileMode.Create,FileAccess.Write  );
            //fileStream.Position = 0;
            fileStream.Flush();
            FileToUpload.CopyTo(fileStream  );
            fileStream.Close();
            //var audio = RecognitionAudio.FromStorageUri("gs://cloud-samples-tests/speech/brooklyn.flac");
            var audio = RecognitionAudio.FromFile(filePath );


            RecognizeResponse  response=null;
            try
            {
                response = speech.Recognize(config, audio);
            }
            catch(Exception ex)
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
                string s=result.Alternatives.OrderByDescending(c => c.Confidence).First().Transcript;
                data= s;
            }

            ///
            System.IO.File.Delete(filePath);
            return data.ToLower().Trim() == SampleText.ToLower().Trim() ? "Perfect" : "Wrong Data:'" + data + "'";

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
        void GetEncodings()
        {
            List<SelectListItem> encs = new()
            {                                 
                new SelectListItem { Value = "0", Text = "EncodingUnspecified" },
                new SelectListItem { Value = "1", Text = "Linear16" },
                new SelectListItem { Value = "2", Text = "Flac" },
                new SelectListItem { Value = "3", Text = "Mulaw" },
                new SelectListItem { Value = "4", Text = "Amr" },
                new SelectListItem { Value = "5", Text = "AmrWb" },
                new SelectListItem { Value = "6", Text = "OggOpus" },
                new SelectListItem { Value = "7", Text = "SpeexWithHeaderByte" },
                new SelectListItem { Value = "9", Text = "WebmOpus" }
            };
            /*
             //summary
            //not specified
             EncodingUnspecified = 0,
                //
                // Summary:
                //     Uncompressed 16-bit signed little-endian samples (Linear PCM).
                Linear16 = 1,
                //
                // Summary:
                //     `FLAC` (Free Lossless Audio Codec) is the recommended encoding because it is
                //     lossless--therefore recognition is not compromised--and requires only about half
                //     the bandwidth of `LINEAR16`. `FLAC` stream encoding supports 16-bit and 24-bit
                //     samples, however, not all fields in `STREAMINFO` are supported.
                Flac = 2,
                //
                // Summary:
                //     8-bit samples that compand 14-bit audio samples using G.711 PCMU/mu-law.
                Mulaw = 3,
                //
                // Summary:
                //     Adaptive Multi-Rate Narrowband codec. `sample_rate_hertz` must be 8000.
                Amr = 4,
                //
                // Summary:
                //     Adaptive Multi-Rate Wideband codec. `sample_rate_hertz` must be 16000.
                AmrWb = 5,
                //
                // Summary:
                //     Opus encoded audio frames in Ogg container ([OggOpus](https://wiki.xiph.org/OggOpus)).
                //     `sample_rate_hertz` must be one of 8000, 12000, 16000, 24000, or 48000.
                OggOpus = 6,
                //
                // Summary:
                //     Although the use of lossy encodings is not recommended, if a very low bitrate
                //     encoding is required, `OGG_OPUS` is highly preferred over Speex encoding. The
                //     [Speex](https://speex.org/) encoding supported by Cloud Speech API has a header
                //     byte in each block, as in MIME type `audio/x-speex-with-header-byte`. It is a
                //     variant of the RTP Speex encoding defined in [RFC 5574](https://tools.ietf.org/html/rfc5574).
                //     The stream is a sequence of blocks, one block per RTP packet. Each block starts
                //     with a byte containing the length of the block, in bytes, followed by one or
                //     more frames of Speex data, padded to an integral number of bytes (octets) as
                //     specified in RFC 5574. In other words, each RTP header is replaced with a single
                //     byte containing the block length. Only Speex wideband is supported. `sample_rate_hertz`
                //     must be 16000.
                SpeexWithHeaderByte = 7,
                //
                // Summary:
                //     Opus encoded audio frames in WebM container ([OggOpus](https://wiki.xiph.org/OggOpus)).
                //     `sample_rate_hertz` must be one of 8000, 12000, 16000, 24000, or 48000.
                WebmOpus = 9
             */
            ViewBag.encodings = encs;
        }
        void GetResult(CheckFileVM model)
        {
            string result = model.Result;
           
            ViewBag.Result = result ;
            ViewBag.alert = "success";
            if (model.Result != "Perfect")
                ViewBag.alert = "danger";
        }
        
        /////////////////////////////////////////////////////////////////
        
        public IActionResult Record()
        {
            var model = new RecordAudioVM  { SampleText = "hello i am at home" };
            GetLanguages();
            GetEncodings();
            GetResult(model);
            GetAudioSrc(model);
            return View(model);
            
        }
        [HttpPost]
        public IActionResult Record(RecordAudioVM model)
        {

            if (ModelState.IsValid)
            {
                string result = AnalyzeAudio (model.AudioFileName, model.Language, model.Encoding, model.SampleText);
                model.Result = result;
            }
            GetLanguages();
            GetEncodings();
            GetResult(model);
            GetAudioSrc(model);
            return View(model);
        }
        [HttpPost]
        public /*IActionResult*/ string ReadMicrophone(/*string audioname,*/ string chunks)
        {
            if (chunks == null)
                return "";
            string audioname = Guid.NewGuid().ToString()+".mp3";

            string fileNameWitPath = Path.Combine(_hostingEnvironment.WebRootPath, "audio", audioname );

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
            return audioname;
        }
        void GetResult(RecordAudioVM  model)
        {
            string result = model.Result;

            ViewBag.Result = result;
            ViewBag.alert = "success";
            if (model.Result != "Perfect")
                ViewBag.alert = "danger";
        }
        void GetAudioSrc(RecordAudioVM model)
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
    }
    }
