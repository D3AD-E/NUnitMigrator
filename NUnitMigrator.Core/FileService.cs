using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace NUnitMigrator.Core
{
    internal static class FileService
    {
        static public async Task<Solution> OpenSolutionAsync(string path)
        {
            var workspace = MSBuildWorkspace.Create();

            var sln = await workspace.OpenSolutionAsync(path);

            return sln;
        }
    }
}
