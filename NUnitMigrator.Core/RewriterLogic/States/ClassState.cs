using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnitMigrator.Core.RewriterLogic.RewriterStates;

namespace NUnitMigrator.Core.RewriterLogic
{
    public class ClassState : IClearable
    {
        public readonly List<AttributeSyntax> RemovedAttributes;
        public ClassState()
        {
            RemovedAttributes = new List<AttributeSyntax>();
        }

        public void Clear()
        {
            RemovedAttributes.Clear();
        }
    }
}
