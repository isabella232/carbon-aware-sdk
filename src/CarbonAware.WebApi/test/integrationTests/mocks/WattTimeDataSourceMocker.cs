using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;
using System.Net;
using System.Text.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System.Net.Mime;
using CarbonAware.Tools.WattTimeClient.Model;
using CarbonAware.Tools.WattTimeClient.Constants;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private readonly object _dataSource = DataSourceType.WattTime;
    private static readonly DateTimeOffset testDataPointOffset = new(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly string testBA = "TEST_BA";
    private static readonly GridEmissionDataPoint defaultDataPoint = new()
    {
        BalancingAuthorityAbbreviation = testBA,
        Datatype = "dt",
        Frequency = 300,
        Market = "mkt",
        PointTime = testDataPointOffset,
        Value = 999.99F,
        Version = "1.0"
    };

    private static readonly List<GridEmissionDataPoint> defaultDataList = new() { defaultDataPoint };

    private static readonly List<Forecast> defaultForecastList = new()
    {
        new Forecast()
        {
            GeneratedAt = testDataPointOffset,
            ForecastData = new List<GridEmissionDataPoint>()
                    {
                        new GridEmissionDataPoint()
                        {
                            BalancingAuthorityAbbreviation = testBA,
                            PointTime = testDataPointOffset,
                            Value = 999.99F,
                            Version = "1.0"
                        }
                    }
        }
    };

    private static readonly BalancingAuthority defaultBalancingAuthority = new()
    {
        Id = 12345,
        Abbreviation = testBA,
        Name = "Test Balancing Authority"
    };

    private static readonly LoginResult defaultLoginResult = new() { Token = "myDefaultToken123" };

    internal WattTimeDataSourceMocker()
    {
        _server = WireMockServer.Start();
        Initialize();
    }

    public void SetupDataMock(DateTimeOffset start, DateTimeOffset end, string location)
    {
        GridEmissionDataPoint newDataPoint = new()
        {
            BalancingAuthorityAbbreviation = location,
            Datatype = defaultDataPoint.Datatype,
            Frequency = defaultDataPoint.Frequency,
            Market = defaultDataPoint.Market,
            PointTime = start,
            Value = defaultDataPoint.Value,
            Version = defaultDataPoint.Version
        };

        List<GridEmissionDataPoint> newDataList = new() { newDataPoint };

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(newDataList));
    }

    public void SetupForecastMock(List<Forecast>? content = null)
    {
        SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(content ?? defaultForecastList));
    }

    public WebApplicationFactory<Program> OverrideWebAppFactory(WebApplicationFactory<Program> factory)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Configure<WattTimeClientConfiguration>(configOpt =>
                {
                    configOpt.BaseUrl = _server.Url!;
                });
            });
        });
    }

    public void Initialize()
    {
        SetupBaMock();
        SetupLoginMock();
    }

    public void Reset()
    {
        _server.Reset();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    private void SetupResponseGivenGetRequest(string path, string body, HttpStatusCode statusCode = HttpStatusCode.OK, string contentType = MediaTypeNames.Application.Json)
    {
        _server
            .Given(Request.Create().WithPath("/" + path).UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", contentType)
                    .WithBody(body)
        );
    }

    private void SetupBaMock(BalancingAuthority? content = null) =>
    SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, JsonSerializer.Serialize(content ?? defaultBalancingAuthority));

    private void SetupLoginMock(LoginResult? content = null) =>
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize(content ?? defaultLoginResult));

}