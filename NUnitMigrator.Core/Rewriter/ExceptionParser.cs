using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NUnitMigrator.Core.Rewriter.ExceptionSyntaxData;

namespace NUnitMigrator.Core.Rewriter
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
                        {
                            details.MatchTarget = memberName;
                        }
                        else
                        {
                            details.Supported = false;
                        }

                        break;

                    case "Property":
                        if (details.Match != MatchType.None)
                        {
                            details.MatchTarget = memberName;
                            details.MatchTargetArguments = memberAccess.TransformParentInvocationArguments(details, 1,
                                (arg, i) =>
                                {
                                    string str = arg.Expression?.GetLiteralString();
                                    if (str != null)
                                        return SyntaxFactory.Argument(SyntaxFactory.IdentifierName(str));
                                    return null;
                                });
                        }
                        else
                        {
                            details.Supported = false;
                        }

                        break;

                    case "With":
                        if (details.MatchTarget == null ||
                            details.Match == MatchType.None)
                        {
                            details.Supported = false;
                        }

                        break;

                    default:
                        if (isGenericException)
                        {
                            if (memberName != null && !(memberName.EndsWith("Exception")))
                            {
                                details.Supported = false;
                            }
                        }
                        else
                        {
                            if (memberName != null && !(
                                    memberName.Equals("Throws") ||
                                    memberName.StartsWith("TypeOf<") ||
                                    memberName.StartsWith("InstanceOf<") ||
                                    memberName.Equals("Exception")))
                            {
                                details.Supported = false;
                            }
                        }

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
