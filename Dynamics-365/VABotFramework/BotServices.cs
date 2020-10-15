using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VABotFramework
{
    public interface IBotServices
    {
        LuisRecognizer VABotDispatch { get; }
        VABotService VABotService { get; }
    }


    public class BotServices : IBotServices
    {
        public BotServices(IConfiguration configuration)
        {
            // Read the setting for cognitive services (LUIS, QnA) from the appsettings.json
            // If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
            // will be made available in the properties collection of the RecognizerResult
            VABotDispatch = new LuisRecognizer(new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
                new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
                includeApiResults: true);

            VABotService = new VABotService(new VABotEndpoint(
                configuration["VABotId"],
                configuration["VABotTenantId"],
                configuration["VABotTokenEndpoint"]),
                configuration["VABotName"]
            );
        }

        public LuisRecognizer VABotDispatch { get; private set; }
        public VABotService VABotService { get; private set; }
    }
}
