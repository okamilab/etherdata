using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using EtherData.Data.Cache;
using EtherData.Data.Queries;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtherData.Functions.Contracts
{
    public static class GetDeploymentStat
    {
        [FunctionName("Contracts_GetDeploymentStat")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v0.1/contracts/deployment")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var cache = new RedisCacheManager(config["REDIS:CONNECTION_STRING"], int.Parse(config["REDIS:LIVE_TIME"]));
            var result = cache.Get<IEnumerable<DeploymentStat>>(CacheKey.CONTRACT_DEPLOYMENT_STAT);
            return req.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
