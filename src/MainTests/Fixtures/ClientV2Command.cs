using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;

namespace MainTests.Fixtures
{
    public class ClientV2Command : CommandClientBase, IStartCommandClient<ClientV2Command>
    {
        private ClientV2Command() : base(IpcServerNames.CLIENTV2)
        {

        }

        public static ClientV2Command Create()
        {
            return new ClientV2Command();
        }
    }
}
