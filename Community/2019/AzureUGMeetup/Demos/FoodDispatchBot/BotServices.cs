using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDispatchBot
{
    public interface IBotServices
    {
        LuisRecognizer FoodDispatch { get; }
        QnAMaker FoodQnA { get; }
    }


    public class BotServices : IBotServices
    {
        public BotServices(IConfiguration configuration)
        {
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult
            FoodDispatch = new LuisRecognizer(new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
                includeApiResults: true);

            FoodQnA = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                EndpointKey = configuration["QnAAuthKey"],
                Host = configuration["QnAEndpointHostName"]
            });
        }

        public LuisRecognizer FoodDispatch { get; private set; }
        public QnAMaker FoodQnA { get; private set; }
    }
}
