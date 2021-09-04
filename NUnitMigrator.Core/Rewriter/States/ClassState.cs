using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnitMigrator.Core.Rewriter.RewriterStates;

namespace NUnitMigrator.Core.Rewriter
{
    public class ClassState : IClearable
    {
        public readonly List<AttributeSyntax> RemovedAttributes;
        public bool IsClassAtrributeNeeded;
        public ClassState()
        {
            RemovedAttributes = new List<AttributeSyntax>();
            IsClassAtrributeNeeded = true;
        }

        public void Clear()
        {
            RemovedAttributes.Clear();
            IsClassAtrributeNeeded = true;
        }
    }
}
