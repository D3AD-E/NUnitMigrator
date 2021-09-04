using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnitMigrator.Core.Rewriter.RewriterStates;

namespace NUnitMigrator.Core.Rewriter
{
    public class MethodState : IClearable
    {
        public readonly UiCultureExpressionsState CultureState;
        public readonly AuthorState AuthorState;
        public readonly ValuesRangeState ValuesRangeState;
        //or syntaxlist?
        public readonly List<AttributeSyntax> AddedAttributes;
        public readonly List<AttributeSyntax> RemovedAttributes;
        public MethodDeclarationSyntax CurrentMethod;
        public bool NeedsStaticModifier;

        public MethodState()
        {
            AddedAttributes = new List<AttributeSyntax>();
            RemovedAttributes = new List<AttributeSyntax>();
            CultureState = new UiCultureExpressionsState();
            AuthorState = new AuthorState();
            ValuesRangeState = new ValuesRangeState();
            NeedsStaticModifier = false;
        }

        public void Clear()
        {
            AddedAttributes.Clear();
            RemovedAttributes.Clear();
            CurrentMethod = null;
            CultureState.Clear();
            AuthorState.Clear();
            ValuesRangeState.Clear();
            NeedsStaticModifier = false;
        }
    }
}
