using Orleans.CodeGeneration;

namespace Common
{
    [Version(1)]
    public interface IFoo : IGrainWithStringKey
    {
        Task<string> GetIdAndVersion(string? payload);
    }
}