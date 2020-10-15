using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Models;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using static CarEvaluator.Car;

namespace CarEvaluator
{
    internal static class Program
    {
        private static string AppPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string TrainDataPath => Path.Combine(AppPath, "datasets", "car-data.csv");
        private static string TestDataPath => Path.Combine(AppPath, "datasets", "car-data-eval.csv");
        private static string ModelPath => Path.Combine(AppPath, "CarEvaluator.zip");

        private static async Task Main(string[] args)
        {
            // STEP1: Create a model
            var model = await TrainAsync();

            // STEP2: Test accuracy of the model
            Evaluate(model);

           // STEP 3: Make a prediction
           var prediction = model.Predict(CarEvalTests.Eval);
            Console.WriteLine($"Predicted result: {prediction.Result:0.####}, actual result: unacceptable");

            Console.ReadLine();
        }

        private static async Task<PredictionModel<CarEval, CarEvalPredictor>> TrainAsync()
        {
          
            var pipeline = new LearningPipeline();

            pipeline.Add(new TextLoader(TrainDataPath).CreateFrom<CarEval>(separator: ','));
            pipeline.Add(new Dictionarizer(("Result", "Label")));

            //pipeline.Add(new TextFeaturizer("Buying", "Buying"));
            //pipeline.Add(new TextFeaturizer("Maintenance", "Maintenance"));
            //pipeline.Add(new TextFeaturizer("Doors", "Doors"));
            //pipeline.Add(new TextFeaturizer("Persons", "Persons"));
            //pipeline.Add(new TextFeaturizer("LugBoot", "LugBoot"));
            //pipeline.Add(new TextFeaturizer("Safety", "Safety"));

            pipeline.Add(new CategoricalOneHotVectorizer("Buying",
                                             "Maintenance",
                                             "Doors",
                                             "Persons",
                                             "LugBoot",
                                             "Safety"));

            pipeline.Add(new ColumnConcatenator("Features",
                    "Buying",
                    "Maintenance",
                    "Doors",
                    "Persons",
                    "LugBoot",
                    "Safety"));

            pipeline.Add(new StochasticDualCoordinateAscentClassifier());
            pipeline.Add(new PredictedLabelColumnOriginalValueConverter() { PredictedLabelColumn = "PredictedLabel" });


            Console.WriteLine("=============== Training model ===============");
            // The pipeline is trained on the dataset that has been loaded and transformed.
            var model = pipeline.Train<CarEval, CarEvalPredictor>();

            // Saving the model as a .zip file.
            await model.WriteAsync(ModelPath);

            Console.WriteLine("=============== End training ===============");
            Console.WriteLine("The model is saved to {0}", ModelPath);

            return model;
        }

        private static void Evaluate(PredictionModel<CarEval, CarEvalPredictor> model)
        {
            // To evaluate how good the model predicts values, it is run against new set
            // of data (test data) that was not involved in training.
            var testData = new TextLoader(TestDataPath).CreateFrom<CarEval>(separator: ',');

            // RegressionEvaluator calculates the differences (in various metrics) between predicted and actual
            // values in the test dataset.
            var evaluator = new ClassificationEvaluator();

            Console.WriteLine("=============== Evaluating model ===============");

            var metrics = evaluator.Evaluate(model, testData);

            Console.WriteLine($"Micro-Accuray is {metrics.AccuracyMicro}");
        }

        
    }
}