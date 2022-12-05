using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.MinimumVersionStrictCompatible.NewActivationScenarios.HeterogenousCluster
{
    [Collection("TestProcesses")]
    public class NewActivation_HeterogenousCluster_SiloV1SiloV2ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV2;

        public NewActivation_HeterogenousCluster_SiloV1SiloV2ClientV2(TestProcessesFixture testProcesses)
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
                VersionCompatibility = VersionCompatibilitiy.StrictVersionCompatible,
                VersionSelector = VersionSelector.MinimumVersion
            });

            // start silov2
            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV2_GatewayPort,
                SiloPort = ConfigConstants.SiloV2_SiloPort,
                PrimarySiloPort = ConfigConstants.SiloV1_SiloPort, // points to primary
                VersionCompatibility = VersionCompatibilitiy.StrictVersionCompatible,
                VersionSelector = VersionSelector.MinimumVersion
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
            Assert.True(_startSiloV2!.Success);
            Assert.True(_startClientV2!.Success);

            var getIdAndVersionResponse = await _testProcesses.ClientV2Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V2 client"
            });

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V2 client, Version: V2 Server", getIdAndVersionResponse.ReturnValue);
        }

        public async Task DisposeAsync()
        {
            // shutdown order matters because primary must be the last to go
            var stopClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            var stopSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
            var stopSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
        }
    }
}