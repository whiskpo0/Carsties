using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStarted.Register(async () => { 
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);    
    }
});

 
app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_=> TimeSpan.FromSeconds(3)); 
