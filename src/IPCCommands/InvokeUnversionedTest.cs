using IPCShared.BaseStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCShared
{
    public class InvokeUnversionedTestRequest : RequestMessageBase
    {
        public string? GrainId { get; set; }
    }

    public class InvokeUnversionedTestResponse : ResponseMessageBase
    {        
        public string? ReturnValue { get; set; }
    }
}
