namespace TestPls
{
    public class RestApiUrls
    {
        private readonly string ReadyApi = "ready";
        private readonly string InitializeApi = "initialize";
        private readonly string InitializedApi = "initialized";
        private readonly string resolveApi = "resolve";
        private readonly string batchResolveApi = "batchResolve";
        private readonly string HoverApi = "hover";
        private readonly string DidOpenApi = "didOpen";
        private readonly string DidCloseApi = "didClose";
        private readonly string DeclarationApi = "declaration";
        private readonly string DefinitionApi = "definition";
        private readonly string ShutdownApi = "shutdown";

        public string ReadyUrl { get; }
        public string InitializeUrl { get; }
        public string InitializedUrl { get; }
        public string ResolveUrl { get; }
        public string BatchResolveUrl { get; }
        public string HoverUrl { get; }
        public string DidOpenUrl { get; }
        public string DidCloseUrl { get; }
        public string DeclarationUrl { get; }
        public string DefinitionUrl { get; }
        
        public string Shutdown { get; }

        public RestApiUrls(int port)
        {
            var baseUrl = $"http://localhost:{port}/api";
            ReadyUrl = $"{baseUrl}/{ReadyApi}";
            InitializeUrl = $"{baseUrl}/{InitializeApi}";
            InitializedUrl = $"{baseUrl}/{InitializedApi}";
            ResolveUrl = $"{baseUrl}/{resolveApi}";
            BatchResolveUrl = $"{baseUrl}/{batchResolveApi}";
            HoverUrl = $"{baseUrl}/{HoverApi}";
            DidOpenUrl = $"{baseUrl}/{DidOpenApi}";
            DidCloseUrl = $"{baseUrl}/{DidCloseApi}";
            DeclarationUrl = $"{baseUrl}/{DeclarationApi}";
            DefinitionUrl = $"{baseUrl}/{DefinitionApi}";
            Shutdown = $"{baseUrl}/{ShutdownApi}";
        }
    }    
}