using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Scribi.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Scribi.Services
{
    public class RuntimeCompilerService : IRuntimeCompilerService
    {
        private readonly ILogger _logger;
        private readonly List<MetadataReference> _references = new List<MetadataReference>();
        private readonly List<Type> _resolvedTypes = new List<Type>();
        private readonly AssemblyLoadContext _context;
        private readonly DependencyContext _depContext;
        private IEnumerable<string> _usings;
        private string _location;
        private readonly ConcurrentDictionary<string, AssemblyMetadata> _metadataFileCache =new ConcurrentDictionary<string, AssemblyMetadata>(StringComparer.OrdinalIgnoreCase);


        public RuntimeCompilerService(ILogger<RuntimeCompilerService> logger)
        {
            _logger = logger;
            _context = AssemblyLoadContext.GetLoadContext(Assembly.GetEntryAssembly());
            _depContext = DependencyContext.Default;
        }

        #region IService Interface
        public void Configure(IConfigurationSection config)
        {
            _location = config.GetValue("Location",string.Empty);
            _usings = new List<string>();
            config.GetSection("Usings").Bind(_usings);
        }

        public void Init()
        {
            //Add references
            DetermineReferences();
            CompileFilesFromLocation("Scripts");
        }

        public void Release()
        {
            //Release all
        }
        #endregion

        public IEnumerable<Type> GetTypes()
        {
            return _resolvedTypes.ToList();
        }

        public Tuple<Assembly, IEnumerable<Type>> CompileFilesFromLocation(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(_location))
                return new Tuple<Assembly, IEnumerable<Type>>(null,new List<Type>());

            var codes = new List<string>();
            foreach (var item in Directory.GetFiles(_location, "*.cs"))
            {
                try
                {
                    var code = File.ReadAllText(item, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        codes.Add(code);
                        _logger.LogDebug($"File {item} added to runtime compiler.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception while adding file {item} to runtime compiler. Exception was: {ex.Message}");
                }
            }

            return CompileFiles(codes, assemblyName);
        }

        public Tuple<Assembly,IEnumerable<Type>> CompileFiles(IEnumerable<string> files, string assemblyName)
        {
            var result = new List<Type>();
            if (files.Any())
            {
                var asm = Compile(assemblyName, files);
                if (asm != null)
                {
                    result.AddRange(asm.GetExportedTypes());
                    _resolvedTypes.AddRange(result);
                    return new Tuple<Assembly, IEnumerable<Type>>(asm,result);
                }
                else
                    _logger.LogInformation($"Could not compile mappings assembly!");
            }
            else
                _logger.LogInformation($"No code to compile!");
            return new Tuple<Assembly, IEnumerable<Type>>(null,result);
        }

        private Assembly Compile(string name, IEnumerable<string> codes)
        {
            if (codes == null)
                throw new ArgumentNullException("code");

            if (!codes.Any())
                throw new ArgumentException("No code given!");

            var trees = new List<SyntaxTree>();
            var usings = _usings != null ? _usings.Select(s => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(s))).ToArray() : new UsingDirectiveSyntax[0];
            foreach (var code in codes)
            {
                // Parse the script to a SyntaxTree
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = (CompilationUnitSyntax)syntaxTree.GetRoot();

                if (usings.Any())
                    root = root.AddUsings(usings);
                
                trees.Add(SyntaxFactory.SyntaxTree(root));
            }


            // Compile the SyntaxTree to an in memory assembly
            var compilation = CSharpCompilation.Create(
                name,
                trees,
                _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

            using (var outputStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    var result = compilation.Emit(outputStream);
                    if (result.Success)
                    {
                        outputStream.Position = 0;
                        var asm = _context.LoadFromStream(outputStream);
                        outputStream.Position = 0;
                        _references.Add(MetadataReference.CreateFromStream(outputStream));
                        return asm;
                    }
                    else
                    {
                        foreach (var item in result.Diagnostics)
                            Console.WriteLine(item.ToString());
                        return null;
                    }
                }
            }
        }

        private void DetermineReferences()
        {
            foreach (var item in _depContext.CompileLibraries.SelectMany(x => x.ResolveReferencePaths()))
                _references.Add(MetadataReference.CreateFromFile(item));
        }
    }
}

