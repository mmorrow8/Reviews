using Azure;
using System;
using Azure.AI.TextAnalytics;

namespace ReviewsBeta
{
    public static class Sentiment
    {
        private static string SentimentKeyCredential = Environment.GetEnvironmentVariable("SentimentKeyCredential", EnvironmentVariableTarget.Process);
        private static string SentimentUri = Environment.GetEnvironmentVariable("SentimentUri", EnvironmentVariableTarget.Process);

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(SentimentKeyCredential);
        private static readonly Uri endpoint = new Uri(SentimentUri);

        public static decimal Analyze(string inputText)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var result = 0m;

            if (!string.IsNullOrWhiteSpace(inputText))
            {
                DocumentSentiment documentSentiment = client.AnalyzeSentiment(inputText);
                result = (decimal)(documentSentiment.Sentences.First().ConfidenceScores.Positive * 5);
            }

            return result;
        }
    }
}
