using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnitMigrator.Core.RewriterLogic.ExceptionSyntaxData;

namespace NUnitMigrator.Core.RewriterLogic
{
    internal static class ExceptionParser
    {
        public static bool TryGetException(SyntaxNode node, ExceptionSyntaxData details)
        {
            if (node is null)
                return false;

            CollectExceptionData(node, details, true);

            if (ExceptionTypeOfCheck(node, details))
                return true;

            foreach (var checkNode in node.ChildNodes())
            {
                if (TryGetException(checkNode, details))
                    return true;
            }

            details.Clear();
            return false;
        }

        private static bool ExceptionTypeOfCheck(SyntaxNode node, ExceptionSyntaxData details)
        {
            if (node.GetExpression() is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression.ToString().Equals("Throws") && !(memberAccess.Name is GenericNameSyntax))
                {
                    if(memberAccess.Parent is MemberAccessExpressionSyntax parent)
                    {
                        var parentName = parent.Name?.ToString();
                        if (parentName != null && (parentName.StartsWith("TypeOf<") || parentName.StartsWith("InstanceOf<")))
                        {
                            return false;
                        }
                    }
                    details.TypeName = memberAccess.Name?.ToString();
                    return true;
                }
            }

            return false;
        }

        private static void CollectExceptionData(SyntaxNode node, ExceptionSyntaxData details, bool isGenericException)
        {
            if (node.GetExpression() is MemberAccessExpressionSyntax memberAccess)
            {
                string memberName = memberAccess.Name?.ToString();
                switch (memberName)
                {
                    case "Contains":
                        details.Match = MatchType.Contains;
                        details.MatchArguments = memberAccess.GetParentInvocationArguments(details, 1);
                        break;
                    case "EqualTo":
                        details.Match = MatchType.EqualTo;
                        details.MatchArguments = memberAccess.GetParentInvocationArguments(details, 1);
                        break;
                    case "StartsWith":
                    case "StartWith":
                        details.Match = MatchType.StartsWith;
                        details.MatchArguments = memberAccess.GetParentInvocationArguments(details, 1);
                        break;
                    case "EndsWith":
                    case "EndWith":
                        details.Match = MatchType.EndsWith;
                        details.MatchArguments = memberAccess.GetParentInvocationArguments(details, 1);
                        break;
                    case "Matches":
                    case "Match":
                        details.Match = MatchType.Matches;
                        details.MatchArguments = memberAccess.GetParentInvocationArguments(details, 1);
                        break;

                    case "Message":
                        if (details.Match != MatchType.None)
                            details.MatchTarget = memberName;
                        else
                            details.Supported = false;
                        break;

                    case "Property":
                        if (details.Match != MatchType.None)
                        {
                            details.MatchTarget = memberName;
                            if (memberAccess?.Parent is InvocationExpressionSyntax invocation && invocation.ArgumentList?.Arguments.Count == 1)
                            {
                                var result = new SeparatedSyntaxList<ArgumentSyntax>();
                                for (int i = 0; i < invocation.ArgumentList.Arguments.Count; i++)
                                {
                                    string str = invocation.ArgumentList.Arguments[i].Expression?.GetLiteralString();
                                    if (str != null)
                                    {
                                        var transformed = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(str));
                                        result = result.Add(transformed);
                                    }
                                }
                                details.MatchTargetArguments = SyntaxFactory.ArgumentList(result);
                            }
                        }
                        else
                        {
                            details.Supported = false;
                        }

                        break;

                    case "With":
                        if (details.MatchTarget == null || details.Match == MatchType.None)
                            details.Supported = false;
                        break;

                    default:
                        if (isGenericException && (memberName != null && !(memberName.EndsWith("Exception"))))
                            details.Supported = false;
                        break;
                }
            }
        }

        public static bool TryGetExceptionDetails(SyntaxNode node, string exceptionMethod, ExceptionSyntaxData details)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (exceptionMethod == null)
                throw new ArgumentNullException(nameof(exceptionMethod));

            CollectExceptionData(node, details, false);

            if (TryGetTypeName(node, exceptionMethod, details))
                return true;

            foreach (var checkNode in node.ChildNodes())
            {
                if (TryGetExceptionDetails(checkNode, exceptionMethod, details))
                    return true;
            }

            details.Clear();
            return false;
        }

        private static bool TryGetTypeName(SyntaxNode node, string exceptionMethod, ExceptionSyntaxData details)
        {
            if (node.GetExpression() is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression.ToString().Equals("Throws") ||
                    memberAccess.Expression.ToString().Equals("Throws.Exception"))
                {
                    if (memberAccess.Name is GenericNameSyntax genericName &&
                        exceptionMethod.Equals(genericName.Identifier.ToString()) &&
                        genericName.TypeArgumentList?.Arguments.Count == 1)
                    {
                        details.TypeName = genericName.TypeArgumentList.Arguments[0].ToString();
                        return true;
                    }
                }
            }

            details.TypeName = null;
            return false;
        }
    }
}
