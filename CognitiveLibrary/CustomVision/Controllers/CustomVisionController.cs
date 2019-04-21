using CognitiveLibrary.Utilities;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using static CognitiveLibrary.Models.CustomVisionModel;

namespace CognitiveLibrary.Controllers
{
    public class CustomVisionController
    {
        public static bool IsValidCard(string url)
        {
            try
            {
                string trainingKey = Constants.CustomVisionTrainingAPIKey;

                // Create the Api, passing in a credentials object that contains the training key
                TrainingApiCredentials trainingCredentials = new TrainingApiCredentials(trainingKey);
                TrainingApi trainingApi = new TrainingApi(trainingCredentials);

                var projects = trainingApi.GetProjects();
                ProjectModel project = projects.FirstOrDefault(e => e.Name == "IDVision");

                // Get the prediction key, which is used in place of the training key when making predictions
                var account = trainingApi.GetAccountInfo();
                var predictionKey = account.Keys.PredictionKeys.PrimaryKey;

                // Create a prediction endpoint, passing in a prediction credentials object that contains the obtained prediction key
                PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
                PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

                byte[] byteData = Utilities.Utilities.GetImagesAsByteArrayFromUri(url);

                MemoryStream stream = new MemoryStream(byteData);
                // Make a prediction against the new project
                var predictionResult = endpoint.PredictImage(project.Id, stream);

                // Loop over each prediction and write out the results
                foreach (var c in predictionResult.Predictions)
                {
                    if ((c.Probability * 100) > 80)
                        return true;
                    else
                        return false;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return false;
        }
    }
}