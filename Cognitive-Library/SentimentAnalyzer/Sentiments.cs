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
    /// <summary>
    /// Thread-safe singleton to manage initialization of ML training model
    /// to improve runtime performance.
    /// </summary>
    public sealed class Sentiments
    {
        private static readonly Sentiments instance = new Sentiments();
        private readonly PredictionEngine<Sentiment, SentimentPrediction> predictionEngine;

        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as beforefieldinit
        /// </summary>
        static Sentiments()
        {
        }

        private Sentiments()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SentimentAnalyzer.MLModels.SentimentModel.zip"))
            {
                var mlContext = new MLContext();
                ITransformer trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);

                // Create prediction engine related to the loaded trained model
                predictionEngine = mlContext.Model.CreatePredictionEngine<Sentiment, SentimentPrediction>(trainedModel);

                stream.Close();
            }
        }

        /// <summary>
        /// Perform a single prediction of the given text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static SentimentPrediction Predict(string text)
        {
            var statement = new Sentiment { Col0 = text };
            return instance.predictionEngine.Predict(statement);
        }

    }
}
