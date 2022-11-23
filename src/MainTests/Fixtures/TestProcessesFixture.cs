using IPCShared.BaseStuff;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainTests.Fixtures
{
    public class TestProcessesFixture : IAsyncLifetime
    {
        const bool USE_SHELL_EXECUTE_TO_MAKE_VISIVLE_CONSOLE = false;

        Process _siloV1Process;
        Process _siloV2Process;
        Process _clientV1Process;
        Process _clientV2Process;
        SiloV1Command? _siloV1Command;
        SiloV2Command? _siloV2Command;
        ClientV1Command? _clientV1Command;
        ClientV2Command? _clientV2Command;

        public SiloV1Command SiloV1Command => _siloV1Command!;
        public SiloV2Command SiloV2Command => _siloV2Command!;
        public ClientV1Command ClientV1Command => _clientV1Command!;
        public ClientV2Command ClientV2Command => _clientV2Command!;

        public TestProcessesFixture()
        {
            _siloV1Process = StartSiloV1Process();
            _siloV2Process = StartSiloV2Process();
            _clientV1Process = StartClientV1Process();
            _clientV2Process = StartClientV2Process();
        }

        Process StartSiloV1Process()
        {
            if (!File.Exists(@"..\..\..\..\SiloV1\bin\Debug\net7.0\SiloV1.exe"))
                throw new ApplicationException("Relative path is incorrect OR filename has changed");

            var p = new Process();
            p.StartInfo.UseShellExecute = USE_SHELL_EXECUTE_TO_MAKE_VISIVLE_CONSOLE;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.FileName = @"..\..\..\..\SiloV1\bin\Debug\net7.0\SiloV1.exe"; // solutionDir\MainTests\bin\Debug\net7.0 is assumed base for relative 

            if (!p.Start())
            {
                throw new ApplicationException($"Start {nameof(StartSiloV1Process)} process failed");
            }

            return p;
        }

        Process StartSiloV2Process()
        {
            if (!File.Exists(@"..\..\..\..\SiloV2\bin\Debug\net7.0\SiloV2.exe"))
                throw new ApplicationException("Relative path is incorrect OR filename has changed");

            var p = new Process();
            p.StartInfo.UseShellExecute = USE_SHELL_EXECUTE_TO_MAKE_VISIVLE_CONSOLE;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.FileName = @"..\..\..\..\SiloV2\bin\Debug\net7.0\SiloV2.exe"; // solutionDir\MainTests\bin\Debug\net7.0 is assumed base for relative 

            if (!p.Start())
            {
                throw new ApplicationException($"Start {nameof(StartSiloV2Process)} process failed");
            }

            return p;
        }

        Process StartClientV1Process()
        {
            var filename = @"..\..\..\..\ClientV1\bin\Debug\net7.0\ClientV1.exe";

            if (!File.Exists(filename))
                throw new ApplicationException("Relative path is incorrect OR filename has changed");

            var p = new Process();
            p.StartInfo.UseShellExecute = USE_SHELL_EXECUTE_TO_MAKE_VISIVLE_CONSOLE;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.FileName = filename; // solutionDir\MainTests\bin\Debug\net7.0 is assumed base for relative 

            if (!p.Start())
            {
                throw new ApplicationException($"Start {nameof(StartClientV1Process)} process failed");
            }
            return p;
        }

        Process StartClientV2Process()
        {
            var filename = @"..\..\..\..\ClientV2\bin\Debug\net7.0\ClientV2.exe";

            if (!File.Exists(filename))
                throw new ApplicationException("Relative path is incorrect OR filename has changed");

            var p = new Process();
            p.StartInfo.UseShellExecute = USE_SHELL_EXECUTE_TO_MAKE_VISIVLE_CONSOLE;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.FileName = filename; // solutionDir\MainTests\bin\Debug\net7.0 is assumed base for relative 

            if (!p.Start())
            {
                throw new ApplicationException($"Start {nameof(StartClientV2Process)} process failed");
            }
            return p;
        }

        public async Task InitializeAsync()
        {
            // start ipc clients
            _siloV1Command = await CommandClientBase.StartCommandClientAsync<SiloV1Command>();
            _siloV2Command = await CommandClientBase.StartCommandClientAsync<SiloV2Command>();
            _clientV1Command = await CommandClientBase.StartCommandClientAsync<ClientV1Command>();
            _clientV2Command = await CommandClientBase.StartCommandClientAsync<ClientV2Command>();
        }

        public Task DisposeAsync()
        {
            _siloV1Process.Kill();
            _siloV2Process.Kill();
            _clientV1Process.Kill();
            _clientV2Process.Kill();
            return Task.CompletedTask;
        }
    }
}