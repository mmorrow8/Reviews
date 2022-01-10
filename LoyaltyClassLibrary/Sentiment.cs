using Azure;
using System;
using Azure.AI.TextAnalytics;

namespace MarkovReviews
{
    public class Sentiment
    {
        private static TextAnalyticsClient client;

        public Sentiment()
        {
            var SentimentKeyCredential = Environment.GetEnvironmentVariable("SentimentKeyCredential");
            var SentimentUri = Environment.GetEnvironmentVariable("SentimentUri");

            AzureKeyCredential credentials = new AzureKeyCredential(SentimentKeyCredential);
            Uri endpoint = new Uri(SentimentUri);

            client = new TextAnalyticsClient(endpoint, credentials);
        }

        public decimal Analyze(string inputText)
        {
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
