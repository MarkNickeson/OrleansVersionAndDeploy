namespace IPCShared.BaseStuff
{
    public abstract class MessageBase
    {
    }

    public class RpcMessageBase : MessageBase
    {
    }

    internal class InternalMessageBase : MessageBase
    {
    }

    internal sealed class QuitCommandMessage : InternalMessageBase
    {
    }

    internal sealed class ClientHelloMessage : InternalMessageBase
    {
        public Guid ClientID { get; set; }
    }

    public abstract class RequestMessageBase : RpcMessageBase
    {
    }

    public class ResponseMessageBase : RpcMessageBase
    {
        public bool Success { get; set; } = false;
        public string? Error { get; set; }
    }

    public class JsonPayloadWithType
    {
        public string TypeFullName { get; set; }
        public string Payload { get; set; }

        public JsonPayloadWithType(string typeFullName, string payload)
        {
            TypeFullName = typeFullName;
            Payload = payload;
        }
    }
}