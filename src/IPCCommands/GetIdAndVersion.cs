using IPCShared.BaseStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCShared
{
    public class GetIdAndVersionRequestX : RequestMessageBase
    {
        public string? GrainId { get; set; }
        public string? InboundPayload { get; set; }
    }

    public class GetIdAndVersionResponse : ResponseMessageBase
    {        
        public string? ReturnValue { get; set; }
    }
}
