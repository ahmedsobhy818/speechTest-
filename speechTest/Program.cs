using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Cloud.Speech.V1;
using Google.Cloud.Storage.V1;
using Google.Protobuf;
using System;
using System.IO;
using System.Threading;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace speechTest
{
    class Program
    {
         static void Main(string[] args)
        {
            string credential_path = @"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";

            string ev = System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (ev == null)
            {

                System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);

                var credential = GoogleCredential.GetApplicationDefault();
                var storage = StorageClient.Create(credential);
                // Make an authenticated API request.

                //


                //
                var buckets = storage.ListBuckets("astral-comfort-339113");// ("My First Project");
                foreach (var bucket in buckets)
                {
                    Console.WriteLine(bucket.Name);
                }
            }


            ///
            var speech = SpeechClient.Create();
            var config = new RecognitionConfig
            {
                // Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                Encoding = RecognitionConfig.Types.AudioEncoding.EncodingUnspecified ,
                SampleRateHertz = 16000,
                LanguageCode = LanguageCodes.English.UnitedStates,
                 
            };
            //var audio = RecognitionAudio.FromStorageUri("gs://cloud-samples-tests/speech/brooklyn.flac");
            var audio = RecognitionAudio.FromFile (@"e:\1.mp3");

            var response = speech.Recognize(config, audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    Console.WriteLine(alternative.Transcript);
                }
            }
        }
    }
    
}
