using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.RewriterLogic
{
    static class RewriterExtentions
    {
        public static ClassDeclarationSyntax AddAttributeWithName(this ClassDeclarationSyntax node, string attributeName)
        {
            var existing = node.AttributeLists;
            var list = new SeparatedSyntaxList<AttributeSyntax>();
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName));
            list = list.Add(attribute);
            if (existing.Count>0)
            {
                var last = existing.Last();
                var newList = SyntaxFactory.AttributeList(list)
                   .WithLeadingTrivia(last.GetClosestWhitespaceTrivia(true))
                   .WithTrailingTrivia(last.GetClosestWhitespaceTrivia(false));

                existing = existing.Add(newList);
                node = node.WithAttributeLists(existing);
            }
            else
            {
                var newList = SyntaxFactory.AttributeList(list).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                var newLists = new SyntaxList<AttributeListSyntax>();
                newLists = newLists.Add(newList);
                node = node.WithAttributeLists(newLists);
            }
            return node;
        }
        public static MethodDeclarationSyntax AddAttribute(this MethodDeclarationSyntax node, AttributeSyntax attribute)
        {
            var existing = node.AttributeLists;
            var list = new SeparatedSyntaxList<AttributeSyntax>();
            list = list.Add(attribute);
            if (existing.Count > 0)
            {
                var last = existing.Last();
                var newList = SyntaxFactory.AttributeList(list)
                   .WithLeadingTrivia(last.GetClosestWhitespaceTrivia(true))
                   .WithTrailingTrivia(last.GetClosestWhitespaceTrivia(false));

                existing = existing.Add(newList);
                node = node.WithAttributeLists(existing);
            }
            else
            {
                var newList = SyntaxFactory.AttributeList(list).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                var newLists = new SyntaxList<AttributeListSyntax>();
                newLists = newLists.Add(newList);
                node = node.WithAttributeLists(newLists);
            }
            return node;
        }

        public static MethodDeclarationSyntax AddAttributes(this MethodDeclarationSyntax node, List<AttributeSyntax> attributes)
        {
            var existing = node.AttributeLists;
            var list = new SeparatedSyntaxList<AttributeSyntax>();
            list = list.AddRange(attributes);
            if (existing.Count > 0)
            {
                var last = existing.Last();
                var newList = SyntaxFactory.AttributeList(list)
                   .WithLeadingTrivia(last.GetClosestWhitespaceTrivia(true))
                   .WithTrailingTrivia(last.GetClosestWhitespaceTrivia(false));

                existing = existing.Add(newList);
                node = node.WithAttributeLists(existing);
            }
            else
            {
                var newList = SyntaxFactory.AttributeList(list).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                var newLists = new SyntaxList<AttributeListSyntax>();
                newLists = newLists.Add(newList);
                node = node.WithAttributeLists(newLists);
            }
            return node;
        }

        public static InvocationExpressionSyntax ChangeName(this InvocationExpressionSyntax node, string name)
        {
            if(node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var trivia = memberAccess.GetLeadingTrivia();

                memberAccess = memberAccess.WithExpression(SyntaxFactory.IdentifierName(name))
                    .WithLeadingTrivia(trivia);
                node = node.WithExpression(memberAccess);
            }
            return node;
        }

        private static SyntaxList<AttributeListSyntax> RemoveAttribute(SyntaxList<AttributeListSyntax> nodeAttributeLists, AttributeSyntax attribute, bool byName)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();
            foreach (var attrList in nodeAttributeLists)
            {
                AttributeListSyntax newAttribute;
                if (byName)
                {
                    var toBeRemoved = attrList.Attributes.SingleOrDefault(x => x.Name.ToString() == attribute.Name.ToString());
                    if (toBeRemoved is null)
                        newAttribute = attrList;
                    else
                        newAttribute = attrList.RemoveNode(toBeRemoved, SyntaxRemoveOptions.KeepNoTrivia);
                }
                else
                {
                    newAttribute = attrList.RemoveNode(attribute, SyntaxRemoveOptions.KeepNoTrivia);
                }

                if (newAttribute.Attributes.Count != 0)
                    newAttributes = newAttributes.Add(newAttribute);
            }
            return newAttributes;
        }

        public static ClassDeclarationSyntax RemoveAttribute(this ClassDeclarationSyntax node, AttributeSyntax attribute, bool byName = false)
        {
            var newAttributes = RemoveAttribute(node.AttributeLists, attribute, byName);
            
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }
        public static MethodDeclarationSyntax RemoveAttribute(this MethodDeclarationSyntax node, AttributeSyntax attribute, bool byName = false)
        {
            var newAttributes = RemoveAttribute(node.AttributeLists, attribute, byName);

            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        //legacy method, in theory should be divided into 2 different ones
        public static MethodDeclarationSyntax AddParametrization(this MethodDeclarationSyntax node, List<AttributeSyntax> parametrizedAttributes) // Range and Values attributes
        {
            AttributeSyntax attributeValues = null;
            foreach (var pa in parametrizedAttributes)
            {
                if (attributeValues == null && pa.Name.ToString().Equals(RewriterData.NUnitData.VALUES_ATTRIBUTE))
                    attributeValues = pa;
            }
            if (attributeValues != null && attributeValues.ArgumentList!=null)
            {
                for (int i = 0; i < attributeValues.ArgumentList.Arguments.Count(); i++)
                {
                    SeparatedSyntaxList<AttributeSyntax> attributes = new SeparatedSyntaxList<AttributeSyntax>();
                    SeparatedSyntaxList<AttributeArgumentSyntax> attributeArguments = new SeparatedSyntaxList<AttributeArgumentSyntax>();

                    foreach (var attr in parametrizedAttributes)
                    {
                        if (attr.Name.ToString().Equals(RewriterData.NUnitData.VALUES_ATTRIBUTE))
                        {
                            try
                            {
                                attributeArguments = attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(attr.ArgumentList.Arguments.ElementAt(i).ToFullString())));
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else //Range attribute
                        {
                            if (attr.ArgumentList.Arguments.Count == 3)
                            {
                                List<AttributeArgumentSyntax> ArgumentList = attr.ArgumentList.Arguments.ToList();
                                try
                                {
                                    double from = Convert.ToDouble(ArgumentList.ElementAt(0).Expression.ToString());
                                    double to = Convert.ToDouble(ArgumentList.ElementAt(1).Expression.ToString());
                                    double step = Convert.ToDouble(ArgumentList.ElementAt(2).Expression.ToString());
                                    double argument = from + step * i;
                                    argument = argument <= to ? argument : to;
                                    attributeArguments = attributeArguments.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression(argument.ToString())));
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                    AttributeArgumentListSyntax attributeArgumentList = SyntaxFactory.AttributeArgumentList(attributeArguments);
                    AttributeSyntax attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("DataRow"), attributeArgumentList);

                    attributes = attributes.Add(attribute);
                    AttributeListSyntax attributeList = SyntaxFactory.AttributeList(attributes);
                    node = node.AddAttributeLists(attributeList);
                }
            }
            return node;
        }

        public static MethodDeclarationSyntax AddComment(this MethodDeclarationSyntax node, string comment)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            comment = comment.StartsWith("//") ? comment : "//" + comment;
            leadingTrivia = leadingTrivia.Add(leadingTrivia[0]);
            leadingTrivia = leadingTrivia.Insert(1, SyntaxFactory.Comment(comment));
            leadingTrivia = leadingTrivia.Insert(2, SyntaxFactory.CarriageReturnLineFeed);
            node = node.WithLeadingTrivia(leadingTrivia);

            return node;
        }
        public static MethodDeclarationSyntax AddExpression(this MethodDeclarationSyntax node, string expression)
        {
            var statements = node.Body.Statements.ToList();
            var originalTrailingTrivia = node.Body.GetTrailingTrivia();
            var originalLeadingTrivia = node.Body.GetLeadingTrivia();
            var newStatement = SyntaxFactory.ParseStatement(expression);

            if (statements.Count > 0 && statements[0].HasLeadingTrivia)
            {
                var trivia = statements[0].GetLeadingTrivia();
                trivia = trivia.Insert(0, SyntaxFactory.CarriageReturnLineFeed);
                newStatement = newStatement.WithLeadingTrivia(trivia);
            }
            else
            {
                //tab fails an assert but should be better than 4 whitespaces
                newStatement = newStatement.WithLeadingTrivia(new [] { SyntaxFactory.CarriageReturnLineFeed, 
                    originalLeadingTrivia[0], 
                    SyntaxFactory.Whitespace("    ") });
            }
            newStatement = newStatement.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            statements.Insert(0, newStatement);

            //strange and bad, but it does not seem to work otherwise, last tab before last bracket is lost
            var lastStatement = statements.Last();
            statements[statements.Count - 1] = lastStatement.WithTrailingTrivia(lastStatement.GetTrailingTrivia().Insert(1, originalLeadingTrivia[0]));

            var newBody = SyntaxFactory.Block(statements);

            newBody = newBody.WithTrailingTrivia(originalTrailingTrivia);
            newBody = newBody.WithLeadingTrivia(originalLeadingTrivia);

            node = node.WithBody(newBody);

            return node;
        }

        public static CSharpSyntaxNode CommentMultiline(this CSharpSyntaxNode node)
        {
            var originalTrailingTrivia = node.GetTrailingTrivia();
            originalTrailingTrivia = originalTrailingTrivia.Insert(0, SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "*/"));
            var originalLeadingTrivia = node.GetLeadingTrivia();
            originalLeadingTrivia = originalLeadingTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.MultiLineCommentTrivia, "/*"));
            node = node.WithTrailingTrivia(originalTrailingTrivia).WithLeadingTrivia(originalLeadingTrivia);
            return node;
        }

        public static AttributeListSyntax Comment(this AttributeListSyntax node)
        {
            var originalLeadingTrivia = node.GetLeadingTrivia();
            originalLeadingTrivia = originalLeadingTrivia.Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "//"));
            node = node.WithLeadingTrivia(originalLeadingTrivia);
            return node;
        }

        public static SyntaxTriviaList GetClosestWhitespaceTrivia(this SyntaxNode node, bool leading)
        {
            var list = leading ? node.GetLeadingTrivia() : node.GetTrailingTrivia();
            if (list.Count > 0)
            {
                var lastTrivia = list.Last();
                switch ((SyntaxKind)lastTrivia.RawKind)
                {
                    case SyntaxKind.WhitespaceTrivia:
                        return SyntaxTriviaList.Create(lastTrivia);
                    case SyntaxKind.EndOfLineTrivia:
                        return SyntaxTriviaList.Create(lastTrivia);
                }
            }

            return SyntaxTriviaList.Empty;
        }

        public static string GetLiteralString(this ExpressionSyntax arg)
        {
            if (arg is LiteralExpressionSyntax)
            {
                string str = arg.ToString().Trim('"');
                if (SyntaxFacts.IsValidIdentifier(str))
                {
                    return str;
                }
            }

            if (arg is InvocationExpressionSyntax invocation &&
                invocation.Expression.ToString().Equals("nameof"))
            {
                var n = SyntaxFactory.ParseName(invocation.ArgumentList.Arguments[0].ToString());
                if (n is QualifiedNameSyntax qn)
                {
                    return qn.Right.ToString();
                }
                if (n is IdentifierNameSyntax iname)
                {
                    return iname.Identifier.ToString();
                }
            }

            return null;
        }

        public static bool TypeSymbolMatchesType(this SemanticModel semanticModel, ITypeSymbol typeSymbol, Type type)
        {
            return SymbolEqualityComparer.Default.Equals(GetTypeSymbolForType(type, semanticModel), typeSymbol);
        }

        private static INamedTypeSymbol GetTypeSymbolForType(Type type, SemanticModel semanticModel)
        {
            if (!type.IsConstructedGenericType)
            {
                return semanticModel.Compilation.GetTypeByMetadataName(type.FullName);
            }

            var typeArgumentsTypeInfos = type.GenericTypeArguments.Select(a => GetTypeSymbolForType(a, semanticModel));

            var openType = type.GetGenericTypeDefinition();
            var typeSymbol = semanticModel.Compilation.GetTypeByMetadataName(openType.FullName);
            return typeSymbol.Construct(typeArgumentsTypeInfos.ToArray<ITypeSymbol>());
        }

        public static ExpressionSyntax GetExpression(this SyntaxNode node)
        {
            if (node is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Expression;
            if (node is InvocationExpressionSyntax invocation)
                return invocation.Expression;
            if (node is ArgumentSyntax argument)
                return argument.Expression;

            return null;
        }

        public static bool HasBooleanResult(this SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(expression);
                if (typeInfo.ConvertedType?.SpecialType == SpecialType.System_Boolean)
                    return true;
            }
            return false;
        }

        public static ISymbol FindSymbol<T>(this Compilation compilation, Func<ISymbol, bool> predicate)
            where T : SyntaxNode
        {
            return compilation.SyntaxTrees
                .Select(x => compilation.GetSemanticModel(x))
                .SelectMany(
                    x => x.SyntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .OfType<T>()
                        .Select(y => x.GetDeclaredSymbol(y)))
                .FirstOrDefault(x => predicate(x));
        }

        public static ArgumentListSyntax GetParentInvocationArguments(
            this MemberAccessExpressionSyntax memberAccess,
            ExceptionSyntaxData details, int numArgumentsRequired,
            Func<ArgumentSyntax, int, bool> check = null)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            if (memberAccess?.Parent is InvocationExpressionSyntax invocation &&
                invocation.ArgumentList?.Arguments.Count == numArgumentsRequired)
            {
                for (int i = 0; i < invocation.ArgumentList.Arguments.Count; i++)
                {
                    if (check != null && !check(invocation.ArgumentList.Arguments[i], i))
                    {
                        details.Supported = false;
                        return null;
                    }
                }

                return invocation.ArgumentList;
            }

            details.Supported = false;
            return null;
        }
    }
}
