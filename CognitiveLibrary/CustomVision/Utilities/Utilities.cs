using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace CognitiveLibrary.Utilities
{
    public class Utilities
    {
        public Utilities()
        {

        }

        public static byte[] GetImagesAsByteArrayFromUri(string contentUri)
        {
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(contentUri);
            return imageBytes;
        }

        static async Task<string> GetAuthenticationToken(string key)
        {
            string endpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                var response = await client.PostAsync(endpoint, null);
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }
        }

        public static async Task<string> TranslateText(string inputText, string language)
        {
            string accessToken = await GetAuthenticationToken(Constants.TranslationAPIKey);
            string url = "http://api.microsofttranslator.com/v2/Http.svc/Translate";
            string query = $"?text={System.Net.WebUtility.UrlEncode(inputText)}&to={language}&contentType=text/plain";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.GetAsync(url + query);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return "Failed: " + result;

                var translatedText = XElement.Parse(result).Value;
                return translatedText;
            }
        }
    }
}