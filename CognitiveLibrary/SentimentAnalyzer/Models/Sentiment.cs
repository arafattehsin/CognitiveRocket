
using Microsoft.ML.Data;

namespace SentimentAnalyzer.Models
{
    public class Sentiment
    {
        [LoadColumn(0)]
        public bool Label { get; set; }
        [LoadColumn(5)]
        public string Text { get; set; }
    }
}
