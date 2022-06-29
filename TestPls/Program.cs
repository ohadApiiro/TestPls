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
            
            var repoPath = "/home/ohad/PycharmProjects/python-playground";
            var fileName = "/home/ohad/PycharmProjects/python-playground/main.py";
            await RpcTester.DoRpcAsync(repoPath, fileName, new Position {line = 6, character = 1}, exePath);
            
            Console.WriteLine("The end");
        }
    }
}