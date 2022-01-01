using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;

namespace MLCloud
{
    public class AzureClient
    {
        private readonly HttpClient _httpClient;

        public AzureClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task ExecuteCall()
        {
            var request = PrepareRequest();

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            var deserializedResponse = JsonConvert.DeserializeObject<List<TranslationList>>(result);
            if (deserializedResponse != null)
            {
                var translations = deserializedResponse.First().Translations;

                foreach (var tr in translations)
                {
                    await TextToSpeech(tr.Text, tr.To);
                }
            }
            else
            {
                Console.WriteLine("There's something wrong with the deserialization of the response.");
            }
        }


        private async Task TextToSpeech(string input, string language)
        {
            var audioFilePath = $"E:/MASTER/ANUL 2/SEM1/Cloud/PrezentareCurs/Audio/audio_{language}.wav"; // var audioFilePath = $"c:/projects/audiofiles/audio_{language}.wav";

            var config = SpeechConfig.FromSubscription("a1dbbe3198644d0f8f6ce645527b89fa", Resources.Location);
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);

            switch (language)
            {
                case "en":
                    config.SpeechSynthesisLanguage = "en-US";
                    config.SpeechSynthesisVoiceName = "en-US-MichelleNeural";
                    break;
                case "fr":
                    config.SpeechSynthesisLanguage = "fr-FR";
                    config.SpeechSynthesisVoiceName = "fr-FR-HenriNeural";
                    break;
                default:
                    config.SpeechSynthesisLanguage = "en-US";
                    config.SpeechSynthesisVoiceName = "en-US-MichelleNeural";
                    break;
            }

            await using (File.Create(audioFilePath)) { }

            try
            {
                var synthesizer = new SpeechSynthesizer(config, null);
                var result = await synthesizer.StartSpeakingTextAsync(input);
                var stream = AudioDataStream.FromResult(result);
                await stream.SaveToWaveFileAsync(audioFilePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error when creating the audio file:" + e.Message);
            }
        }

        private HttpRequestMessage PrepareRequest()
        {
            var body = new object[]
            {
                new
                {
                    Text = Resources.TextToTranslate
                }
            };
            var requestBody = JsonConvert.SerializeObject(body);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(Resources.Endpoint + Resources.Route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", Resources.SubscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", Resources.Location);

            return request;
        }
    }
}
