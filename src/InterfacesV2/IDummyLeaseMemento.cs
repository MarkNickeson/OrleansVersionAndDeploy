using Orleans.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Version(2)]
    public interface IDummyLeaseMemento : IGrainWithStringKey
    {
        Task SetMemento(Guid activeLeaseID);
        Task<Guid?> GetMemento();
    }
}
