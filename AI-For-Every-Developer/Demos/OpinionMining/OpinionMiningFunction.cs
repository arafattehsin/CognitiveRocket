using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.TextAnalytics;
using Azure;
using System.Collections.Generic;
using System.Linq;

namespace OpinionMining
{
    public static class OpinionMiningFunction
    {
        [FunctionName("OpinionMiningFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            TextAnalyticsClient client = GetTextAnalyticsClient();
            string sentence = req.Query["sentence"];

            var sentiment = await AnalyzeSentimentAsync(client, sentence, includeOpinionMining: true);

            List<OpinionDTO> Opinions = new List<OpinionDTO>();
            var opinions = sentiment?.Sentences.SelectMany(s => s.Opinions);
            if (opinions != null && opinions.Any())
            {
                var minedOpinionList = opinions.Select(om => new OpinionDTO()
                {
                    Aspect = om.Target.Text,
                    Opinions = string.Join(", ", om.Assessments.Select(o => $"{o.Text} ({o.Sentiment.ToString("G")})"))
                });
                Opinions.AddRange(minedOpinionList);
            }

            var jsonResult = new
            {
                Sentiment = sentiment?.Sentiment.ToString("G"),
                Opinions = Opinions
            };

            return new OkObjectResult(jsonResult);
        }

        private static TextAnalyticsClient GetTextAnalyticsClient()
        {
            var apiKey = System.Environment.GetEnvironmentVariable("API_KEY", EnvironmentVariableTarget.Process);
            var endpointURL = System.Environment.GetEnvironmentVariable("ENDPOINT_URL", EnvironmentVariableTarget.Process);

            var credentials = !string.IsNullOrEmpty(apiKey) ? new AzureKeyCredential(apiKey) : null;
            var endpoint = !string.IsNullOrEmpty(endpointURL) && Uri.IsWellFormedUriString(endpointURL, UriKind.Absolute) ? new Uri(endpointURL) : null;

            if (apiKey != null && endpointURL != null)
                return new TextAnalyticsClient(endpoint, credentials);
            else
                return null;
        }

        public static async Task<DocumentSentiment> AnalyzeSentimentAsync(TextAnalyticsClient client, string input, string language = "en", bool includeOpinionMining = false)
        {
            var options = new AnalyzeSentimentOptions() { IncludeOpinionMining = includeOpinionMining };
            return await client.AnalyzeSentimentAsync(input, language, options);
        }

        public class OpinionDTO
        {
            public string Aspect { get; set; }
            public string Opinions { get; set; }
        }
    }
}
