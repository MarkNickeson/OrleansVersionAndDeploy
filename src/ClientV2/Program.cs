using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace ClientV2
{
    public class LocalCommandServer : CommandServerBase
    {
        IHost? _clientHost;
        IClusterClient? _clusterClient;

        public LocalCommandServer() : base(IpcServerNames.CLIENTV2)
        {
        }

        protected override async Task<ResponseMessageBase> HandleRequestMessageOverride(RequestMessageBase requestMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            switch (requestMessage)
            {
                case StartClientRequest r:
                    return await InvokeStartClient(r);
                case StopClientRequest r:
                    return await InvokeStopClient(r);
                case GetIdAndVersionRequestX r:
                    return await InvokeGetIdAndVersionRequest(r);
                default:
                    throw new ApplicationException($"Unexpected request message type: {requestMessage.GetType().FullName}");
            }
        }

        async Task<GetIdAndVersionResponse> InvokeGetIdAndVersionRequest(GetIdAndVersionRequestX req)
        {
            if (_clusterClient == null)
            {
                return new GetIdAndVersionResponse() { Error = "Client is not active" };
            }

            try
            {
                var foo = _clusterClient.GetGrain<IFoo>(req.GrainId);
                var rval = await foo.GetIdAndVersion(req.InboundPayload);
                return new GetIdAndVersionResponse()
                {
                    Success = true,
                    ReturnValue = rval
                };
            }
            catch (Exception ex)
            {
                return new GetIdAndVersionResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        async Task<ResponseMessageBase> InvokeStartClient(StartClientRequest req)
        {
            try
            {
                if (_clientHost != null)
                {
                    return new ResponseMessageBase()
                    {
                        Success = false,
                        Error = "Client Already Active"
                    };
                }

                _clientHost = await Host.CreateDefaultBuilder()
                  .UseOrleansClient(builder => builder
                    .UseStaticClustering(new IPEndPoint(IPAddress.Loopback, req.GatewayPort)))
                  .StartAsync();

                _clusterClient = _clientHost.Services.GetRequiredService<IClusterClient>();

                return new ResponseMessageBase()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessageBase()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        async Task<ResponseMessageBase> InvokeStopClient(StopClientRequest req)
        {
            if (_clientHost == null)
            {
                return new ResponseMessageBase()
                {
                    Success = false,
                    Error = "Client not Active"
                };
            }

            try
            {
                await _clientHost.StopAsync();
                _clientHost = null;
                _clusterClient = null;
                return new ResponseMessageBase()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseMessageBase()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting clientV2 server");
            var server = new LocalCommandServer();
            await server.StartAsync();
            await server.Completed;
            Console.WriteLine("Completed clientV2 server");
        }
    }
}