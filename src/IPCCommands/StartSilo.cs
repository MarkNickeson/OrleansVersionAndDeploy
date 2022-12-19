using IPCShared.BaseStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPCShared
{
    public class StartSiloRequest : RequestMessageBase
    {
        public int? PrimarySiloPort { get; set; } = null;
        public int GatewayPort { get; set; } // inbound connections
        
        public int SiloPort { get; set; } // internode connections

        public VersionCompatibilitiy? VersionCompatibility { get; set; }
        public VersionSelector? VersionSelector { get; set; }
    }

    public class StopSiloRequest : RequestMessageBase
    {
    }
}
