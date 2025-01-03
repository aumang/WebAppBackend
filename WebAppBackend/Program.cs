using GoldPriceTracker.Services;
using Microsoft.AspNetCore.SignalR;
using Nest;
using ProductPriceApp.Hubs;
using ProductPriceApp.Services;
using ProductPriceTracker.Hubs;
using ProductPriceTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddSingleton<ElasticsearchService>();
builder.Services.AddSingleton<ElasticSearchProductService>();
builder.Services.AddSingleton<CircuitBreakerService>();
//builder.Services.AddSingleton<CustomCircuitBreaker>(provider =>
//{
//    var logger = provider.GetRequiredService<ILogger<CustomCircuitBreaker>>();
//    var hubContext = provider.GetRequiredService<IHubContext<CircuitBreakerHub>>();

//    // Hardcoded values for threshold and timeout
//    int threshold = 5;
//    TimeSpan timeout = TimeSpan.FromSeconds(30);

//    return new CustomCircuitBreaker(threshold, timeout, logger, hubContext);
//});
builder.Services.AddSingleton(sp =>
    new CustomCircuitBreaker(
        threshold: 2,
        timeout: TimeSpan.FromSeconds(3),
        logger: sp.GetRequiredService<ILogger<CustomCircuitBreaker>>(),
        hubContext: sp.GetRequiredService<IHubContext<CircuitBreakerHub>>()
    )
);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GoldPriceService>();
builder.Services.AddSingleton(sp =>
{
    var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
        .DefaultIndex("products"); // Specify the default index
    return new ElasticClient(settings);
});
builder.Services.AddSingleton<GoldPricesService>();
builder.Services.AddSignalR();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("reactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ProductHub>("/hubs/product");
app.MapHub<ProductPriceHub>("/hubs/productprice");
app.MapHub<CircuitBreakerHub>("/hubs/circuitbreakerhub");
app.MapHub<GoldPriceHub>("/hubs/goldpriceshub");
app.MapControllers();
app.UseCors("reactApp");


app.Run();
