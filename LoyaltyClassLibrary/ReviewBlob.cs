using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyClassLibrary
{
    public class ReviewBlob
    {
        private static string BlobConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString", EnvironmentVariableTarget.Process);

        public static string GetBlob(string filename)
        {
            var svcClient = new BlobServiceClient(BlobConnectionString);
            var container = svcClient.GetBlobContainerClient("reviewblob");

            var blobClient = container.GetBlockBlobClient(filename);

            var blob = blobClient.DownloadContent();
                        
            return blob.Value.Content.ToString();
        }
    }
}
