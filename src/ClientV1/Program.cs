using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;
using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace SiloV1Client
{
    public class LocalCommandServer : CommandServerBase
    {
        IHost? _clientHost;
        IClusterClient? _clusterClient;

        public LocalCommandServer() : base(IpcServerNames.CLIENTV1)
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
                case AcquireLeaseRequest r:
                    return await InvokeAcquireLeaseRequest(r);
                case ReadLeaseIDRequest r:
                    return await InvokeReadLeaseIDRequest(r);
                case InvokeUnversionedTestRequest r:
                    return await InvokeUnversionedTestRequest(r);
                default:
                    throw new ApplicationException($"Unexpected request message type: {requestMessage.GetType().FullName}");
            }
        }

        async Task<InvokeUnversionedTestResponse> InvokeUnversionedTestRequest(InvokeUnversionedTestRequest req)
        {
            if (_clusterClient == null)
            {
                return new InvokeUnversionedTestResponse() { Error = "Client is not active" };
            }

            try
            {
                var grainRef = _clusterClient.GetGrain<IVersionlessGrainTest>(req.GrainId);
                var rval = await grainRef.GetLabel();
                return new InvokeUnversionedTestResponse()
                {
                    Success = true,
                    ReturnValue = rval
                };
            }
            catch (Exception ex)
            {
                return new InvokeUnversionedTestResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        async Task<ReadLeaseIDResponse> InvokeReadLeaseIDRequest(ReadLeaseIDRequest req)
        {
            if (_clusterClient == null)
            {
                return new ReadLeaseIDResponse() { Error = "Client is not active" };
            }

            try
            {
                var dummyLease = _clusterClient.GetGrain<IDummyLease>(req.GrainId);
                var rval = await dummyLease.ReadLeaseID();
                return new ReadLeaseIDResponse()
                {
                    Success = true,
                    LeaseID = rval
                };
            }
            catch (Exception ex)
            {
                return new ReadLeaseIDResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
            }
        }

        async Task<AcquireLeaseResponse> InvokeAcquireLeaseRequest(AcquireLeaseRequest req)
        {
            if (_clusterClient == null)
            {
                return new AcquireLeaseResponse() { Error = "Client is not active" };
            }

            try
            {
                var dummyLease = _clusterClient.GetGrain<IDummyLease>(req.GrainId);
                var rval = await dummyLease.DummyAcquire();
                return new AcquireLeaseResponse()
                {
                    Success = true,
                    LeaseID = rval
                };
            }
            catch (Exception ex)
            {
                return new AcquireLeaseResponse()
                {
                    Success = false,
                    Error = ex.ToString()
                };
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
                        .UseLocalhostClustering(req.GatewayPorts))
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
            Console.WriteLine("Starting clientV1 server");
            var server = new LocalCommandServer();
            await server.StartAsync();
            await server.Completed;
            Console.WriteLine("Completed clientV1 server");
        }
    }
}