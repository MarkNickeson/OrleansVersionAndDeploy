using IPCShared.BaseStuff;

namespace IPCShared
{
    public class StartClientRequest : RequestMessageBase
    {
        public int GatewayPort { get; set; } // inbound connections     
    }

    public class StopClientRequest : RequestMessageBase
    {
    }
}
