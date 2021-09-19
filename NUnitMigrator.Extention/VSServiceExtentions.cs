using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using NUnitMigrator.Core.RewriterLogic;


namespace NUnitMigrator.Extention
{
    public static class VSServiceExtentions
    {
        public static async Task<VisualStudioWorkspace> GetRoslynVisualStudioWorkspaceAsync(this IAsyncServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var componentModel = await serviceProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            if (componentModel == null)
                return null;
            var workspace = componentModel.GetService<VisualStudioWorkspace>();
            return workspace;
        }

        public static ErrorTask AddTask(this ErrorListProvider errorListProvider, UnsupportedNodeInfo node)
        {
            if (errorListProvider == null)
                throw new ArgumentNullException(nameof(errorListProvider));
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var task = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Error,
                CanDelete = true,
                Category = TaskCategory.Misc,
                Text = node.Info,
                Line = node.Location.GetLineSpan().StartLinePosition.Line,
                Column = node.Location.GetLineSpan().StartLinePosition.Character,
                Document = node.Location.GetLineSpan().Path,
            };

            task.Navigate += (sender, e) =>
            {
                //there are two Bugs in the errorListProvider.Navigate method:
                //    Line number needs adjusting
                //    Column is not shown
                task.Line++;
                errorListProvider.Navigate(task, new Guid(EnvDTE.Constants.vsViewKindCode));
                task.Line--;
            };

            errorListProvider.Tasks.Add(task);

            return task;
        }
    }
}
