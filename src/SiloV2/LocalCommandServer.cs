using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Versions.Compatibility;
using Orleans.Versions.Selector;
using System.Net;

namespace SiloV2
{
    public class LocalCommandServer : CommandServerBase
    {
        IHost? siloHost;

        public LocalCommandServer() : base(IpcServerNames.SILOV2)
        {
        }

        protected override async Task<ResponseMessageBase> HandleRequestMessageOverride(RequestMessageBase requestMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            switch (requestMessage)
            {
                case StartSiloRequest r:
                    return await InvokeStartSilo(r);
                case StopSiloRequest r:
                    return await InvokeStopSilo(r);
                default:
                    throw new ApplicationException($"Unexpected request message type: {requestMessage.GetType().FullName}");
            }
        }

        async Task<ResponseMessageBase> InvokeStartSilo(StartSiloRequest req)
        {
            if (siloHost != null)
            {
                return new ResponseMessageBase()
                {
                    Success = false,
                    Error = "Silo Already Active"
                };
            }

            siloHost = await Host.CreateDefaultBuilder()
              .UseOrleans(builder => builder
                .UseLocalhostClustering(req.SiloPort, req.GatewayPort, req.PrimarySiloPort == null ? null : new IPEndPoint(IPAddress.Loopback, req.PrimarySiloPort.Value))
                .Configure<GrainVersioningOptions>(o => {
                    switch(req.VersionSelector)
                    {
                        case VersionSelector.AllCompatibleVersions:
                            o.DefaultVersionSelectorStrategy = nameof(AllCompatibleVersions);
                            break;
                        case VersionSelector.LatestVersion:
                            o.DefaultVersionSelectorStrategy = nameof(LatestVersion);
                            break;
                        case VersionSelector.MinimumVersion:
                            o.DefaultVersionSelectorStrategy = nameof(MinimumVersion);
                            break;
                    }

                    switch (req.VersionCompatibility)
                    {
                        case VersionCompatibilitiy.AllVersionsCompatible:
                            o.DefaultCompatibilityStrategy = nameof(AllVersionsCompatible);
                            break;
                        case VersionCompatibilitiy.BackwardCompatible:
                            o.DefaultCompatibilityStrategy = nameof(BackwardCompatible);
                            break;
                        case VersionCompatibilitiy.StrictVersionCompatible:
                            o.DefaultCompatibilityStrategy = nameof(StrictVersionCompatible);
                            break;
                    }
                })
              ).StartAsync();

            return new ResponseMessageBase()
            {
                Success = true
            };
        }

        async Task<ResponseMessageBase> InvokeStopSilo(StopSiloRequest req)
        {
            if (siloHost == null)
            {
                return new ResponseMessageBase()
                {
                    Success = false,
                    Error = "Silo not Active"
                };
            }

            try
            {
                await siloHost.StopAsync();
                siloHost = null;
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
}