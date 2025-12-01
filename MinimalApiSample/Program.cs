using MinimalApiSample.Log;
using MinimalApiSample.Models;
using MinimalApiSample.Components;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiCommonSettingsModel>(builder.Configuration.GetSection("ApiCommonSettings"));
builder.Services.Configure<UnitedApiSettingsModel>(builder.Configuration.GetSection("UnitedApiSettings"));

builder.Services.AddScoped<IAppLoggerFactory, AppLoggerFactory>();
builder.Services.AddScoped<IParamValidatorComponent, ParamValidatorComponent>();
builder.Services.AddScoped<IGraphAPIComponent, GraphAPIComponent>();
builder.Services.AddScoped<IResultComponent, ResultComponent>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

namespace MinimalApiSample
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
        .UConfigureAppConfiguration((hostingContext, config) =>
        {
          config.SetBasePath(Directory.GetCurrentDirectory());
        })
        .UseStartup<Startup>();
  }
}
