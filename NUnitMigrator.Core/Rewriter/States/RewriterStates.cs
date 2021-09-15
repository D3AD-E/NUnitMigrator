using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.Rewriter
{
    public class RewriterStates
    {
        public interface IClearable
        {
            void Clear();
        }

        public class UiCultureExpressionsState : IClearable
        {
            public string Expression { get; set; }
            public bool IsExpressionNeeded { get; set; }
            public string UIExpression { get; set; }
            public bool IsUIExpressionNeeded { get; set; }

            public UiCultureExpressionsState()
            {
                Clear();
            }
            public void Clear()
            {
                Expression = string.Empty;
                IsExpressionNeeded = false;
                UIExpression = string.Empty;
                IsUIExpressionNeeded = false;
            }
        }

        public class AuthorState : IClearable
        {
            public string Email { get; set; }
            public bool IsEmailNeeded { get; set; }

            public AuthorState()
            {
                Clear();
            }
            public void Clear()
            {
                Email = string.Empty;
                IsEmailNeeded = false;
            }
        }

        public class ValuesRangeState : IClearable
        {
            public readonly List<AttributeSyntax> Attributes;
            public bool IsPropertyNeeded { get; set; }

            public void Clear()
            {
                Attributes.Clear();
                IsPropertyNeeded = false;
            }

            public ValuesRangeState()
            {
                Attributes = new List<AttributeSyntax>();
                IsPropertyNeeded = false;
            }
        }
    }
}
