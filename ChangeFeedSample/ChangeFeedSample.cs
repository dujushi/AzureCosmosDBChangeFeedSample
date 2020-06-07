using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using ChangeFeedSample.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ChangeFeedSample
{
    public class ChangeFeedSample
    {
        private readonly BlobContainerClient _blobContainerClient;

        public ChangeFeedSample(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient ?? throw new ArgumentNullException(nameof(blobContainerClient));
        }

        [FunctionName(nameof(ChangeFeedSample))]
        public async Task Run(
            [CosmosDBTrigger(
                "ChangeFeedSample",
                "Payment",
                ConnectionStringSetting = "CosmosDBConnectionString",
                LeaseCollectionName = "leases",
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> documents,
            ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.LogInformation("Documents modified: " + documents.Count);
                foreach (var document in documents)
                {
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var payment = JsonSerializer.Deserialize<Payment>(document.ToString(), jsonSerializerOptions);
                    var paymentString = JsonSerializer.Serialize(payment, jsonSerializerOptions);
                    var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(paymentString));
                    await _blobContainerClient.UploadBlobAsync(document.Id, memoryStream);
                }
            }
        }
    }
}