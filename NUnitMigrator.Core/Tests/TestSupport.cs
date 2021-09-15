using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NUnitMigrator.Core.Rewriter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Tests
{
    
    public static class TestSupport
    {
        public class TestResult
        {
            public string Text;
            public List<UnsupportedNodeInfo> Errors;
        }
        public static TestResult RunTest(string input)
        {
            var tree = Parse(input);

            var metadata = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(NUnit.Framework.TestFixtureAttribute).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create("Test", new[] { tree }, metadata);
           
            var semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees[0]);

            var diagnostics = compilation.GetDiagnostics();

            foreach (var error in diagnostics)
            {
                Console.WriteLine(error);
            }
            var rewriter = new Rewriter(semanticModel);
            
            var text = rewriter.Visit(tree.GetRoot());
            TestResult result = new TestResult
            {
                Errors = rewriter.Unsupported,
                Text = text.ToFullString()
            };

            return result;
    }

        private static SyntaxTree Parse(string text)
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.None)
                .WithLanguageVersion(LanguageVersion.Latest);
            var parsedText = SourceText.From(text, Encoding.UTF8);
            return SyntaxFactory.ParseSyntaxTree(parsedText, options);
        }
    }
}
