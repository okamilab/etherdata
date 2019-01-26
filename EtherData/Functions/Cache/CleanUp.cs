using System.Net;
using System.Net.Http;
using EtherData.Cache;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtherData.Functions.Cache
{
    public static class CleanUp
    {
        [FunctionName("CleanUp")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = "v0.1/cache/cleanup")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cache = new RedisCacheManager(config);
            cache.CleanUp();

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
