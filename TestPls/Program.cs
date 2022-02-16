using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Python.Core.Text;
using Microsoft.Python.LanguageServer.Protocol;
using Nerdbank.Streams;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using StreamJsonRpc;

namespace TestPls
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string exePath = null;
            if (args.Length > 0)
            {
                exePath = args[0];
            }
            
            var repoPath = "/home/ohad/src/django";
            var fileName = "/home/ohad/src/django/django/shortcuts.py";
            await RpcTester.DoRpcAsync(repoPath, fileName, new Position { line = 18, character = 22 }, exePath);
            
            Console.WriteLine("The end");
        }
    }
}