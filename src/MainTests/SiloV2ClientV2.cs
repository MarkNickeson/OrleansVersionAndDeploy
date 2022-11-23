using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests
{
    [Collection("TestProcesses")]
    public class SiloV2ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV2;

        public SiloV2ClientV2(TestProcessesFixture testProcesses)
        {
            _testProcesses = testProcesses;
        }

        public async Task InitializeAsync()
        { 
            // start silov2
            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                RendezvousPort = 11110,
                GatewayPort = 30000,
                SiloPort = 11110,
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v2
            _startClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPort = 30000
            });            
        }

        [Fact]
        public async Task Activation_LatestVersion_BackwardCompatible_ExpectSuccess()
        {
            Assert.True(_startSiloV2!.Success);
            Assert.True(_startClientV2!.Success);

            var getIdAndVersionResponse = await _testProcesses.ClientV2Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V2 client"
            });                        

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal("GrainKey: TestGrain1, Payload: Invoked by V2 client, Version: V2 Server", getIdAndVersionResponse.ReturnValue);
        }

        public async Task DisposeAsync()
        {
            var stopClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            //Assert.True(stopClientV2.Success);

            var stopSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
            //Assert.True(stopSiloV2.Success);
        }
    }
}