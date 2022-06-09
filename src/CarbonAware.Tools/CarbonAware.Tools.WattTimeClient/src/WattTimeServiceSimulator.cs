using System.Net;
using System.Text;
using CarbonAware.Tools.WattTimeClient.Constants;
using CarbonAware.Tools.WattTimeClient.Model;
using System.Text.Json;

namespace CarbonAware.Tools.WattTimeClient
{
    /// <summary>
    /// A WattTime Service Simulator to use for testing
    /// </summary>
    /// <remarks> In order to use in tests, override the baseUrl in the WattTime client as http://localhost:{port}</remarks>
    public static class WattTimeServiceSimulator
    {
        /// <summary>
        /// The port the HttpListener should listen on
        /// </summary>
        private const int Port = 8888;

        /// <summary>
        /// This is the heart of the web server
        /// </summary>
        private static readonly HttpListener Listener = new() { Prefixes = { $"http://localhost:{Port}/" } };

        /// <summary>
        /// A flag to specify when we need to stop
        /// </summary>
        private static volatile bool _keepGoing = true;

        /// <summary>
        /// Keep the task in a static variable to keep it alive
        /// </summary>
        private static Task _mainLoop;

        /// <summary>
        /// Call this to start the listener server
        /// </summary>
        public static void StartListener()
        {
            if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
            _mainLoop = MainLoop();
        }

        /// <summary>
        /// Call this to stop the listener server. It will not kill any requests currently being processed.
        /// </summary>
        public static void StopListener()
        {
            _keepGoing = false;
            lock (Listener)
            {
                //Use a lock so we don't kill a request that's currently being processed
                Listener.Stop();
            }
            try
            {
                _mainLoop.Wait();
            }
            catch {}
        }

        /// <summary>
        /// The main loop to handle requests into the HttpListener
        /// </summary>
        /// <returns></returns>
        private static async Task MainLoop()
        {
            Listener.Start();
            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await Listener.GetContextAsync();
                    lock (Listener)
                    {
                        if (_keepGoing) ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpListenerException) return; //this gets thrown when the listener is stopped
                }
            }
        }

        /// <summary>
        /// Handle an incoming request
        /// </summary>
        /// <param name="context">The context of the incoming request</param>
        private static void ProcessRequest(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                try
                {
                    var handled = false;
                    switch (context.Request.Url?.AbsolutePath)
                    {
                        
                        case "/" + Paths.Data:
                            {
                                response.ContentType = "application/json";
                                var listResult = new List<GridEmissionDataPoint>()
                                {
                                    new GridEmissionDataPoint(),
                                    new GridEmissionDataPoint(),
                                    new GridEmissionDataPoint(),
                                    new GridEmissionDataPoint()
                                };
                                var responseBody = JsonSerializer.Serialize(listResult);

                                //Write it to the response stream
                                var buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
                            }
                            break;
                        case "/" + Paths.Forecast:
                            {
                                response.ContentType = "application/json";
                                var listResult = new List<Forecast>()
                                {
                                    new Forecast(),
                                    new Forecast(),
                                    new Forecast(),
                                    new Forecast()
                                };
                                var responseBody = JsonSerializer.Serialize(listResult);

                                //Write it to the response stream
                                var buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
                            }
                            break;
                        case "/" + Paths.BalancingAuthorityFromLocation:
                            {
                                response.ContentType = "application/json";
                                var responseBody = JsonSerializer.Serialize(new BalancingAuthority { Abbreviation = "CAISO_NORTH" });

                                //Write it to the response stream
                                var buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
                            }
                            break;
                        case "/" + Paths.Login:
                            {
                                response.ContentType = "application/json";
                                var responseBody = JsonSerializer.Serialize(new LoginResult { Token = "validToken" });

                                //Write it to the response stream
                                var buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
                            }
                            break;
                        case "/" + Paths.Historical:
                            {
                                response.ContentType = "application/zip";
                                var responseBody = "myStreamResults";

                                //Write it to the response stream
                                var buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
                            }
                            break;
                    }
                    if (!handled)
                    {
                        response.StatusCode = 404;
                    }
                }
                catch (Exception e)
                {
                    //Return the exception details the client
                    response.StatusCode = 500;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(e));
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}