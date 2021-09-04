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
            public string Expression;
            public bool IsExpressionNeeded = false;
            public string UIExpression;
            public bool IsUIExpressionNeeded = false;

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
            public string Email;
            public bool IsEmailNeeded = false;

            public void Clear()
            {
                Email = string.Empty;
                IsEmailNeeded = false;
            }
        }

        public class ValuesRangeState : IClearable
        {
            public List<AttributeSyntax> Attributes;
            public bool IsPropertyNeeded = false;

            public void Clear()
            {
                Attributes.Clear();
                IsPropertyNeeded = false;
            }

            public ValuesRangeState()
            {
                Attributes = new List<AttributeSyntax>();
            }
        }
    }
}
