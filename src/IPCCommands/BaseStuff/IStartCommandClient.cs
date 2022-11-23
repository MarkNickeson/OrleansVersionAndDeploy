namespace IPCShared.BaseStuff
{
    public interface IStartCommandClient<T> where T : CommandClientBase
    {
        static abstract T Create();
    }    
}
