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
    internal class MSTestSyntaxFactory
    {
        public static InvocationExpressionSyntax ThrowsExceptionSyntax(
            ExpressionSyntax expression,
            ExceptionSyntaxData details,
            SeparatedSyntaxList<ArgumentSyntax>? additionalArguments = null)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));
            if (!details.Supported)
                throw new InvalidOperationException();

            if (details.Match == MatchType.None)
            {
                return ThrowsExceptionNaked(expression, details, additionalArguments);
            }

            return ThrowsExceptionWithMatch(expression, details, additionalArguments);
        }

        public static InvocationExpressionSyntax ThrowsExceptionNaked(
            ExpressionSyntax expression,
            ExceptionSyntaxData details,
            SeparatedSyntaxList<ArgumentSyntax>? additionalArguments = null)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));
            if (!details.Supported)
                throw new InvalidOperationException();

            // Assert.ThrowsException<<ExceptionType>>(() => /* whatever */));

            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Assert"),
                        SyntaxFactory.GenericName(SyntaxFactory.Identifier("ThrowsException"))
                            .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        SyntaxFactory.IdentifierName(details.TypeName))))))
                .WithArgumentList(
                    BuildArgumentList(expression, additionalArguments));
        }


        private static InvocationExpressionSyntax AssertOperation(string type, string method)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(type),
                    SyntaxFactory.IdentifierName(method)));
        }

        public static InvocationExpressionSyntax ThrowsExceptionWithMatch(
            ExpressionSyntax expression, ExceptionSyntaxData details,
            SeparatedSyntaxList<ArgumentSyntax>? additionalArguments = null)
        {
            string type = "StringAssert";
            string method;
            var matchTypeArgument = details.MatchArguments.Arguments[0];
            switch (details.Match)
            {
                case MatchType.Matches:
                    method = "Matches";
                    matchTypeArgument = SyntaxFactory.Argument(
                        CreateObjectInstance(typeof(System.Text.RegularExpressions.Regex).FullName,
                            details.MatchArguments.Arguments[0])).NormalizeWhitespace();
                    break;
                case MatchType.EqualTo:
                    type = "Assert";
                    method = "AreEqual";
                    break;
                case MatchType.Contains:
                    method = "Contains";
                    break;
                case MatchType.StartsWith:
                    method = "StartsWith";
                    break;
                case MatchType.EndsWith:
                    method = "EndsWith";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var matchTargetArgument = SyntaxFactory.IdentifierName(details.MatchTarget);
            switch (details.MatchTarget)
            {
                case "Property":
                    matchTargetArgument = SyntaxFactory.IdentifierName(
                        details.MatchTargetArguments.Arguments[0].Expression.ToString());
                    break;
            }

            var argumentList = new SeparatedSyntaxList<ArgumentSyntax>();
            argumentList = argumentList.Add(
                SyntaxFactory.Argument(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThrowsExceptionNaked(expression, details, additionalArguments),
                        matchTargetArgument
                    )));
            argumentList = argumentList.Add(matchTypeArgument);

            return AssertOperation(type, method)
                .WithArgumentList(SyntaxFactory.ArgumentList(argumentList));
        }

        private static ArgumentListSyntax BuildArgumentList(
            ExpressionSyntax expression,
            SeparatedSyntaxList<ArgumentSyntax>? additionalArguments)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            ArgumentListSyntax argumentList;
            if (additionalArguments.HasValue && additionalArguments.Value.Any())
            {
                var args = new SeparatedSyntaxList<ArgumentSyntax>();
                args = args.Add(SyntaxFactory.Argument(expression));
                args = args.AddRange(additionalArguments.Value);
                argumentList = SyntaxFactory.ArgumentList(args);
            }
            else
            {
                argumentList = SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(expression)));
            }

            return argumentList;
        }

        public static ExpressionSyntax CreateObjectInstance(string identifierName, params ArgumentSyntax[] arguments)
        {
            var result = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(identifierName));

            if (arguments != null && arguments.Length > 0)
            {
                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.AddRange(arguments);
                result = result.WithArgumentList(SyntaxFactory.ArgumentList(argList));
            }

            return result;
        }

        public static AttributeSyntax CreateAttribute(string name, ExpressionSyntax expression)
        {
            SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
            argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(expression));
            AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
            AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(name), argumentListSyntax);
            return newAttribute;
        }

        public static AttributeSyntax CreateAttribute(string name, List<ExpressionSyntax> expressions)
        {
            SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
            foreach (var expression in expressions)
            {
                argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(expression));
            }
            AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
            AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(name), argumentListSyntax);
            return newAttribute;
        }

        //does not look good
        public static InvocationExpressionSyntax CreateComparisonExpression(InvocationExpressionSyntax node,
            MemberAccessExpressionSyntax memberAccess, SyntaxKind compareOperator,
            ExpressionSyntax arg0, ExpressionSyntax arg1, int initialArgAmount, bool isTrue = true)
        {
            var argList = new SeparatedSyntaxList<ArgumentSyntax>();
            var binaryExpression = SyntaxFactory.BinaryExpression(compareOperator, arg0, arg1);
            argList = argList.Add(SyntaxFactory.Argument(binaryExpression).NormalizeWhitespace());
            var remainingArguments = node.ArgumentList.Arguments.Skip(initialArgAmount);
            if (remainingArguments.Any())
            {
                argList = argList.AddRange(remainingArguments);
            }
            memberAccess = isTrue ? memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue")) :
                memberAccess.WithName(SyntaxFactory.IdentifierName("IsFalse"));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            return node;
        }
    }
}
