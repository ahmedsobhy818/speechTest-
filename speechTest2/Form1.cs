using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Google.Cloud.Speech.V1.SpeechClient;

namespace speechTest2
{
    public partial class Form1 : Form
    {
       

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            return;
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string credential_path =Application.StartupPath + @"\\astral-comfort-339113-ffabf3f1a626.json";  //@"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);

            await StreamingMicRecognizeAsync();
        }

        WaveInEvent waveIn;
        object writeLock = new object();
        bool writeMore = true;
        StreamingRecognizeStream  streamingCall=null;
        async Task StreamingMicRecognizeAsync()//int seconds)
        {
            var speech = SpeechClient.Create();
             streamingCall = speech.StreamingRecognize();
            // Write the initial request with the config.
            await streamingCall.WriteAsync(
            new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        Encoding =
            RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "en",
                    },
                    InterimResults = true,
                }
            });
           

           // Read from the microphone and stream to API.
           //object writeLock = new object();
           //bool writeMore = true;
            waveIn = new NAudio.Wave.WaveInEvent();
           waveIn.DeviceNumber = 0;
           waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
           waveIn.DataAvailable +=
           (object sender, NAudio.Wave.WaveInEventArgs args) =>
           {
               lock (writeLock)
               {
                   if (!writeMore)
                   {
                       return;
                   }
                   streamingCall.WriteAsync(
                   new StreamingRecognizeRequest()
                   {
                       AudioContent = Google.Protobuf.ByteString
                   .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                   }).Wait();
               }
           };
            MessageBox.Show("Speak now.");
            waveIn.StartRecording();
           
//           await Task.Delay(TimeSpan.FromSeconds(seconds));
//           // Stop recording and shut down.
//           waveIn.StopRecording();
//           lock (writeLock)
//           {
//               writeMore = false;
//           }
//           await streamingCall.WriteCompleteAsync();
////await printResponses;
//            await PrintResponses(streamingCall);
            
}

       async    Task  PrintResponses(StreamingRecognizeStream streamingCall)
        {
            while (await streamingCall.GetResponseStream().MoveNextAsync(
               default(CancellationToken)))
            {
                foreach (var result in streamingCall.GetResponseStream()
                .Current.Results)
                {
                    //foreach (var alternative in result.Alternatives)
                    //{
                    //    Console.WriteLine(alternative.Transcript);

                    //}
                    if (result.IsFinal)
                    {
                        var top =
                        result.Alternatives.OrderBy(x => x.Confidence).First();
                        listBox1.Items.Add(top.Transcript);

                    }
                }
            }
            waveIn = null;
            writeLock = new object();
            writeMore = true;
            streamingCall = null;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"reading file : \mp3\2.mp3 "  );
            //string credential_path = @"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";
            string credential_path = Application.StartupPath + @"\\astral-comfort-339113-ffabf3f1a626.json";  //@"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            var speech = SpeechClient.Create();

            var config = new RecognitionConfig
            {
                // Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                Encoding = RecognitionConfig.Types.AudioEncoding.EncodingUnspecified,
               SampleRateHertz = 16000,
                 LanguageCode = LanguageCodes.English.UnitedStates
                 
            };
            //var audio = RecognitionAudio.FromStorageUri("gs://cloud-samples-tests/speech/brooklyn.flac");
            var audio = RecognitionAudio.FromFile(Application.StartupPath+ @"\mp3\2.mp3");

            var response = speech.Recognize(config, audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    MessageBox.Show (alternative.Transcript);
                }
            }

            ///

          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                string url = "https://maps.googleapis.com/maps/api/staticmap?center=31,31&zoom=9&size=400x400&key=AIzaSyAtMY4ACoQo__g-DkQmfA7txnp46Vm8SWk";
                wc.DownloadFile(url, @"e:\map.png");
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            waveIn.StopRecording();
            lock (writeLock)
            {
                writeMore = false;
            }
            await streamingCall.WriteCompleteAsync();
            //await printResponses;
            await PrintResponses(streamingCall);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //string credential_path = @"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";
            string credential_path = Application.StartupPath + @"\\astral-comfort-339113-ffabf3f1a626.json";  //@"C:\Users\hp-pc\source\repos\speechTest\speechTest\bin\Debug\net5.0\astral-comfort-339113-ffabf3f1a626.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);

            var client = TextToSpeechClient.Create();
          
            // The input to be synthesized, can be provided as text or SSML.
            var input = new SynthesisInput
            {
                Text = textBox1.Text 
            };

            // Build the voice request.
            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                SsmlGender = SsmlVoiceGender.Female
            };

            // Specify the type of audio file.
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request.
            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            // Write the response to the output file.
            using (var output = File.Create("output.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }
            MessageBox.Show("VOICE SAVED ON " + Application.StartupPath + "\\output.mp3");
        }
    }
    
}
