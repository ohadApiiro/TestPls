using System;
using System.Threading.Tasks;
using Microsoft.Python.LanguageServer.Protocol;
using Newtonsoft.Json;
using RestSharp;

namespace TestPls
{
    [Serializable]
    public class  ReadyData
    {
        public string res;
    }
    public class PlsApi
    {
        private RestApiUrls _restApiUrls;

        public PlsApi(RestApiUrls restApiUrls)
            => _restApiUrls = restApiUrls;

        public async Task Initialize(InitializeParams initParams)
        {
            var res = await RestPostAsync<InitializeResult>(_restApiUrls.InitializeUrl, initParams);
            Console.WriteLine();
        }

        public async Task Initialized()
        {
            await RestPostAsync<int>(_restApiUrls.InitializedUrl, new {}, false);
        }

        public async Task<string> Ready()
        {
            var res =  await RestGetAsync(_restApiUrls.ReadyUrl);
            return res;
        }
        
        // public async Task<BatchResolveResults> BatchResolve(SourceFileMethodCalls sourceFileMethodCalls)
        // {
        //     return (BatchResolveResults)await RestPostAsync<BatchResolveResults>(_restApiUrls.BatchResolveUrl, sourceFileMethodCalls);
        // }
        
        public async Task<string> Resolve(TextDocumentPositionParams positionParams)
        {
            return (string)await RestPostAsync<string>(_restApiUrls.ResolveUrl, positionParams);
        }

        public async Task<Hover> Hover(TextDocumentPositionParams positionParams)
        {
            Hover hover = (Hover)await RestPostAsync<Hover>(_restApiUrls.HoverUrl, positionParams);
            return hover;
        }

        public async Task<Location> GotoDeclaration(TextDocumentPositionParams positionParams)
        {
            return (Location)await RestPostAsync<Location>(_restApiUrls.DeclarationUrl, positionParams);
        } 
        
        public async Task<Reference[]> GotoDefinition(TextDocumentPositionParams positionParams)
        {
            return (Reference[])await RestPostAsync<Reference[]>(_restApiUrls.DefinitionUrl, positionParams);
        }

        public async Task OpenTextDocument(DidOpenTextDocumentParams openParams)
        {
            await RestPostAsync<int>(_restApiUrls.DidOpenUrl, openParams, false);
        }

        public async Task Shutdown()
        {
            await RestPostAsync<int>(_restApiUrls.Shutdown, new {}, false);
        }

        public static async Task<string> RestGetAsync(string url)
        {
            string result = string.Empty;
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            try
            {
                var response = await client.ExecuteAsync(request);
                result = response.Content;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GET API call: {url} failed");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            

            return result;
        }
        
        private static async Task<object> RestPostAsync<T>(string url, object paramObj, bool getRes = true, object filters = null)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST).AddJsonBody(paramObj);
                if (filters != null)
                {
                    request.AddJsonBody(filters);
                }

                var response = await client.ExecuteAsync<T>(request);

                if (getRes)
                {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"POST API call: {url} failed");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        private static async Task<string> RestPostStringAsync(string url, object paramObj)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST).AddJsonBody(paramObj);

                var response = await client.ExecuteAsync<string>(request);
                return response.Content;
            }
            catch (Exception e)
            {
                Console.WriteLine($"POST API call: {url} failed");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }
    }
}