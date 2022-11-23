using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests
{
    [Collection("TestProcesses")]
    public class SiloV1ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1Response;
        ResponseMessageBase? _startClientV2Response;

        public SiloV1ClientV2(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        {
            // start silov1
            _startSiloV1Response = await _testProcesses.SiloV1Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                RendezvousPort = 11110,
                GatewayPort = 30000,
                SiloPort = 11110,
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v2
            _startClientV2Response = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPort = 30000
            });
        }


        [Fact]
        public async Task Activation_LatestVersion_BackwardCompatible_ExpectFail()
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