using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.LatestVersionBackwardsCompatible.ExistingActivationScenarios.HeterogenousCluster
{
    [Collection("TestProcesses")]
    public class ExistingActivation_HeterogenousCluster_SiloV1SiloV2ClientV1ClientV2_ClientV1BackwardsCompatible : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV1;
        ResponseMessageBase? _startClientV2;

        public ExistingActivation_HeterogenousCluster_SiloV1SiloV2ClientV1ClientV2_ClientV1BackwardsCompatible(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        {
            // start silov1
            _startSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV1_GatewayPort,
                SiloPort = ConfigConstants.SiloV1_SiloPort,
                PrimarySiloPort = null, // null means this is primary
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV2_GatewayPort,
                SiloPort = ConfigConstants.SiloV2_SiloPort,
                PrimarySiloPort = ConfigConstants.SiloV1_SiloPort,
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v1
            _startClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });

            // start client v2
            _startClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });
        }

        [Fact]
        public async Task Test()
        {
            Assert.True(_startSiloV1!.Success);
            Assert.True(_startClientV1!.Success);
            Assert.True(_startSiloV2!.Success);
            Assert.True(_startClientV2!.Success);

            // use client V2 to cause activation on Silo V2

            var getIdAndVersionResponse = await _testProcesses.ClientV2Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V2 client"
            });

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V2 client, Version: V2 Server", getIdAndVersionResponse.ReturnValue);

            // use client V1 to access grain via backwards compatibility
            var getIdAndVersionResponse2 = await _testProcesses.ClientV1Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1", // same grain key
                InboundPayload = "Invoked by V1 client"
            });

            Assert.True(getIdAndVersionResponse2.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V1 client, Version: V2 Server", getIdAndVersionResponse2.ReturnValue);
        }

        public async Task DisposeAsync()
        {
            // shutdown order matters because primary must be the last to go
            var stopClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            var stopClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            var stopSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
            var stopSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
        }
    }
}