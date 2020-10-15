using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using SentimentAnalyzer.Models;
using System.Reflection;

namespace SentimentAnalyzer
{
    public class Sentiments
    {
        public static SentimentPrediction Predict(string text)
        {
            //Create MLContext to be shared across the model creation workflow objects 
            //Set a random seed for repeatable/deterministic results across multiple trainings.
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("SentimentAnalyzer.MLModels.SentimentModel.zip");

            var mlContext = new MLContext(seed: 1);
            Sentiment statement = new Sentiment { Col0 = text };
            ITransformer trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<Sentiment, SentimentPrediction>(trainedModel);

            stream.Close();

            //Score
            return predEngine.Predict(statement);
        }

    }
}
