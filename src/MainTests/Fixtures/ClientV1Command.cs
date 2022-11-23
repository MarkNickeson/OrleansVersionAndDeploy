using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;

namespace MainTests.Fixtures
{
    public class ClientV1Command : CommandClientBase, IStartCommandClient<ClientV1Command>
    {
        private ClientV1Command() : base(IpcServerNames.CLIENTV1)
        {

        }

        public static ClientV1Command Create()
        {
            return new ClientV1Command();
        }
    }
}
