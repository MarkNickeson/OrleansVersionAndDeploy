using Common;
using Orleans.Runtime;

namespace SiloV1
{
    public class DummyLeaseMemento : IGrainBase, IDummyLeaseMemento
    {
        Guid? _activeLeaseId;

        public IGrainContext GrainContext { get; }

        public DummyLeaseMemento(IGrainContext grainContext)
        {
            GrainContext = grainContext;
        }

        public Task SetMemento(Guid activeLeaseID)
        {
            if (_activeLeaseId.HasValue) throw new ApplicationException("I'm just a simple lease memento, only able to call once");
            _activeLeaseId = activeLeaseID;
            return Task.CompletedTask;
        }

        public Task<Guid?> GetMemento()
        {
            try
            {
                if (_activeLeaseId.HasValue)
                {
                    try
                    {
                        return Task.FromResult(_activeLeaseId);
                    }
                    finally
                    {
                        _activeLeaseId = null;
                    }
                }
                else
                {
                    return Task.FromResult<Guid?>(null);
                }
            }
            finally
            {
                this.DeactivateOnIdle(); // want memento to be cleaned up ASAP
            }
        }
    }
}
