using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using MLCloud;
using Newtonsoft.Json; // Install Newtonsoft.Json with NuGet

public class Program
{
    private static readonly string subscriptionKey = "839b2465040a48229f8c3b83327aa4c3";
    private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";
    private static readonly string route = "/translate?api-version=3.0&from=ro&to=en&to=fr";
    private static readonly string textToTranslate = "Dana are multe alune si se joaca Minecraft";

    // Add your location, also known as region. The default is global.
    // This is required if using a Cognitive Services resource.
    private static readonly string location = "westeurope";

    static async Task Main(string[] args)
    {
        // Input and output languages are defined as parameters.
        
        object[] body = new object[] { new { Text = textToTranslate } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            // Build the request.
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(endpoint + route);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", location);

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request);
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<List<Translations>>(result);
            Console.WriteLine(result);

            var translations = obj.First().translations;

            foreach(var tr in translations)
            {
                await textToSpeech(tr.text, tr.to);
            }
            
        }

        
    }

    public static async Task textToSpeech(string input, string language)
    {
        var config = SpeechConfig.FromSubscription("a1dbbe3198644d0f8f6ce645527b89fa", "westeurope");
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

        var path = $"c:/projects/audiofiles/audio_{language}.wav";
        using (FileStream fs = File.Create(path))
        {
            Console.WriteLine("File Created");
        }
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm);
        try
        {


            var synthesizer = new SpeechSynthesizer(config, null);


            var result = await synthesizer.StartSpeakingTextAsync(input);

            var stream = AudioDataStream.FromResult(result);
            await stream.SaveToWaveFileAsync(path);
        }
        catch (Exception e)
        {
            Console.WriteLine("err");
        }
        Console.WriteLine("audio");

    }
}
