using Common;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiloV1
{
    public class DummyLease : IGrainBase, IDummyLease
    {
        Guid? _activeLeaseId;

        public IGrainContext GrainContext { get; }
        public IGrainFactory GrainFactory { get; }

        public DummyLease(IGrainContext grainContext, IGrainFactory grainFactory)
        {
            GrainContext = grainContext;
            GrainFactory = grainFactory;
        }

        public Task<Guid?> ReadLeaseID()
        {
            return Task.FromResult(_activeLeaseId);
        }

        public Task<Guid> DummyAcquire()
        {
            if (_activeLeaseId.HasValue) throw new ApplicationException("I'm a dummy lease, only first call works.");
            _activeLeaseId = Guid.NewGuid();
            return Task.FromResult<Guid>(_activeLeaseId.Value);
        }

        Task IGrainBase.OnDeactivateAsync(Orleans.DeactivationReason reason, System.Threading.CancellationToken token)
        {
            if (_activeLeaseId.HasValue)
            {
                if (reason.ReasonCode == DeactivationReasonCode.IncompatibleRequest)
                {
                    var memento = GrainFactory.GetGrain<IDummyLeaseMemento>(this.GetPrimaryKeyString());
                    memento.SetMemento(_activeLeaseId.Value);
                }
            }

            return Task.CompletedTask;
        }
    }
}
