using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Python.Core.Text;
using Microsoft.Python.LanguageServer.Protocol;
using TraceLevel = Microsoft.Python.LanguageServer.Protocol.TraceLevel;

namespace TestPls
{
    public class ParamsFactory
    {
        //private static string _filePath = "/home/ohad/PycharmProjects/PyDataModels/manage.py";
        private static string _rootPath = "/home/ohad/PycharmProjects/PyDataModels";
        private static string _cacheFolderPath = "/home/ohad/tmp";
        private static string _pythonPath = "/usr/bin/python3.6m";
        private static string _pythonVersion = "3.6.9";
        private static readonly SymbolKind [] SymbolKinds =
            Enum.GetValues<SymbolKind>().Where(symbolKind => symbolKind != SymbolKind.None).ToArray();

        public static InitializeParams GetInitObject(string rootPath)
        {
            _rootPath = rootPath;
            string[] incFiles = {"**/*.py"}; //GetAllFiles();
            string[] exFiles = {"venv"}; //GetAllFiles();
            //Console.WriteLine($"Process id {Process.GetCurrentProcess().Id}");
            
          
            var initObject = new InitializeParams
            {
                processId = Process.GetCurrentProcess().Id,
                rootPath = rootPath,
                trace = TraceLevel.Verbose,
                capabilities = new ClientCapabilities
                {
                    workspace = new WorkspaceClientCapabilities(),
                    textDocument = new TextDocumentClientCapabilities
                    {
                        documentSymbol = new TextDocumentClientCapabilities.DocumentSymbolCapabilities
                        {
                            symbolKind =
                                new TextDocumentClientCapabilities.DocumentSymbolCapabilities.SymbolKindCapabilities
                                {
                                    valueSet = SymbolKinds
                                }
                        },
                        references = new TextDocumentClientCapabilities.ReferencesCapabilities
                        {
                            dynamicRegistration = true
                        }
                    }
                },

                initializationOptions = new PythonInitializationOptions
                {
                    includeFiles = incFiles,
                    excludeFiles = exFiles,
                    cacheFolderPath = _cacheFolderPath,
                    interpreter = new PythonInitializationOptions.Interpreter
                    {
                        properties = new PythonInitializationOptions.Interpreter.InterpreterProperties
                        {
                            Version = _pythonVersion,
                            InterpreterPath = _pythonPath,
                        }
                    },
                }
            };
            
            return initObject;
        }

        public static string[] GetAllFiles()
        {
            var files = new List<string>();

            DirSearch(_rootPath, files);

            return files.ToArray();
        }

        static void DirSearch(string sDir, List<string> dirs)
        {
            if (sDir.EndsWith("venv"))
            {
                return;
            }

            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (string fileName in Directory.GetFiles(d, "*.py"))
                {
                    dirs.Add(fileName);
                }

                DirSearch(d, dirs);
            }
        }

        public static DocumentSymbolParams GetDocumentSymbolParams(string filePath)
        {
            DocumentSymbolParams docParams = new DocumentSymbolParams
            {
                textDocument = new TextDocumentIdentifier
                {
                    uri = new Uri(filePath)
                }
            };


            return docParams;
        }

        public static ReferencesParams GetReferenceParams(string filePath, Position position)
        {
            ReferencesParams res = new ReferencesParams
            {
                textDocument = new TextDocumentIdentifier
                {
                    uri = new Uri(filePath)
                },
                
                position = position,
                context = new ReferenceContext {includeDeclaration = true}
            };

            return res;
        }

        public static CompletionParams GetComplitionParams(string filePath, Position position)
        {
            return new CompletionParams
            {
                textDocument = new TextDocumentIdentifier
                {
                    uri = new Uri(filePath)
                },
                
                position = position,
            };
        }
        
        public static TextDocumentPositionParams GetPositionParams(string filePath, Position position)
        {
            return new TextDocumentPositionParams
            {
                textDocument = new TextDocumentIdentifier
                {
                    uri =  new Uri(filePath)
                },
                position = position
            }; 
        }
        
        public static DidOpenTextDocumentParams GetDidOpenParams(string filePath)
        {
            DidOpenTextDocumentParams res = new DidOpenTextDocumentParams
            {
                textDocument = new TextDocumentItem
                {
                    uri = new Uri(filePath),
                    text = File.ReadAllText(filePath),
                    version = 1
                },
            };
            
            return res;
        }
    }
}