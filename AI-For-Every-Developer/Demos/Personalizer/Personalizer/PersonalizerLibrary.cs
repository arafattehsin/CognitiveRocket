using Azure;
using Azure.AI.Personalizer;

namespace Personalizer
{
    public class PersonalizerLibrary
    {
        // The key specific to your personalizer service instance; e.g. "0123456789abcdef0123456789ABCDEF"
        private const string ApiKey = "<your API Key>";

        // The endpoint specific to your personalizer service instance; e.g. https://westus2.api.cognitive.microsoft.com/
        private const string ServiceEndpoint = "https://<yourpersonalizerresourcename>.cognitiveservices.azure.com/"; 

        // Personalizer client
        private PersonalizerClient client;

        // Rankable actions (change them according to your needs)
        IList<PersonalizerRankableAction> actionList;

        public PersonalizerLibrary()
        {
            client = InitializePersonalizerClient(new Uri(ServiceEndpoint));
            actionList = GetRankableActions();
        }

        // <authorization>
        /// <summary>
        /// Initializes the personalizer client.
        /// </summary>
        /// <param name="url">Azure endpoint</param>
        /// <returns>Personalizer client instance</returns>
        private PersonalizerClient InitializePersonalizerClient(Uri url)
        {
            return new PersonalizerClient(url, new AzureKeyCredential(ApiKey));
        }
        // </authorization>

        private IList<PersonalizerRankableAction> GetRankableActions()
        {
            IList<PersonalizerRankableAction> actions = new List<PersonalizerRankableAction>
            {
               new PersonalizerRankableAction(
                    id: "chocolate_cake",
                    features:
                    new List<object>() { new { variety = "cake", sweetlevel = "medium" }, new { nutritionLevel = 3, cuisine = "austrian" } }
                ),

                new PersonalizerRankableAction(
                    id: "tresleches_cake",
                    features:
                    new List<object>() { new { variety = "cake", sweetlevel = "high" }, new { nutritionLevel = 3, cuisine = "mexican" } }
                ),
                 new PersonalizerRankableAction(
                    id: "nutella_cupcake",
                    features:
                    new List<object>() { new { variety = "cupcake", sweetlevel = "high" }, new { nutritionLevel = 6 } }
                ),
                 new PersonalizerRankableAction(
                    id: "vanilla_cupcake",
                    features:
                    new List<object>() { new { variety = "cupcake", sweetlevel = "high" }, new { nutritionLevel = 7 } }
                ),

                new PersonalizerRankableAction(
                    id: "fruit_trifle",
                    features:
                    new List<object>() { new { variety = "dessert", sweetlevel = "low" }, new { nutritionalLevel = 5 } }
                ),

                new PersonalizerRankableAction(
                    id: "banoffee_pie",
                    features:
                    new List<object>() { new { variety = "dessert", sweetlevel = "medium" }, new { nutritionalLevel = 5 } }
                ),

                new PersonalizerRankableAction(
                    id: "mango_delight",
                    features:
                    new List<object>() { new { variety = "shake", sweetlevel = "medium" }, new { nutritionLevel = 2 }, new { drink = true } }
                ),

                new PersonalizerRankableAction(
                    id: "the_original",
                    features:
                    new List<object>() { new { variety = "shake", sweetlevel = "high" }, new { nutritionLevel = 2 }, new { drink = true } }
                )
            };

            return actions;
        }

        public  PersonalizerResponse GetRankResults(IList<object> currentContextFeatures)
        {
            string eventId = Guid.NewGuid().ToString();
            var request = new PersonalizerRankOptions(actionList, currentContextFeatures, null, eventId);
            PersonalizerRankResult result = client.Rank(request);
            return new PersonalizerResponse() { EventId = eventId, RewardActionId = result.RewardActionId };
        }

        public void SubmitReward(string eventId, float reward)
        {
            client.Reward(eventId, reward);
        }

    }

    public class PersonalizerResponse
    {
        public string RewardActionId { get; set; }
        public string EventId { get; set; }
    }
}
