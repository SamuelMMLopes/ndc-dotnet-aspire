using Google.Protobuf;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Dometrain.Cart.Processor;

public class ChangeFeedProcessorService : BackgroundService
{
    private const string DatabaseId = "cartdb";
    private const string SourceContainerId = "carts";
    private const string LeaseContainerId = "carts-leases";

    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<ChangeFeedProcessorService> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public ChangeFeedProcessorService(CosmosClient cosmosClient, ILogger<ChangeFeedProcessorService> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = _cosmosClient.GetDatabase(DatabaseId);
        await database.CreateContainerIfNotExistsAsync(new ContainerProperties(LeaseContainerId, "/id"), 400, cancellationToken: stoppingToken);

        var leaseContainer = _cosmosClient.GetContainer(DatabaseId, LeaseContainerId);

        var changeFeedProcessor = _cosmosClient.GetContainer(DatabaseId, SourceContainerId)
            .GetChangeFeedProcessorBuilder<ShoppingCart>(processorName: "cache-processor", onChangesDelegate: HandleChangesAsync)
            .WithInstanceName($"cache-processor-{Guid.NewGuid().ToString()}")
            .WithLeaseContainer(leaseContainer)
            .Build();

        _logger.LogInformation("Starting Change Feed Processor");
        await changeFeedProcessor.StartAsync();
    }

    async Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<ShoppingCart> changes,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Started handling changes for lease {LeaseToken}", context.LeaseToken);
        _logger.LogDebug("Change Feed request consumed {RequestCharge} RU.", context.Headers.RequestCharge);
        _logger.LogDebug("SessionToken {SessionToken}", context.Headers.Session);

        if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
        {
            _logger.LogWarning("Change Feed request took longer than expected. Diagnostics: {@Diagnostics}", context.Diagnostics);
        }

        var db = _connectionMultiplexer.GetDatabase();
        foreach (var shoppingCart in changes)
        {
            var serializedShoppingCart = JsonConvert.SerializeObject(shoppingCart).Replace("\"id\"", "\"StudentId\"");
            _logger.LogInformation(serializedShoppingCart);
            await db.StringSetAsync($"cart_id_{shoppingCart.StudentId.ToString()}", serializedShoppingCart);
        }

        _logger.LogDebug("Finished handling changes.");
    }
}
