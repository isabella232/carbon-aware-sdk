using CarbonAware.DataSources.Configuration;
using CarbonAware.Tools.WattTimeClient;
using CarbonAware.Tools.WattTimeClient.Configuration;
using CarbonAware.Tools.WattTimeClient.Constants;
using CarbonAware.Tools.WattTimeClient.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CarbonAware.WebApi.IntegrationTests;
public class WattTimeDataSourceMocker : IDataSourceMocker
{
    protected WireMockServer _server;
    private static readonly object _dataSource = DataSourceType.WattTime;
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

    private static readonly Forecast defaultForecastList =
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
                    },

                    new GridEmissionDataPoint()
                    {
                        BalancingAuthorityAbbreviation = testBA,
                        PointTime = testDataPointOffset + TimeSpan.FromMinutes(5.0),
                        Value = 999.99F,
                        Version = "1.0"
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
        GridEmissionDataPoint newDataPoint = new GridEmissionDataPoint()
        {
            BalancingAuthorityAbbreviation = location,
            Datatype = "dt",
            Frequency = 300,
            Market = "mkt",
            PointTime = start,
            Value = 999.99F,
            Version = "1.0"
        };

        SetupResponseGivenGetRequest(Paths.Data, JsonSerializer.Serialize(newDataPoint));
    }

    /// <summary>
    /// Setup forecast calls on mock server
    /// </summary>
    /// <param name="server">Wire mock server to setup for forecast path. </param>
    /// <param name="content"> [Optional] List of forecasts to return in the mock. </param>
    /// <remarks> If no content is passed, server mocks a static forecast list with a single forecast. </remarks>
    public void SetupForecastMock() { 
        SetupResponseGivenGetRequest(Paths.Forecast, JsonSerializer.Serialize(defaultForecastList));
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

    /// <summary>
    /// Helper function for setting up server response given a get request.
    /// </summary>
    /// <param name="path">String path server should respond to.</param>
    /// <param name="statusCode">Status code server should respond with.</param>
    /// <param name="contentType">Content type server should return.</param>
    /// <param name="body">Response body from the request.</param>
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


    public void Initialize()
    {
        SetupResponseGivenGetRequest(Paths.BalancingAuthorityFromLocation, JsonSerializer.Serialize(defaultBalancingAuthority));
        SetupResponseGivenGetRequest(Paths.Login, JsonSerializer.Serialize( defaultLoginResult));
    }

    public void Reset()
    {
        _server.Reset();
    }

    public void Dispose()
    {
        _server.Dispose();
    }

    // server

}