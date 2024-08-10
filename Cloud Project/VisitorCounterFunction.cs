using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

public static class VisitorCounterFunction
{
    private static readonly string storageConnectionString = "AccountEndpoint=https://yadavcosmosdb.table.cosmos.azure.com:443/";
    private static readonly string tableName = "VisitorCount";
    private static readonly string partitionKey = "id"; // Assuming 'id' is your partition key

    private static readonly CloudTableClient tableClient;
    private static readonly CloudTable table;

    static VisitorCounterFunction()
    {
        var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
        table = tableClient.GetTableReference(tableName);
    }

    [FunctionName("UpdateViewCount")]
    public static async Task<IActionResult> UpdateViewCount(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "updateViewCount")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Updating view count.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var input = JsonConvert.DeserializeObject<dynamic>(requestBody);

        // Retrieve item from CosmosDB Table
        var retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, "0");
        var result = await table.ExecuteAsync(retrieveOperation);
        var entity = result.Result as DynamicTableEntity;

        if (entity == null)
        {
            return new NotFoundResult();
        }

        var views = int.Parse(entity.Properties["views"].Int32Value.ToString());
        views++;

        // Update item in CosmosDB Table
        entity.Properties["views"] = new EntityProperty(views);
        var updateOperation = TableOperation.Replace(entity);
        await table.ExecuteAsync(updateOperation);

        return new OkObjectResult(views);
    }
}
