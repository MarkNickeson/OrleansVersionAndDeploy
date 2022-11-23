using Orleans.CodeGeneration;

namespace MatchingInterfaceNamespace
{
    [Version(2)]
    public interface IFoo : IGrainWithStringKey
    {
        Task<string> GetIdAndVersion(string? payload);
    }
}