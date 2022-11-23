using Common;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiloV2
{
    public class Foo : IGrainBase, IFoo
    {
        public IGrainContext GrainContext { get; }

        public Foo(IGrainContext grainContext)
        {
            GrainContext = grainContext;
        }

        public Task<string> GetIdAndVersion(string? payload)
        {
            return Task.FromResult($"GrainKey: {this.GetPrimaryKeyString()}, Payload: {payload}, Version: V2 Server");
        }
    }
}
