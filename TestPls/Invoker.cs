using System;
using System.Threading.Tasks;
using Microsoft.Python.LanguageServer.Protocol;
using Polly;
using Polly.Retry;
using StreamJsonRpc;

namespace TestPls
{
    public class Invoker
    {
        private static readonly int NumberOfRetries = 10;
        private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<TaskCanceledException>()
            .WaitAndRetryAsync(NumberOfRetries, _ => TimeSpan.FromMilliseconds(3));

        public static async Task<InitializeResult> InvokeInitAsync(InitializeParams initParams, JsonRpc rpc)
        {
            return await InvokeWithParameterObjectAsync<InitializeResult>("initialize", rpc, initParams);
        }

        public static async Task InvokeInitializedAsync(JsonRpc rpc)
        {
            await InvokeWithParameterObjectAsync("initialized", rpc);
        }

        public static async Task<Hover> InvokeHoverAsync(TextDocumentPositionParams positionParams ,JsonRpc rpc)
        {
            return  await InvokeWithParameterObjectAsync<Hover>("textDocument/hover", rpc, positionParams);
        }
        
        public static async Task<Reference[]> InvokeGotoDefinitionAsync(
            TextDocumentPositionParams positionParams,
            JsonRpc rpc)
        {
            return  await InvokeWithParameterObjectAsync<Reference[]>("textDocument/definition", rpc, positionParams); 
        }
        
        private static async Task InvokeWithParameterObjectAsync(string targetName, JsonRpc rpc, object argument = null)
        {
            await RetryPolicy.ExecuteAsync(async () =>
            {
                await rpc.InvokeWithParameterObjectAsync(targetName, argument ?? new { });
            });
        }

        private static async Task<TResult> InvokeWithParameterObjectAsync<TResult>(string targetName, JsonRpc rpc, object argument = null)
        {
            TResult result = default;
            await RetryPolicy.ExecuteAsync(async () =>
            {
                result  = await rpc.InvokeWithParameterObjectAsync<TResult>(targetName, argument ?? new {} );
            });
        
            return result;
        }
    }
}