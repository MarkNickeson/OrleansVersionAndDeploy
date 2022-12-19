using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.OrleansDefaults.ExistingActivationScenarios.HeterogenousCluster
{
    [Collection("TestProcesses")]
    public class VersionedToVersioned_SiloV1ClientV1_To_SiloV1SiloV2ClientV1ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV1;
        ResponseMessageBase? _startClientV2;

        public VersionedToVersioned_SiloV1ClientV1_To_SiloV1SiloV2ClientV1ClientV2(TestProcessesFixture testProcesses)
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
                VersionCompatibility = null, // use orleans default
                VersionSelector = null // use orleans default
            });            

            // start client v1
            _startClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });            
        }

        [Fact]
        public async Task ClientV1ActivatesGrainOnSiloV1_SiloV2Starts_ClientV2Starts_ClientV2AccessSameGrain_Reactivated()
        {
            Assert.True(_startSiloV1!.Success);
            Assert.True(_startClientV1!.Success);                    

            // use client V1 to cause activation on Silo V1
            var getIdAndVersionResponse = await _testProcesses.ClientV1Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1",
                InboundPayload = "Invoked by V1 client"
            });

            Assert.True(getIdAndVersionResponse.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V1 client, Version: V1 Server", getIdAndVersionResponse.ReturnValue);

            // start v2 silo
            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV2_GatewayPort,
                SiloPort = ConfigConstants.SiloV2_SiloPort,
                PrimarySiloPort = ConfigConstants.SiloV1_SiloPort,
                VersionCompatibility = null, // use orleans default
                VersionSelector = null // use orleans default
            });

            // start client v2
            _startClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });

            // use client V2 to access same grain
            var getIdAndVersionResponse2 = await _testProcesses.ClientV2Command.ExecuteAsync<GetIdAndVersionRequestX, GetIdAndVersionResponse>(new GetIdAndVersionRequestX()
            {
                GrainId = "TestGrain1", // same grain key
                InboundPayload = "Invoked by V2 client"
            });

            Assert.True(getIdAndVersionResponse2.Success);
            Assert.Equal($"GrainKey: TestGrain1, Payload: Invoked by V2 client, Version: V2 Server", getIdAndVersionResponse2.ReturnValue);
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