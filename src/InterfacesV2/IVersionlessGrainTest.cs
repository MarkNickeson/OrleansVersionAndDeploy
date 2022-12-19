using Orleans.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Version(1)]
    public interface IVersionlessGrainTest : IGrainWithStringKey
    {
        Task<string> GetLabel();
    }
}
