using IPCShared;
using IPCShared.BaseStuff;
using MainTests.Fixtures;

namespace MainTests.ReactivationScenarios.MementoStateTransition
{
    [Collection("TestProcesses")]
    public class Reactivation_MementoStateTransition_SiloV1ClientV1TransitionToSiloV1SiloV2ClientV2 : IAsyncLifetime
    {
        readonly TestProcessesFixture _testProcesses;
        ResponseMessageBase? _startSiloV1;
        ResponseMessageBase? _startSiloV2;
        ResponseMessageBase? _startClientV1;
        ResponseMessageBase? _startClientV2;

        public Reactivation_MementoStateTransition_SiloV1ClientV1TransitionToSiloV1SiloV2ClientV2(TestProcessesFixture testProcesses)
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

            // start client v1
            _startClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });
        }

        [Fact]
        public async Task Test()
        {
            Assert.True(_startSiloV1!.Success);
            Assert.True(_startClientV1!.Success);

            var acquireLeaseResponse = await _testProcesses.ClientV1Command.ExecuteAsync<AcquireLeaseRequest, AcquireLeaseResponse>(new AcquireLeaseRequest()
            {
                GrainId = "TestLease1",
            });

            // start silov2
            _startSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StartSiloRequest, ResponseMessageBase>(new StartSiloRequest()
            {
                GatewayPort = ConfigConstants.SiloV2_GatewayPort,
                SiloPort = ConfigConstants.SiloV2_SiloPort,
                PrimarySiloPort = ConfigConstants.SiloV1_SiloPort, // points to primary
                VersionCompatibility = VersionCompatibilitiy.BackwardCompatible,
                VersionSelector = VersionSelector.LatestVersion
            });

            // start client v2
            _startClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StartClientRequest, ResponseMessageBase>(new StartClientRequest()
            {
                GatewayPorts = new int[] { ConfigConstants.SiloV1_GatewayPort, ConfigConstants.SiloV2_GatewayPort }
            });

            // attempt to read the lease id via client 2

            var readLeaseResponse = await _testProcesses.ClientV2Command.ExecuteAsync<ReadLeaseIDRequest, ReadLeaseIDResponse>(new ReadLeaseIDRequest()
            {
                GrainId = "TestLease1", // same grain key
            });

            Assert.True(readLeaseResponse.Success);
            Assert.NotNull(readLeaseResponse.LeaseID);
            Assert.Equal(acquireLeaseResponse.LeaseID, readLeaseResponse.LeaseID);
        }

        public async Task DisposeAsync()
        {
            // shutdown order matters because primary must be the last to go
            var stopClientV1 = await _testProcesses.ClientV1Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            if (_startClientV2 != null && _startClientV2.Success)
            {
                var stopClientV2 = await _testProcesses.ClientV2Command.ExecuteAsync<StopClientRequest, ResponseMessageBase>(new StopClientRequest() { });
            }

            if (_startSiloV2 != null && _startSiloV2.Success)
            {
                var stopSiloV2 = await _testProcesses.SiloV2Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
            }
            var stopSiloV1 = await _testProcesses.SiloV1Command.ExecuteAsync<StopSiloRequest, ResponseMessageBase>(new StopSiloRequest() { });
        }
    }
}