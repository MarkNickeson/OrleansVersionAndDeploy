using Common;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiloV2
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

        public Task<Guid> DummyAcquire()
        {
            if (_activeLeaseId.HasValue) throw new ApplicationException("I'm a dummy lease, only first call works.");
            _activeLeaseId = Guid.NewGuid();
            return Task.FromResult<Guid>(_activeLeaseId.Value);
        }

        public Task<Guid?> ReadLeaseID()
        {
            return Task.FromResult(_activeLeaseId);
        }

        async Task IGrainBase.OnActivateAsync(System.Threading.CancellationToken token)
        {
            // check for memento
            var memento = GrainFactory.GetGrain<IDummyLeaseMemento>(this.GetPrimaryKeyString());
            _activeLeaseId = await memento.GetMemento();
        }

        Task IGrainBase.OnDeactivateAsync(Orleans.DeactivationReason reason, System.Threading.CancellationToken token)
        {
            // NOTE: MUST be ready for future transition, cannot "fix"
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
