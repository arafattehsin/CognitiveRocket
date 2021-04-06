# SentimentAnalyzer - On device (offline) Sentiment Analysis for .NET Standard apps
 
 This is the library which can be used to consume the model which I created using ML.NET. For further information, you can always have a look at my [blog](https://www.arafattehsin.com/blog/sentimentanalyzer-ondevice-machine-learning/)
 
```c#
using SentimentAnalyzer;

var sentiment = Sentiments.Predict("some string");
```