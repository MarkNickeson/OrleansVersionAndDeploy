using Common;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiloV1
{
    public class VersionlessGrainTest : IGrainBase, IVersionlessGrainTest
    {
        public IGrainContext GrainContext { get; }        

        public VersionlessGrainTest(IGrainContext grainContext)
        {
            GrainContext = grainContext;
        }

        public Task<string> GetLabel()
        {
            return Task.FromResult("Unversioned");
        }
    }
}
