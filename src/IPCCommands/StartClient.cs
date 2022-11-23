using IPCShared.BaseStuff;

namespace IPCShared
{
    public class StartClientRequest : RequestMessageBase
    {
        public int[] GatewayPorts { get; set; } // inbound connections     
    }

    public class StopClientRequest : RequestMessageBase
    {
    }
}
