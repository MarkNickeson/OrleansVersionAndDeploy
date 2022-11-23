using H.Pipes;
using H.Pipes.AccessControl;
using H.Pipes.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace IPCShared.BaseStuff
{
    public abstract class CommandServerBase
    {
        static int _clientCount = 0;
        TaskCompletionSource _completionSource;
        readonly string _pipename;
        PipeServer<JsonPayloadWithType>? _server;
        bool _clientConnected = false;

        public Task Completed => _completionSource.Task;

        protected CommandServerBase(string pipename)
        {
            _pipename = pipename;
            _completionSource = new TaskCompletionSource();
        }

        public async Task StartAsync()
        {
            _server = new PipeServer<JsonPayloadWithType>(_pipename, new H.Formatters.SystemTextJsonFormatter());
            _server.ClientConnected += Server_ClientConnected;
            _server.MessageReceived += Server_MessageReceived;
            _server.ClientDisconnected += _server_ClientDisconnected;

            await _server.StartAsync();
        }

        private void _server_ClientDisconnected(object? sender, H.Pipes.Args.ConnectionEventArgs<JsonPayloadWithType> e)
        {
            Console.WriteLine("Client disconnected...");
            if (_clientConnected )
            {                
                Process.GetCurrentProcess().Kill();
                Console.WriteLine("After kill...");
            }
        }

        private async void Server_MessageReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<JsonPayloadWithType?> e)
        {
            var type = Assembly.GetExecutingAssembly().GetType(e.Message!.TypeFullName!);
            var instance = JsonSerializer.Deserialize(e.Message.Payload!, type!);

            if (instance != null & e.Connection != null)
            {
                switch (instance!)
                {
                    case InternalMessageBase internalMessage:
                        await HandleInternalMessage(internalMessage, e.Connection!);
                        break;
                    case RequestMessageBase requestMessage:
                        await HandleRequestMessage(requestMessage, e.Connection!);
                        break;
                    default:
                        Console.WriteLine($"Unexpected message type: {instance!.GetType().FullName}");
                        break;
                }
            }
        }

        private async Task HandleInternalMessage(InternalMessageBase internalMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            switch (internalMessage)
            {
                case QuitCommandMessage quitMessage:
                    await connection.DisposeAsync();
                    _completionSource.SetResult();
                    return;
                default:
                    Console.WriteLine($"Unexpected internal message: {internalMessage.GetType().FullName}");
                    return;
            }
        }

        async Task HandleRequestMessage(RequestMessageBase requestMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            try
            {
                var responseMessage = await HandleRequestMessageOverride(requestMessage, connection);
                var jsonText = JsonSerializer.Serialize(responseMessage, responseMessage.GetType());

                // convert request message into json with type
                var transportMessage = new JsonPayloadWithType(responseMessage.GetType().FullName!, jsonText);

                // write the response message back to client
                await connection.WriteAsync(transportMessage);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException("Not done yet", ex);
            }
        }

        protected abstract Task<ResponseMessageBase> HandleRequestMessageOverride(RequestMessageBase requestMessageBase, PipeConnection<JsonPayloadWithType> connection);

        private async void Server_ClientConnected(object? sender, H.Pipes.Args.ConnectionEventArgs<JsonPayloadWithType> e)
        {
            _clientConnected = true;
            Console.WriteLine("Client connected");

            // allow only one, reject otherwise
            if (Interlocked.CompareExchange(ref _clientCount, 1, 0) != 0)
            {
                Console.WriteLine("Client rejected");
                // rejected
                await e.Connection.StopAsync();
                return;
            }

            //await e.Connection.WriteAsync(new ClientHelloMessage() { ClientID = Guid.NewGuid() });
        }
    }

}