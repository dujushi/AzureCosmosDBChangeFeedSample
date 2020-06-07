using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ChangeFeedSample.Startup))]
namespace ChangeFeedSample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var container = new BlobContainerClient(configuration["PaymentBlobStorageConnectionString"], configuration["PaymentBlobStorageContainerName"]);
                container.CreateIfNotExists();
                return container;
            });
        }
    }
}
