using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Python.Core.Text;
using Microsoft.Python.LanguageServer.Protocol;
using Nerdbank.Streams;
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
            Hover hover = await rpc.InvokeWithParameterObjectAsync<Hover>("textDocument/hover", positionParams);
            if (hover != null)
            {
                Console.WriteLine($"Hover results {hover.contents.value}");
            }

            Reference[] definitions =
                await rpc.InvokeWithParameterObjectAsync<Reference[]>("textDocument/definition", positionParams);
            if (definitions != null && definitions.Length > 0)
            {
                Console.WriteLine($"definition results {definitions[0]?.uri}");
            }
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

            InitializeResult res = await rpc.InvokeWithParameterObjectAsync<InitializeResult>("initialize", initParams);

            await rpc.InvokeWithParameterObjectAsync("initialized", new { });
            Thread.Sleep(10);
        }

        public static Process StartProcess(string exePath)
        {
            // var exePath =
            //     "/home/ohad/src/TestPls/TestPls/ext/Microsoft.Python.LanguageServer.dll";
            var exePath1 = "/Lim.FeaturesExtractor.Unified/ext/Microsoft.Python.LanguageServer.dll";

            Console.WriteLine($"starting {exePath}");
            var shortCmd = $"{exePath}";
            //var psi = new ProcessStartInfo("/home/ohad/.dotnet/dotnet", shortCmd);
            var psi = new ProcessStartInfo("/snap/dotnet-sdk/155/dotnet", shortCmd);
            //var psi = new ProcessStartInfo("dotnet", shortCmd);
            var proc = new Process();
            proc.StartInfo = psi;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            return proc;
        }
        
        private static async Task PrintLocationAsync(JsonRpc rpc, DocumentSymbol[] symbols, string filePath,
            string symbol)
        {
            var position = GetSymbolPosition(symbol, symbols);
            if (position == null)
            {
                Console.WriteLine($"symbol {symbol} not found");
                return;
            }

            var refParams = ParamsFactory.GetReferenceParams(filePath, position.Value);
            Reference[] references = await rpc.InvokeWithParameterObjectAsync<Reference[]>
                ("textDocument/references", refParams);

            Console.WriteLine($"found {references.Length} references");
            foreach (var reference in references)
            {
                Console.WriteLine($"{reference.range.start}");
            }

            var positionParams = ParamsFactory.GetPositionParams(filePath, references.Last().range.start);
            Reference[] defReferences = await rpc.InvokeWithParameterObjectAsync<Reference[]>
                ("textDocument/definition", positionParams);

            Console.WriteLine();
        }

        private static Position? GetSymbolPosition(string symbolName, DocumentSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                if (symbol.name == symbolName)
                {
                    return symbol.selectionRange.start;
                }
            }

            return null;
        }
       
        private static async Task OpenFilesAsync(IEnumerable<string> files, JsonRpc rpc)
        {
            foreach (var file in files)
            {
                var didOpenParams = ParamsFactory.GetDidOpenParams(file);
                await rpc.InvokeWithParameterObjectAsync("textDocument/didOpen", didOpenParams);
            }
        }
    }
}