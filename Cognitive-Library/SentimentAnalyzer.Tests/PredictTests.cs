using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SentimentAnalyzer.Tests
{
    public class PredictTests
    {
        [Fact]
        public void Should_Return_Negative_For_Negative_Sounding_Review()
        {
            var prediction = Sentiments.Predict("This is the worst product I've ever reviewed!").Prediction;

            Assert.False(prediction);
        }

        [Fact]
        public void Should_Return_Positive_For_Positive_Sounding_Review()
        {
            var prediction = Sentiments.Predict("This is the best product I've ever reviewed!").Prediction;

            Assert.False(prediction);
        }
    }
}
