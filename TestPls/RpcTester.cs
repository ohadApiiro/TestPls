using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Python.Core.Text;
using Microsoft.Python.LanguageServer.Protocol;
using Nerdbank.Streams;
using Polly;
using StreamJsonRpc;

namespace TestPls
{
    public class RpcTester
    {
        public static async Task DoRpcAsync(string rootPath, string filePath, Position position, string exePath = null)
        {
            // /home/ohad/src/apiiro/python-language-server/output/bin/Debug/Microsoft.Python.LanguageServer.dll"
            // /home/ohad/src/apiiro/python-language-server/src/LanguageServer/Impl/bin/Release/net5.0/python/Microsoft.Python.LanguageServer.dll
            
            if (exePath == null)
            {
                exePath = "/Lim.FeaturesExtractor.Unified/ext/Microsoft.Python.LanguageServer.dll";
            }
            var process = StartProcess(exePath);
            if (process.HasExited)
            {
                Console.WriteLine("process is dead!");
            }
            
            using (var rpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream))
            {
                await DoInit(rootPath, rpc);
                OpenFile(filePath, rpc);

                await PrintSymbol(filePath, position, rpc);
                await Close(process, rpc);
            }

            Console.WriteLine();
        }

        private static async Task PrintSymbol(string filePath, Position position, JsonRpc rpc)
        {
            var positionParams = ParamsFactory.GetPositionParams(filePath, position);
            Hover hover = await GetHoverAsync(filePath, position, rpc);
            
            var msg = hover == null ? "hover api returned null" : hover.contents.value;
            Console.WriteLine($"{msg}");

            var definitions = await Invoker.InvokeGotoDefinitionAsync(positionParams, rpc); //rpc.InvokeWithParameterObjectAsync<Reference[]>("textDocument/definition", positionParams);
            msg = (definitions == null || definitions.Length == 0)
                ? "no results from goto definition api"
                : $"{definitions[0].uri}";
            Console.WriteLine(msg);
        }

        private static async Task<Hover> GetHoverAsync(string filePath, Position position, JsonRpc rpc)
        {
            var positionParams = ParamsFactory.GetPositionParams(filePath, position);

            var sleepMsec = 5;
            var count = 1;
            Hover hover = null;
            await Policy.HandleResult<bool>(b => b == false)
                .WaitAndRetryAsync(10,
                    _ =>
                    {
                        //Console.WriteLine($"value of _ {_}");
                        sleepMsec += 5;
                        return TimeSpan.FromMilliseconds(10);
                    })
                .ExecuteAsync(
                    async () =>
                    {
                        Console.WriteLine($"Retry count {count++}");
                        hover = await Invoker.InvokeHoverAsync(positionParams, rpc); //rpc.InvokeWithParameterObjectAsync<Hover>("textDocument/hover", positionParams);
                        if (hover == null)
                        {
                            Console.WriteLine("Hover is null");
                            return false;
                        }
                    
                        var className = hover.contents?.value?.Split(Environment.NewLine)[0] ?? string.Empty;
                        if (className.Contains("in progress"))
                        {
                            Console.WriteLine("in progress");
                            return false;
                        }

                        return true;
                    }
                );

            return hover;
        }
        private static async Task Close(Process process, JsonRpc rpc)
        {
            Console.WriteLine($"{process.Id}: End of operation reached");
            await rpc.InvokeWithParameterObjectAsync("shutdown", new { });
        }

        private static void OpenFile(string filePath, JsonRpc rpc)
        {
            var openParams = ParamsFactory.GetDidOpenParams(filePath);
            rpc.InvokeWithParameterObjectAsync("textDocument/didOpen", openParams);
        }

        private static async Task<Location> Gotodeclaration(JsonRpc rpc, TextDocumentPositionParams positionParams)
        {
            return await rpc.InvokeWithParameterObjectAsync<Location>("textDocument/declaration", positionParams);
        }

        private static async Task<Reference[]> FindAllReferences(string filePath, Position position, JsonRpc rpc)
        {
            var refParams = ParamsFactory.GetReferenceParams(filePath, position);
            return await rpc.InvokeWithParameterObjectAsync<Reference[]>("textDocument/references", refParams);
        }

        private static async Task DoInit(string rootPath, JsonRpc rpc)
        {
            InitializeParams initParams = ParamsFactory.GetInitObject(rootPath);
            await Invoker.InvokeInitAsync(initParams, rpc);
            await Invoker.InvokeInitializedAsync(rpc);
        }

        public static Process StartProcess(string exePath)
        {
            var exePath1 = "/Lim.FeaturesExtractor.Unified/ext/Microsoft.Python.LanguageServer.dll";

            Console.WriteLine($"starting {exePath}");
            var shortCmd = $"{exePath}";
            
            var psi = new ProcessStartInfo("/snap/dotnet-sdk/155/dotnet", shortCmd);
           
            var proc = new Process();
            proc.StartInfo = psi;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            return proc;
        }
    }
}