# SentimentAnalyzer - On-device (offline) Sentiment Analysis for .NET Standard apps
 
 This is a library which can be used to consume a model which I created using ML.NET. 
 
 For further information, you can always have a look at my [blog](https://www.arafattehsin.com/blog/sentimentanalyzer-ondevice-machine-learning/)
 
## Quick Start

```c#
using SentimentAnalyzer;

var sentiment = Sentiments.Predict("some string");
```

`Predict` returns a `SentimentPrediction` which contains:

- `Prediction` (`bool`) - `true` is Positive sentiment, `false` is Negative sentiment.
- `Score` (`float`) - A score representing the model's accuracy