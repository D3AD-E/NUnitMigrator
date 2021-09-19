using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;
using NUnitMigrator.Core.RewriterLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.App.Logic
{
    internal class MainLogic
    {
        public static async void Migrate(string path)
        {
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1 ? visualStudioInstances[0]
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}'.");

            MSBuildLocator.RegisterInstance(instance);
            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);
                Console.WriteLine($"Loading solution '{path}'");
                var solution = await workspace.OpenSolutionAsync(path);

                int documentCount = 0;
                foreach (var projectId in solution.ProjectIds)
                {
                    var project = solution.GetProject(projectId);
                    Console.WriteLine($"Project.CompilationOptions.Platform: {project.CompilationOptions.Platform}");
                    Console.WriteLine($"Project.CompilationOptions.Language: {project.CompilationOptions.Language}");
                    Console.WriteLine($"Project.CompilationOptions.OptimizationLevel: {project.CompilationOptions.OptimizationLevel}");
                    Console.WriteLine($"Project.CompilationOptions.WarningLevel: {project.CompilationOptions.WarningLevel}");
                    Console.WriteLine($"Project.CompilationOptions.OutputKind: {project.CompilationOptions.OutputKind}");
                    foreach (var documentId in project.DocumentIds)
                    {
                        Document document = project.GetDocument(documentId);
                        if (document.SourceCodeKind != SourceCodeKind.Regular)
                            continue;
                        Console.Write($"Checking document: {document.Name}... ");
                        var model = await document.GetSemanticModelAsync();
                        var rewriter = new Rewriter(model);
                        var tree = await document.GetSyntaxTreeAsync();
                        var root = await tree.GetRootAsync();
                        if (!IsNUnitTestFile(root))
                        {
                            WriteLineInColor("Skipped", ConsoleColor.Yellow);
                            continue;
                        }

                        var result = rewriter.Visit(tree.GetRoot());
                        documentCount++;
                        solution = solution.WithDocumentSyntaxRoot(documentId, result);

                        if (rewriter.Unsupported.Count > 0)
                        {
                            WriteLineInColor($"{rewriter.Unsupported.Count} errors encountered", ConsoleColor.Red);

                            foreach (var unsupprotedNode in rewriter.Unsupported)
                            {
                                Console.WriteLine(unsupprotedNode.ToString());
                            }

                            WriteLineInColor($"{document.Name} Processed", ConsoleColor.Green);
                        }
                        else
                        {
                            WriteLineInColor("Processed", ConsoleColor.Green);
                        }

                    }
                }
                Console.WriteLine($"Changed {documentCount} documents");
                var applyResult = workspace.TryApplyChanges(solution);
                if (applyResult)
                    WriteLineInColor("Successfully applied changes", ConsoleColor.Green);
                else
                    WriteLineInColor("Could not apply changes", ConsoleColor.Red);
            }
        }

        private static bool IsNUnitTestFile(SyntaxNode root)
        {
            foreach (var usingDirective in root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList())
                if (usingDirective.ToString().Equals("using NUnit.Framework;"))
                    return true;
            return false;
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private static void WriteLineInColor(string text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
