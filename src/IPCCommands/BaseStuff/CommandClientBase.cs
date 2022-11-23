using H.Pipes;
using H.Pipes.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace IPCShared.BaseStuff
{    
    public abstract class CommandClientBase
    {
        static SemaphoreSlim _singleLocker = new SemaphoreSlim(1, 1);
        TaskCompletionSource<ResponseMessageBase>? _requestCompletionSource;
        readonly PipeClient<JsonPayloadWithType> _client;

        protected CommandClientBase(string pipename)
        {
            _client = new PipeClient<JsonPayloadWithType>(pipename, formatter: new H.Formatters.SystemTextJsonFormatter());
            _client.MessageReceived += Client_MessageReceived;
            _client.Connected += Client_Connected;
            _client.Disconnected += _client_Disconnected;
            // other possible events to handle
            // client.Disconnected
            // client.ExceptionOccurred
        }

        private void _client_Disconnected(object? sender, H.Pipes.Args.ConnectionEventArgs<JsonPayloadWithType> e)
        {
            Console.WriteLine("Client disconnected...");            
        }

        private void Client_Connected(object? sender, H.Pipes.Args.ConnectionEventArgs<JsonPayloadWithType> e)
        {
            Console.WriteLine("Connected");
        }

        private async void Client_MessageReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<JsonPayloadWithType?> e)
        {
            // convert payload to instance
            var type = Assembly.GetExecutingAssembly().GetType(e.Message!.TypeFullName!);
            var instance = JsonSerializer.Deserialize(e.Message.Payload!, type!);

            if (instance != null && e.Connection != null)
            {
                switch (instance)
                {
                    case InternalMessageBase internalMessage:
                        await HandleInternalMessage(internalMessage, e.Connection!);
                        return;
                    case ResponseMessageBase responseMessage:
                        HandleResponseMessage(responseMessage, e.Connection!);
                        return;
                    default:
                        throw new ApplicationException($"Unexpected message type {e.Message.GetType().FullName}");
                }
            }
        }

        private Task HandleInternalMessage(InternalMessageBase internalMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            return Task.CompletedTask;
        }

        void HandleResponseMessage(ResponseMessageBase responseMessage, PipeConnection<JsonPayloadWithType> connection)
        {
            _requestCompletionSource!.SetResult(responseMessage);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest requestMessage)
            where TRequest : RequestMessageBase
            where TResponse : ResponseMessageBase
        {
            await _singleLocker.WaitAsync();

            try
            {
                // prepare completion details
                _requestCompletionSource = new TaskCompletionSource<ResponseMessageBase>();

                // convert request message into json with type
                var transportMessage = new JsonPayloadWithType(requestMessage.GetType().FullName!, JsonSerializer.Serialize(requestMessage));

                // submit request message to server
                await _client.WriteAsync(transportMessage);

                // await completion with a timeout
                var response = await _requestCompletionSource.Task;

                _requestCompletionSource = null;

                if (response is TResponse r)
                {
                    return r;
                }
                else
                {
                    throw new ApplicationException($"Unexpected return type from IPC. {typeof(TResponse).FullName} expected but {response.GetType().FullName} returned");
                }
            }
            finally
            {
                _singleLocker.Release();
            }
        }

        public static async Task<T> StartCommandClientAsync<T>() where T : CommandClientBase, IStartCommandClient<T>
        {
            var rval = T.Create();
            await rval.StartAsync();
            return rval;
        }

        async Task StartAsync()
        {
            await _client.ConnectAsync();
        }
    }
}
