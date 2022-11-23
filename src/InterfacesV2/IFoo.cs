using Orleans.CodeGeneration;

namespace Common
{
    [Version(2)]
    public interface IFoo : IGrainWithStringKey
    {
        Task<string> GetIdAndVersion(string? payload);
    }
}