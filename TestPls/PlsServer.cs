using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Python.LanguageServer.Protocol;
using Nerdbank.Streams;
using StreamJsonRpc;

namespace TestPls
{
    public class PlsServer
    {
        private string _rootPath;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly Process _process;
        private Stream _stream;

        public PlsServer(string rootPath, AutoResetEvent autoResetEvent)
        {
            _rootPath = rootPath;
            _autoResetEvent = autoResetEvent;
            _process = StartProcess();
        }

        public async Task DoInitAsync()
        {
            Console.WriteLine("do init");
            _stream = FullDuplexStream.Splice(_process.StandardOutput.BaseStream, _process.StandardInput.BaseStream);
            using (var rpc = JsonRpc.Attach(_stream))
            {
                var initParams = ParamsFactory.GetInitObject(_rootPath);
                Console.WriteLine("initialize");
                await rpc.InvokeWithParameterObjectAsync<InitializeResult>("initialize", initParams);
                
                Console.WriteLine("initialized");
                await rpc.InvokeWithParameterObjectAsync("initialized", new {});
                Thread.Sleep(1);
                await rpc.InvokeWithParameterObjectAsync("shutdown", new {});
                Console.WriteLine("after shoutdown");
            }

            _autoResetEvent.Set();
        }
        
        private  Process StartProcess()
        {
            var exePath =
                "/home/ohad/src/python-language-server/output/bin/Microsoft.Python.LanguageServer/Debug/Microsoft.Python.LanguageServer.dll";
            var projectPath =
                "/home/ohad/src/python-language-server/src/LanguageServer/Impl/Microsoft.Python.LanguageServer.csproj";
        
            var psi = new ProcessStartInfo("/home/ohad/.dotnet/dotnet", $"run {exePath} --project {projectPath}");
            var proc = new Process();
            proc.StartInfo = psi;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            return proc;
        } 
    }
}