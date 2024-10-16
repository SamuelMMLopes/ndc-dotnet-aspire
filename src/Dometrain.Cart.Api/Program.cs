
using Dometrain.Api.Shared;
using Dometrain.Cart.Api.ShoppingCarts;
using Dometrain.Monolith.Api.Sdk;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddMetrics();

builder.Services.AddApiDefaults(config);
//MainApi__BaseUrl
builder.Services.AddDometrainApi(config["MainApi:BaseUrl"]!, config["Identity:AdminApiKey"]!);

builder.AddAzureCosmosClient("cosmosdb");
builder.AddRedisClient("redis");

builder.Services.AddSingleton<IShoppingCartService, ShoppingCartService>();
builder.Services.AddSingleton<ShoppingCartRepository>();
builder.Services.AddSingleton<IShoppingCartRepository>(x =>
    new CachedShoppingCartRepository(x.GetRequiredService<ShoppingCartRepository>(), x.GetRequiredService<IConnectionMultiplexer>()));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapApiDefaults();

app.MapShoppingCartEndpoints();

app.Run();
