using MinimalApiSample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// DIコンテナServiceNowCommonSettingsModelを登録
builder.Services.Configure<ServiceNowCommonSettingsModel>(builder.Configuration.GetSection("ServiceNowCommonSettings"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
