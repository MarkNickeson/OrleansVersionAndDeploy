using Orleans.CodeGeneration;

namespace MatchingInterfaceNamespace
{
    [Version(1)]
    public interface IFoo : IGrainWithStringKey
    {
        Task<string> GetIdAndVersion(string? payload);
    }
}