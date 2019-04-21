using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using SentimentAnalyzer.Models;

namespace SentimentAnalyzer
{
    public class Sentiments
    {
        private static readonly string BaseModelsRelativePath = @"../../../Result";
        private static readonly string ModelRelativePath = $"{BaseModelsRelativePath}/SentimentModel.zip";
        private static string ModelPath = GetAbsolutePath(ModelRelativePath);

        public static SentimentPrediction Predict(string text)
        {
            //Create MLContext to be shared across the model creation workflow objects 
            //Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);
            Sentiment sampleStatement = new Sentiment { Text = text };
            ITransformer trainedModel = mlContext.Model.Load(ModelPath, out var modelInputSchema);

            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<Sentiment, SentimentPrediction>(trainedModel);

            //Score
            return predEngine.Predict(sampleStatement);
        }

        static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Sentiments).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}
