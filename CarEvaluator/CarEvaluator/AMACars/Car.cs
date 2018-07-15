using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AMACars
{
    public class Car
    {
        public class CarEval
        {
            [Column(ordinal: "0")]
            public string Buying;

            [Column(ordinal: "1")]
            public string Maintenance;

            [Column(ordinal: "2")]
            public string Doors;

            [Column(ordinal: "3")]
            public string Persons;

            [Column(ordinal: "4")]
            public string LugBoot;

            [Column(ordinal: "5")]
            public string Safety;

            [Column(ordinal: "6")]
            public string Result;
        }

        public class CarEvalPredictor
        {
            [ColumnName("PredictedLabel")]
            public string Result;

            public static string GetPredictedResult(string buying, string maintenance, string doors, string persons, string lugboot, string safety)
            {
                buying = ValueMapper(buying);
                maintenance = ValueMapper(maintenance);
                doors = ValueMapper(doors);
                persons = ValueMapper(persons);
                lugboot = ValueMapper(lugboot);
                safety = ValueMapper(safety);

                CarEval carEval = new CarEval() { Buying = buying, Maintenance = maintenance, Doors = doors, Persons = persons, LugBoot = lugboot, Safety = safety };

                // Load model 
                string appPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                string modelPath = Path.Combine(appPath, "CarEvaluator.zip");

                // Read model
                var model = PredictionModel.ReadAsync<CarEval, CarEvalPredictor>(modelPath).Result;

                // Predict car value
                var prediction = model.Predict(carEval);

                // Return to the caller
                return ValueMapper(prediction.Result);
            }

            static string ValueMapper(string value)
            {
                switch (value)
                {
                    case "Very High":
                        return "v-high";
                    case "High":
                        return "high";
                    case "Medium":
                        return "med";
                    case "Low":
                        return "low";
                    case "Small":
                        return "small";
                    case "Big":
                        return "big";
                    case "5 or more":
                        return "5-more";
                    case "More":
                        return "more";
                    case "acc":
                        return "Acceptable";
                    case "unacc":
                        return "Unacceptable";
                    case "good":
                        return "Good";
                    case "v-good":
                        return "Very Good";
                    case "vgood":
                        return "Very Good";
                    default:
                        return value;
                }
                    
            }
        }
    }
  
}
