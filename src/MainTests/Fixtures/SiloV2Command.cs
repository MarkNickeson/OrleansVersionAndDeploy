using IPCShared;
using IPCShared.BaseStuff;

namespace MainTests.Fixtures
{
    public class SiloV2Command : CommandClientBase, IStartCommandClient<SiloV2Command>
    {
        SiloV2Command() : base(IpcServerNames.SILOV2)
        {
        }

        public static SiloV2Command Create()
        {
            return new SiloV2Command();
        }
    }
}
