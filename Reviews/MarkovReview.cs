using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReviewsBeta;
using LoyaltyClassLibrary;

namespace Reviews
{
    public static class MarkovReview
    {
        static MarkovChain chain = new MarkovChain();

        [FunctionName("generate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //When the function is published to Azure, large JSON files cause out of memory errors
            //Locally, large JSON files are no problem
            //I believe there is a 1.5GB memory limit on Azure functions
            //Ideally, I would use EF and store the Markov chain in a table to correct this
            //Another solution might be to use an Azure VM, but serverless functions are superior, in my opinion

            LoadTrainingBlob("Musical_Instruments_5.json");

            //Too large for this function when published to Azure
            //LoadTrainingBlob("Video_Games_5.json");

            //Using local JSON files is the fastest method
            //LoadLocalJSON(@"C:\Users\Mark\Desktop\Loyalty\Data\Video_Games_5.json");

            var response = new Review();
            response.reviewText = chain.Generate(3, 100);
            response.reviewRating = Sentiment.Analyze(response.reviewText);

            return new JsonResult(response);
        }

        public static void LoadTrainingBlob(string blobName)
        {
            if (chain.GetChain().Count == 0)
            {
                var trainingJSON = ReviewBlob.GetBlob(blobName);
                chain.LoadTrainingJSON(trainingJSON);
            }
        }

        public static void LoadLocalJSON(string filename)
        {
            if (chain.GetChain().Count == 0)
            {
                chain.LoadTrainingJSON(File.ReadLines(filename));
            }
        }
    }
}
