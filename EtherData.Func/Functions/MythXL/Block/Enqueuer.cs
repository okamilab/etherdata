using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;
using EtherData.Func.Utils;
using EtherData.Func.Models;

namespace EtherData.Func.MythXL.Block
{
    public static class Enqueuer
    {
        [FunctionName("MythXL_Block_Enqueuer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mythxl/v1.0/blocks/enqueue")] HttpRequest req,
            [Queue("%Storage:BlockQueue%", Connection = "Storage:Connection")] CloudQueue blockQueue,
            ILogger log,
            ExecutionContext context)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var lastBlockKey = config.GetValue<string>("MythXL:BlockEnqueuer:LastBlock");
            var stateManager = new StateManager(config);
            var state = await stateManager.GetValueAsync(lastBlockKey);
            var lastBlock = int.Parse(state.Value);

            // TODO check head block
            var batchSize = 100;
            for (var i = 0; i < batchSize; i++)
            {
                string msg = JsonConvert.SerializeObject(new BlockMessage { Block = lastBlock + i });
                await blockQueue.AddMessageAsync(new CloudQueueMessage(msg));
            }

            lastBlock = lastBlock + batchSize;
            await stateManager.SetValueAsync(lastBlockKey, lastBlock.ToString());

            return new OkObjectResult(lastBlock);
        }
    }
}
