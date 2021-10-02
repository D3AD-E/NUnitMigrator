using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NUnitMigrator.Core.RewriterLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using DTEProject = EnvDTE.Project;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NUnitMigrator.Extention
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MigrateProjectCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6f7cdf65-8130-4676-bd20-8d325a1305e1");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;
        private readonly IVsOutputWindowPane _outputWindowPane;
        private readonly ErrorListProvider _errorListProvider;
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateProjectCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MigrateProjectCommand(AsyncPackage package, ErrorListProvider errorListProvider, OleMenuCommandService commandService, IVsOutputWindowPane outputWindowPane)
        {
            this._package = package ?? throw new ArgumentNullException(nameof(package));
            _errorListProvider = errorListProvider ?? throw new ArgumentNullException(nameof(errorListProvider));
            _outputWindowPane = outputWindowPane ?? throw new ArgumentNullException(nameof(outputWindowPane));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }
        private void OutputMessage(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _outputWindowPane.OutputString(text);
        }
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MigrateProjectCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, ErrorListProvider errorListProvider, IVsOutputWindowPane outputWindowPane)
        {
            // Switch to the main thread - the call to AddCommand in MigrateProjectCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MigrateProjectCommand(package,errorListProvider, commandService, outputWindowPane);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await InvokeRefactoringAsync();
            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "MigrateProjectCommand";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this._package,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

    

        private async Task InvokeRefactoringAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                bool diagnosticsWritten = false;
                _errorListProvider.Tasks.Clear();

                var workspace = await ServiceProvider.GetRoslynVisualStudioWorkspaceAsync();
                var solution = workspace.CurrentSolution;
                var newSolution = solution;
                var currentDTEProject = GetActiveProject();
                foreach (var projectId in solution.ProjectIds)
                {
                    var project = solution.GetProject(projectId);
                    if (!project.Name.Equals(currentDTEProject.Name))
                        continue;
                    if (project.HasDocuments && project.Language == "C#")
                    {
                        var documentIds = GetSupportedDocumentIds(project);

                        OutputMessage($"Updating project {project.FilePath}");

                        foreach (var documentId in documentIds)
                        {
                            var document = project.GetDocument(documentId);
                            OutputMessage($"Processing {document.FilePath}");

                            var semanticModel = await document.GetSemanticModelAsync();
                            var tree = await document.GetSyntaxTreeAsync();
                            var rewriter = new Rewriter(semanticModel);
                            var result = rewriter.Visit(await tree.GetRootAsync());


                            OutputMessage($"Saving changes in {document.FilePath}");
                            var newDocument = document.WithSyntaxRoot(result);
                            project = newDocument.Project;
                            newSolution = project.Solution;

                            ShowUnsupported(rewriter.Unsupported);
                        }

                        if (newSolution != solution)
                        {
                            if (!workspace.TryApplyChanges(newSolution))
                            {
                                OutputMessage("Changes not saved");
                            }

                            AddMSTestPackages(currentDTEProject);
                            //RemoveNUnitPackages(currentDTEProject);
                        }
                    }
                }

                if (diagnosticsWritten)
                {
                    _errorListProvider.Show();
                }
            }
            catch (Exception ex)
            {
                OutputMessage(ex.ToString());
            }
        }
        private static DTEProject GetActiveProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.DTE dte = Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
            return GetActiveProject(dte);
        }

        private static DTEProject GetActiveProject(EnvDTE.DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTEProject activeProject = null;

            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;
            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
            {
                activeProject = activeSolutionProjects.GetValue(0) as DTEProject;
            }

            return activeProject;
        }

        private void ShowUnsupported(List<UnsupportedNodeInfo> unsupportedNodes)
        {
            foreach (var node in unsupportedNodes)
            {
                OutputMessage(node.ToString());
                _errorListProvider.AddTask(node);
            }
        }

        private void AddMSTestPackages(DTEProject selectedProject)
        {
            OutputMessage("Adding MSTest packages");
            using (var packageInstaller = PackageManager.Setup(selectedProject, _outputWindowPane))
            {
                foreach (var package in ExtentionData.MSTestPackages)
                {
                    var spec = package.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                    if (spec.Count > 1)
                    {
                        packageInstaller.AddPackage(spec[0], spec[1]);
                    }
                    else
                    {
                        packageInstaller.AddPackage(spec[0]);
                    }
                }
            }
        }

        private void RemoveNUnitPackages(DTEProject selectedProject)
        {
            OutputMessage("Removing NUnit packages");
            using (var packageInstaller = PackageManager.Setup(selectedProject, _outputWindowPane))
            {
                foreach (var package in ExtentionData.NUnitPackages)
                {
                    packageInstaller.RemovePackage(package.Trim());
                }
            }
        }

        private static IEnumerable<DocumentId> GetSupportedDocumentIds(Project project)
        {
            return project.Documents
                .Where(document => document.SupportsSemanticModel &&
                                   document.SupportsSyntaxTree &&
                                   document.SourceCodeKind == SourceCodeKind.Regular)
                .Select(d => d.Id);
        }
    }
}
