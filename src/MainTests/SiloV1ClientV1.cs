using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests
{
    [Collection("TestProcesses")]
    public class SiloV1ClientV1 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1;
        ResponseMessageBase? _startClientV1;

        public SiloV1ClientV1(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        {
            // start silov1
            _startSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                RendezvousPort = 11110,
                GatewayPort = 30000,
                SiloPort = 11110,
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v1
            _startClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPort = 30000
            });
        }

        [Fact]
        public async Task Activation_LatestVersion_BackwardCompatible_ExpectSuccess()
        {
            Assert.True(_startSiloV1!.Success);
            Assert.True(_startClientV1!.Success);

            var getIdAndVersionResponse = await _testProcesses.ClientV1Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V1 client"
            });                        

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V1 client, Version: V1 Server", getIdAndVersionResponse.ReturnValue);
        }

        public async Task DisposeAsync()
        {
            var stopClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });           
            var stopSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });        
        }
    }
}