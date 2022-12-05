using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.MinimumVersionStrictCompatible.NewActivationScenarios.HomogenousCluster
{
    [Collection("TestProcesses")]
    public class NewActivation_HomogenousCluster_SiloV1ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1Response;
        ResponseMessageBase? _startClientV2Response;

        public NewActivation_HomogenousCluster_SiloV1ClientV2(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        {
            // start silov1
            _startSiloV1Response = await _testProcesses.SiloV1Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV1_GatewayPort,
                SiloPort = ConfigConstants.SiloV1_SiloPort,
                VersionCompatibility = VersionCompatibilitiy.StrictVersionCompatible,
                VersionSelector = VersionSelector.MinimumVersion
            });

            // start client v2
            _startClientV2Response = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort }
            });
        }

        [Fact]
        public async Task Test()
        {
            Assert.True(_startSiloV1Response!.Success);
            Assert.True(_startClientV2Response!.Success);

            var getIdAndVersionResponse = await _testProcesses.ClientV2Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V2 client"
            });

            // expect failure because V1 silo
            Assert.False(getIdAndVersionResponse.Success);
        }

        public async Task DisposeAsync()
        {
            var stopClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            var stopSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
        }
    }
}