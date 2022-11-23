using H.Pipes;
using IPCShared;
using IPCShared.BaseStuff;

namespace MainTests.Fixtures
{
    public class SiloV1Command : CommandClientBase, IStartCommandClient<SiloV1Command>
    {
        SiloV1Command() : base(IpcServerNames.SILOV1)
        {
        }

        public static SiloV1Command Create()
        {
            return new SiloV1Command();
        }
    }
}
