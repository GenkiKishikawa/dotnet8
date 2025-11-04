using MinimalApiSample.Log;
using MinimalApiSample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiCommonSettingsModel>(builder.Configuration.GetSection("ApiCommonSettings"));

builder.Services.AddScoped<IAppLoggerFactory, AppLoggerFactory>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
