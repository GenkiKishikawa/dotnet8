using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalApiSample.Models;

namespace MinimalApiSample.Controllers
{
  [Route("api/servicenow_common")]
  [ApiController]
  public class ServiceNowCommonController : ControllerBase
  {
    private readonly ServiceNowCommonSettingsModel _settings;

    private readonly IGraphAPIComponent _graphApi;

    private readonly IResultComponent _result;

    private readonly IHttpClientFactory _clientFactory;

    private readonly IConfiguration _config;

    private readonly AppLogger _appLogger;

    public ServiceNowCommonController(
      IOptions<ServiceNowCommonSettingsModel> settings,
      IGraphAPIComponent graphApi,
      IResultComponent result,
      IHttpClientFactory clientFactory,
      IConfiguration config,
      IAppLoggerFactory appLoggerFactory
    )
    {
      _settings = settings.Value;
      _graphApi = graphApi;
      _result = result;
      _clientFactory = clientFactory;
      _config = config;
      _appLogger = appLoggerFactory.CreateLogger(typeof(ServiceNowCommonController).FullName);
    }

    [HttpGet("get")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<ActionResult<ResultModel>> Get()
    {
      try


    }
  }
  }