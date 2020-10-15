using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Net.Http; 
using System.Threading;
using System.Threading.Tasks;

namespace Middleware
{
    internal class SemanticLayer : IMiddleware
    {
        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if(context.Activity.Type is ActivityTypes.Message)
            {
                // simple middleware to add an additional send activity
                var user = UserState<User>.Get(context);

                user.Queries.Add(context.Activity.AsMessageActivity().Text);

                //if (user.Queries.Count > 50)
                //    // Send an email to the author with all respective
                //    // output of the sentiments, like score etc. 
                //    ExtractSentiments(user.Queries);
            }
            
            await next();
        }

        private double? ExtractSentiments(List<string> Queries)
        {
            // Create a client.
            ITextAnalyticsAPI client = new TextAnalyticsAPI(new ApiKeyServiceClientCredentials());
            client.AzureRegion = AzureRegions.Westus;

            MultiLanguageBatchInput multiLanguageBatchInput = new MultiLanguageBatchInput();
            multiLanguageBatchInput.Documents = new List<MultiLanguageInput>();

            for(int i = 0; i < Queries.Count; i++)
            {
                multiLanguageBatchInput.Documents.Add(new MultiLanguageInput("en", i.ToString(), Queries[i]));
            }

            SentimentBatchResult sentimentBatchResult = client.SentimentAsync(multiLanguageBatchInput).Result;
            double? score = 0;

            // Printing sentiment results
            foreach (var document in sentimentBatchResult.Documents)
                score += document.Score;

            return (score / sentimentBatchResult.Documents.Count);
        }
    }


    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", "adb9f24230cc469d803c448094668054");
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    public class User
    {
        public List<string> Queries { get; set; } = new List<string>();
        public string ID = string.Empty;
    }
}