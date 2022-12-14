using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.LatestVersionBackwardsCompatible.NewActivationScenarios.HomogenousCluster
{
    [Collection("TestProcesses")]
    public class NewActivation_HomogenousCluster_SiloV2ClientV1 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV1;

        public NewActivation_HomogenousCluster_SiloV2ClientV1(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        {
            // start silov2
            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV2_GatewayPort,
                SiloPort = ConfigConstants.SiloV2_SiloPort,
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v1
            _startClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV2_GatewayPort }
            });
        }

        [Fact]
        public async Task Test()
        {
            Assert.True(_startSiloV2!.Success);
            Assert.True(_startClientV1!.Success);

            var getIdAndVersionResponse = await _testProcesses.ClientV1Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V1 client"
            });

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V1 client, Version: V2 Server", getIdAndVersionResponse.ReturnValue);
        }

        public async Task DisposeAsync()
        {
            var stopClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            //Assert.True(stopClientV1.Success);

            var stopSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
            //Assert.True(stopSiloV2.Success);
        }
    }
}