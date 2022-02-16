using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Python.Core.Text;
using Microsoft.Python.LanguageServer.Protocol;
using Newtonsoft.Json;
using RestSharp;
using Refit;

namespace TestPls
{
    [Serializable]
    public class SomeClass
    {
        public Uri Uri { get; set; }
    }

    public interface IRefitExample
    {
        [Post("/api/example")]
        Task<string> Example(SomeClass someClass);
    }
    
    public interface ILanguageServerApi
    {
        [Get("/api/ready")]
        Task<string> Ready();

        [Post("/api/initialize")]
        Task<string> Initialize(InitializeParams initializeWrapperParams);
        
        [Post("/api/initialized")]
        Task Initialized([Body] InitializedParams initializedParams);

        [Post("/api/hover")]
        Task<string> Hover(TextDocumentPositionParams positionParams);

        [Post("/api/declaration")]
        Task<string> GotoDeclaration([Body] TextDocumentPositionParams positionParams);
    }
    
    public class Tester
    {
        private PlsApi _api;
        private static string _rootPath = "/home/ohad/PycharmProjects/PyRepo";
        
        private string _fileName = $"{_rootPath}/main.py";
        private ILanguageServerApi _apiRefit;

        public Tester()
        {
            _api = new PlsApi(new RestApiUrls(5000));
            var settings = new RefitSettings(new NewtonsoftJsonContentSerializer());
            _apiRefit = RestService.For<ILanguageServerApi>($"http://localhost:5000", settings);
        }
        
        public async Task Init(string rootPath)
        {
            var ready = await _api.Ready();
            if (ready == "ready")
            {
                Console.WriteLine("ready success");
            }
            
            var initializeWrapperParams = ParamsFactory.GetInitObject(_rootPath);
            await _api.Initialize(initializeWrapperParams); 
            await _api.Initialized();
            Console.WriteLine();
        }

        // public async Task ResolveFile(string file, IEnumerable<Position> positions)
        // {
        //     SourceFileMethodCalls sourceFileMethodCalls = new SourceFileMethodCalls
        //     {
        //         FilePath = file,
        //         Positions = positions.ToArray()
        //     };
        //     BatchResolveResults res = await _api.BatchResolve(sourceFileMethodCalls);
        //     if (res == null)
        //     {
        //         Console.WriteLine("BatchResolveResults is null");
        //     }
        // }
        public async Task PrintSymbol(string file, int row, int column)
        {
            var position = new Position { line = row, character = column };
            var positionParams = ParamsFactory.GetPositionParams(file, position);

            var resolved = await _api.Resolve(positionParams);
            Console.WriteLine($"resolved {resolved}");
            
            Hover hover = await _api.Hover(positionParams);
            
            if (hover == null)
            {
                Console.WriteLine($"Hover failed {position.line}:{position.character}");
                return;
            }

            Console.WriteLine($"Hover success {hover.contents.value} pos {position.line}:{position.character}");

            var end = hover.range.Value.end;
            var fileName = await GetSymbolFileName(_fileName, hover.range.Value.start);
            //await _api.Shutdown();
            Console.WriteLine($"{fileName ?? string.Empty}|{hover.contents?.value ?? string.Empty}");
        }
        
        private async Task<string> GetSymbolFileName(string filePath, Position position)
        {
            TextDocumentPositionParams positionParams = ParamsFactory.GetPositionParams(filePath, position);
            //Location location = await _api.GotoDeclaration(positionParams);

            Reference[] references = await _api.GotoDefinition(positionParams);
            return references[0]?.uri?.ToString();
        }
        
        public async Task PrintSymbolRefit(int row, int column)
        {
            var position = new Position { line = row, character = column };
            var positionParams = ParamsFactory.GetPositionParams(_fileName, position);
            var json = await _apiRefit.Hover(positionParams);
            
            if (string.IsNullOrEmpty(json))
            {
                Console.WriteLine($"Hover failed {position.line}:{position.character}");
                return;
            }

            Console.WriteLine("hover success");

        }
        
        private  async Task OpenFilesAsync(IEnumerable<string> filesToOpen)
        {
            int i = 0;
            foreach (var file in filesToOpen)
            {
                var didOpenParams = ParamsFactory.GetDidOpenParams(file);
                await _api.OpenTextDocument(didOpenParams);
                Console.WriteLine(i++);
            }
        }

        public async Task InitRefit()
        {
            var ready = await _apiRefit.Ready();
            InitializeParams initializeWrapperParams = ParamsFactory.GetInitObject(_rootPath);
            var initRes = await _apiRefit.Initialize(initializeWrapperParams);
            await _apiRefit.Initialized(new InitializedParams());
            Console.WriteLine();
        } 
        
        private void GetAllFiles(string root, List<string> files)
        {
            // if (root.StartsWith("/home/ohad/src/django/tests"))
            // {
            //     return;
            // }
            foreach (var file in Directory.GetFiles(root, "*.py"))
            {
                files.Add(file);
            }

            foreach (var directory in Directory.GetDirectories(root))
            {
                GetAllFiles(directory, files);
            }
        }
    }
}