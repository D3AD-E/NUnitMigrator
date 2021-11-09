using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnitMigrator.Core.RewriterLogic.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnitMigrator.Core.RewriterLogic
{
    public class Rewriter : CSharpSyntaxRewriter
    {
        public readonly List<UnsupportedNodeInfo> Unsupported;
        public int ChangesAmount;
        private readonly SemanticModel _semanticModel;

        private readonly MethodState _methodState;
        private readonly ClassState _classState;
        private RewriterOptions _options;

        public Rewriter(SemanticModel model) : base()
        {
            _semanticModel = model;
            _methodState = new MethodState();
            _classState = new ClassState();
            _options = new RewriterOptions();
            Unsupported = new List<UnsupportedNodeInfo>();
            ChangesAmount = 0;
        }

        public void SetOptions(RewriterOptions options)
        {
            _options = options;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _classState.Clear();
            node = base.VisitClassDeclaration(node) as ClassDeclarationSyntax;
            if (node != null)
            {
                if (_classState.RemovedAttributes.Count > 0)
                {
                    foreach (var attribute in _classState.RemovedAttributes)
                    {
                        node = node.RemoveAttribute(attribute, true);
                    }
                    ChangesAmount++;
                }
            }
            return node;
        }
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _methodState.Clear();
            _methodState.CurrentMethod = node;
            int unsupportedCount = Unsupported.Count();
            node = base.VisitMethodDeclaration(node) as MethodDeclarationSyntax;
            if (node != null)
            {
                if (_methodState.AddedAttributes.Count > 0)
                    node = node.AddAttributes(_methodState.AddedAttributes);
                if (_methodState.RemovedAttributes.Count > 0)
                {
                    foreach (var attribute in _methodState.RemovedAttributes)
                    {
                        node = node.RemoveAttribute(attribute, true);
                    }
                }
                if(_methodState.AuthorState.IsEmailNeeded)
                    node = node.AddComment(_methodState.AuthorState.Email);
                if(_methodState.CultureState.IsExpressionNeeded)
                    node = node.AddExpression(_methodState.CultureState.Expression);
                if (_methodState.CultureState.IsUIExpressionNeeded)
                    node = node.AddExpression(_methodState.CultureState.UIExpression);
                if (_methodState.NeedsStaticModifier)
                    node = node.AddModifiers(SyntaxFactory.ParseToken("static "));
                if (_methodState.ValuesRangeState.IsPropertyNeeded)
                    node = node.AddParametrization(_methodState.ValuesRangeState.Attributes);

                if (_options.CommentUnsupported && unsupportedCount < Unsupported.Count)
                    node = node.Comment();
            }
            return node;
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax attribute)
        {
            attribute = (AttributeSyntax)base.VisitAttribute(attribute);
            var prevAttribute = attribute;
            attribute = HandleAttribute(attribute);
            if(prevAttribute != attribute)
                ChangesAmount++;
            return attribute;
        }
       
        private AttributeSyntax HandleAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            switch (name)
            {
                case RewriterData.NUnitData.TEST_FIXTURE_ATTRIBUTE:
                    if (attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count == 0)
                        attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_CLASS_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.NON_PARALLELIZABLE_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.DO_NOT_PARRELELIZE_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.LOP_ATTRIBUTE:
                case RewriterData.NUnitData.IGNORE_ATTRIBUTE:
                    break;
                case RewriterData.NUnitData.DESCRIPTION_ATTRIBUTE:
                     if (attribute.Parent?.Parent is ClassDeclarationSyntax)
                        _classState.RemovedAttributes.Add(attribute);
                    break;
                case RewriterData.NUnitData.CATEGORY_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_CATEGORY_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.EXPLICIT_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.IGNORE_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.MAX_TIME_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TIMEOUT_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.PROPERTY_ATTRIBUTE:
                    if (attribute.Parent?.Parent is MethodDeclarationSyntax)
                        attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_PROPETY_ATTRIBUTE));
                    else if (attribute.Parent?.Parent is ClassDeclarationSyntax)
                        _classState.RemovedAttributes.Add(attribute);
                    break;
                case RewriterData.NUnitData.SETUP_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_INIT_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.TEAR_DOWN_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_CLEANUP_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.TEST_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_METHOD_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.TEST_OF_ATTRIBUTE:
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.DESCRIPTION_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.TCS_ATTRIBUTE:
                case RewriterData.NUnitData.VALUES_SOURCE_ATTRIBUTE:
                    attribute = TransformTCSAttribute(attribute);
                    break;
                case RewriterData.NUnitData.AUTHOR_ATTRIBUTE:
                    if (attribute.Parent?.Parent is MethodDeclarationSyntax)
                        attribute = TransformAuthorAttribute(attribute);
                    else if(attribute.Parent?.Parent is ClassDeclarationSyntax)
                        _classState.RemovedAttributes.Add(attribute);
                    break;
                case RewriterData.NUnitData.TEST_CASE_ATTRIBUTE:
                    attribute = TransformTestCaseAttribute(attribute);
                    break;
                case RewriterData.NUnitData.OTSU_ATTRIBUTE:
                case RewriterData.NUnitData.TFSE_ATTRIBUTE:
                    if (attribute.Parent?.Parent is MethodDeclarationSyntax)
                    {
                        _methodState.NeedsStaticModifier = !_methodState.CurrentMethod.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);
                        attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.CLASS_INIT_ATTRIBUTE));
                    }
                    break;
                case RewriterData.NUnitData.OTTD_ATTRIBUTE:
                case RewriterData.NUnitData.TFT_ATTRIBUTE:
                    _methodState.NeedsStaticModifier = !_methodState.CurrentMethod.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.CLASS_CLEANUP_ATTRIBUTE));
                    break;
                case RewriterData.NUnitData.APARTMENT_ATTRIBUTE:
                case RewriterData.NUnitData.REPEAT_ATTRIBUTE:
                case RewriterData.NUnitData.RETRY_ATTRIBUTE:
                case RewriterData.NUnitData.NTA_ATTRIBUTE:
                case RewriterData.NUnitData.COMBINATORIAL_ATTRIBUTE:
                case RewriterData.NUnitData.PAIRWISE_ATTRIBUTE:
                case RewriterData.NUnitData.PARALLELIZABLE_ATTRIBUTE:
                case RewriterData.NUnitData.SEQUENTIAL_ATTRIBUTE:
                    _methodState.RemovedAttributes.Add(attribute);
                    break;
                case RewriterData.NUnitData.VALUES_ATTRIBUTE:
                case RewriterData.NUnitData.RANGE_ATTRIBUTE:
                    _methodState.ValuesRangeState.IsPropertyNeeded = true;
                    _methodState.ValuesRangeState.Attributes.Add(attribute);
                    break;
                case RewriterData.NUnitData.SET_CULTURE_ATTRIBUTE:
                    _methodState.RemovedAttributes.Add(attribute);
                    _methodState.CultureState.IsExpressionNeeded = true;
                    _methodState.CultureState.Expression = "System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture" + attribute.ArgumentList.ToFullString() + ";";
                    break;
                case RewriterData.NUnitData.SET_UI_CULTURE_ATTRIBUTE:
                    _methodState.RemovedAttributes.Add(attribute);
                    _methodState.CultureState.IsUIExpressionNeeded = true;
                    _methodState.CultureState.UIExpression = "System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture" + attribute.ArgumentList.ToFullString() + ";";
                    break;
                case RewriterData.NUnitData.DFPT_ATTRIBUTE:
                case RewriterData.NUnitData.CULTURE_ATTRIBUTE:
                case RewriterData.NUnitData.DATAPOINT_ATTRIBUTE:
                case RewriterData.NUnitData.DATAPOINT_SOURCE_ATTRIBUTE:
                case RewriterData.NUnitData.ORDER_ATTRIBUTE:
                case RewriterData.NUnitData.PLATFORM_ATTRIBUTE:
                case RewriterData.NUnitData.REQUIRES_THREAD_ATTRIBUTE:
                case RewriterData.NUnitData.SINGLE_THREADED_ATTRIBUTE:
                case RewriterData.NUnitData.THEORY_ATTRIBUTE:
                case RewriterData.NUnitData.TIMEOUT_ATTRIBUTE:
                case RewriterData.NUnitData.RANDOM_ATTRIBUTE:
                case RewriterData.NUnitData.SETUP_FIXTURE_ATTRIBUTE:
                default:
                    {
                        var symbol = _semanticModel.GetSymbolInfo(attribute.Name);

                        var symbolType = symbol.Symbol?.ContainingType?.ToDisplayString();
                        if (symbolType != null && symbolType.StartsWith("NUnit."))
                        {
                            Unsupported.Add(new UnsupportedNodeInfo
                            {
                                Info = "Attribute is not supported",
                                Location = attribute.GetLocation(),
                                NodeName = attribute.Name.ToString()
                            });
                        }
                        break;
                    }
            }
            return attribute;
        }

        private AttributeSyntax TransformTestCaseAttribute(AttributeSyntax attribute)
        {
            attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.DATA_ROW_ATTRIBUTE));
            var savedArguments = new SeparatedSyntaxList<AttributeArgumentSyntax>();
            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                var argName = argument.NameEquals?.Name?.ToString();
                switch (argName)
                {
                    case "TestName":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("TestProperty", new List<ExpressionSyntax>
                            { 
                                SyntaxFactory.ParseExpression("\"TestName\""),
                                argument.Expression
                            }));
                        break;
                    case "Author":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("Owner", argument.Expression));
                        break;
                    case "Category":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("TestCategory", argument.Expression));
                        break;
                    case "Description":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("Description", argument.Expression));
                        break;
                    case "Explicit":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("Explicit", argument.Expression));
                        break;
                    case "Ignore":
                    case "IgnoreReason":
                    case "Reason":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("Ignore", argument.Expression));
                        break;
                    case "TestOf":
                        _methodState.AddedAttributes.Add(MSTestSyntaxFactory.CreateAttribute("Description", 
                            SyntaxFactory.ParseExpression("\"" + argument.Expression + "\"")));
                        break;
                    case "ExpectedResult":
                        break;
                    default:
                        savedArguments = savedArguments.Add(argument);
                        break;
                }
            }
            attribute = attribute.WithArgumentList(SyntaxFactory.AttributeArgumentList(savedArguments));
            return attribute;
        }

        //only such arguments are supported
        //string data
        //type a, string b
        private AttributeSyntax TransformTCSAttribute(AttributeSyntax attribute)
        {
            bool supported = false;
            string targetName = null;
            string targetType = null;
            string explicitTargetType = null;
            if (attribute.ArgumentList != null)
            {
                int argumentCount = attribute.ArgumentList.Arguments.Count;
                if (argumentCount == 1)
                {
                    //get string even with nameof
                    var arg0 = attribute.ArgumentList.Arguments[0];
                    var type0 = _semanticModel.GetTypeInfo(arg0.Expression);
                    if (type0.ConvertedType?.SpecialType == SpecialType.System_String &&
                        (targetName = arg0.Expression.GetLiteralString()) != null)
                    {
                        supported = true;
                        targetType = GetMethodContainingType(_methodState.CurrentMethod);
                    }
                }
                else if (argumentCount == 2)
                {
                    var arg0 = attribute.ArgumentList.Arguments[0];
                    var arg1 = attribute.ArgumentList.Arguments[1];
                    var type0 = _semanticModel.GetTypeInfo(arg0.Expression);
                    var type1 = _semanticModel.GetTypeInfo(arg1.Expression);
                    //if first is type and second is string
                    if (_semanticModel.TypeSymbolMatchesType(type0.ConvertedType, typeof(Type)) &&
                        arg0.Expression is TypeOfExpressionSyntax typeOfExpression &&
                        type1.ConvertedType?.SpecialType == SpecialType.System_String &&
                        (targetName = arg1.Expression.GetLiteralString()) != null)
                    {
                        targetType = _semanticModel.GetTypeInfo(typeOfExpression.Type).ConvertedType?.ToString();
                        explicitTargetType = targetType;
                        supported = targetType != null;
                    }
                }
            }

            if (!supported)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Syntax is not supported",
                    Location = attribute.GetLocation(),
                    NodeName = attribute.Name.ToString()
                });
                return attribute;
            }

            string sourceType;
            if (_semanticModel.Compilation.FindSymbol<MethodDeclarationSyntax>(
                symbol => symbol is IMethodSymbol method && method.Name == targetName && method.ContainingType.Name == targetType) != null)
            {
                sourceType = "DynamicDataSourceType.Method";
            }
            else if (_semanticModel.Compilation.FindSymbol<PropertyDeclarationSyntax>(
                symbol => symbol is IPropertySymbol method && method.Name == targetName && method.ContainingType.Name == targetType) != null)
            {
                sourceType = "DynamicDataSourceType.Property";
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Must be a method or a property",
                    Location = attribute.GetLocation(),
                    NodeName = attribute.Name.ToString()
                });
                return attribute;
            }

            var argList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
            if (explicitTargetType != null)
            {
                argList = argList.Add(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(targetName))));
                argList = argList.Add(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.TypeOfExpression(SyntaxFactory.IdentifierName(explicitTargetType))));
            }
            else
            {
                argList = argList.Add(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(targetName))));
            }

            if (sourceType != "DynamicDataSourceType.Method")
            {
                argList = argList.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseName(sourceType)));
            }

            attribute = attribute.WithName(SyntaxFactory.IdentifierName("DynamicData"));
            attribute = attribute.WithArgumentList(SyntaxFactory.AttributeArgumentList(argList).NormalizeWhitespace());

            return attribute;
        }

        private AttributeSyntax TransformAuthorAttribute(AttributeSyntax attribute)
        {
            var argumentList = attribute.ArgumentList;
            if (argumentList.Arguments.Count > 1)
            {
                var translatedAttribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.OWNER_ATTRIBUTE));
                _methodState.AuthorState.IsEmailNeeded = true;
                _methodState.AuthorState.Email = argumentList.Arguments[1].ToString();
                var newList = argumentList.RemoveNode(argumentList.Arguments[1], SyntaxRemoveOptions.KeepNoTrivia);
                return translatedAttribute.WithArgumentList(newList);
            }
            else
            {
                return attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.OWNER_ATTRIBUTE));
            }
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax attribute)
        {
            var existing = _semanticModel.GetSymbolInfo(attribute.Name);

            if (SyntaxFactory.ParseName("NUnit.Framework").ToFullString().Equals(existing.Symbol?.ToDisplayString()))
            {
                attribute = attribute.WithName(SyntaxFactory.ParseName("Microsoft.VisualStudio.TestTools.UnitTesting"));
            }

            return attribute;
        }

        private static string GetMethodContainingType(MethodDeclarationSyntax node)
        {
            do
            {
                var parent = node.Parent;
                if (parent is ClassDeclarationSyntax cSyntax)
                {
                    return cSyntax.Identifier.ToString();
                }
                if (parent is StructDeclarationSyntax sSyntax)
                {
                    return sSyntax.Identifier.ToString();
                }
            }
            while (node.Parent != null);
            return null;
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            SymbolInfo info;
            try
            {
                info = _semanticModel.GetSymbolInfo(node);
            }
            catch (ArgumentException)
            {
                return base.VisitInvocationExpression(node);
            }
            var prevNode = node;
            node = HandleInvocationExpression(node, info);
            if (prevNode != node)
                ChangesAmount++;
            node = base.VisitInvocationExpression(node) as InvocationExpressionSyntax;
            
            return node;
        }

        private InvocationExpressionSyntax HandleInvocationExpression(InvocationExpressionSyntax node, SymbolInfo info)
        {
            var frameworkInfo = info.Symbol?.ContainingType.ToDisplayString();
            if ("NUnit.Framework.Assert".Equals(frameworkInfo) &&
                node.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var memberName = memberAccess.Name?.ToString();
                if ("That".Equals(memberName) && node.ArgumentList?.Arguments.Count > 0)
                {
                    if (node.ArgumentList.Arguments.Count >= 2)
                    {
                        var secondArgument = node.ArgumentList?.Arguments[1];
                        if(secondArgument.ToString().StartsWith("Throws"))
                        {
                            var remainingArguments = new SeparatedSyntaxList<ArgumentSyntax>();
                            remainingArguments = remainingArguments.AddRange(node.ArgumentList.Arguments.Skip(2));

                            var details = new ExceptionSyntaxData();
                            if (ExceptionParser.TryGetException(secondArgument, details) ||
                                ExceptionParser.TryGetExceptionDetails(secondArgument, "TypeOf", details))
                            {
                                if (details.Supported)
                                {
                                    node = MSTestSyntaxFactory.ThrowsExceptionSyntax(node.ArgumentList.Arguments[0].Expression,
                                            details, remainingArguments)
                                        .WithLeadingTrivia(node.GetClosestWhitespaceTrivia(true));
                                    return node;
                                }
                                else
                                {
                                    Unsupported.Add(new UnsupportedNodeInfo
                                    {
                                        Info = "Unsupported exception constraint expression",
                                        Location = node.GetLocation(),
                                        NodeName = node.ToString()
                                    });
                                }
                            }
                        }
                        if (secondArgument.Expression is MemberAccessExpressionSyntax constraintMemberAccess)
                        {
                            node = TransformConstraintAssertion(node, memberAccess, constraintMemberAccess);
                        }
                        else if (secondArgument.Expression is InvocationExpressionSyntax constraintInvocationExpression
                            && constraintInvocationExpression.Expression is MemberAccessExpressionSyntax invocationConstraintMA)
                        {
                            node = TransformConstraintAssertion(node, memberAccess, invocationConstraintMA);
                        }
                        else if (_semanticModel.HasBooleanResult(node.ArgumentList.Arguments[0].Expression))
                        {
                            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
                            node = node.WithExpression(memberAccess);
                        }
                        else
                        {
                            Unsupported.Add(new UnsupportedNodeInfo
                            {
                                Info = "Unsupported invocation expression",
                                Location = node.GetLocation(),
                                NodeName = node.ToString()
                            });
                        }
                    }
                    else
                    {
                        if (_semanticModel.HasBooleanResult(node.ArgumentList.Arguments[0].Expression))
                        {
                            //this is bad
                            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
                            node = node.WithExpression(memberAccess);
                        }
                        else
                        {
                            Unsupported.Add(new UnsupportedNodeInfo
                            {
                                Info = "Unsupported invocation expression",
                                Location = node.GetLocation(),
                                NodeName = node.ToString()
                            });
                        }
                    }
                }
                else if ("Pass".Equals(memberName) || "Fail".Equals(memberName) || "Ignore".Equals(memberName) || "Inconclusive".Equals(memberName)
                    || "AreEqual".Equals(memberName) || "AreNotEqual".Equals(memberName) || "AreSame".Equals(memberName) || "AreNotSame".Equals(memberName)
                    || "IsTrue".Equals(memberName)||"IsFalse".Equals(memberName))
                {
                    //ignored
                    return node;
                }
                else if ("Throws".Equals(memberName))
                {
                    node = TransformThrowsAssertion(node, memberAccess, false);
                }
                else if("ThrowsAsync".Equals(memberName))
                {
                    node = TransformThrowsAssertion(node, memberAccess, true);
                }
                else if ("Null".Equals(memberName))
                {
                    memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsNull"));
                    node = node.WithExpression(memberAccess);
                }
                else if ("NotNull".Equals(memberName))
                {
                    memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsNotNull"));
                    node = node.WithExpression(memberAccess);
                }
                else if("Contains".Equals(memberName))
                {
                    node = TransformContainConstraintOrExpression(node, memberAccess, false);
                }
                else if ("True".Equals(memberName))
                {
                    memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
                    node = node.WithExpression(memberAccess);
                }
                else if ("False".Equals(memberName))
                {
                    memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsFalse"));
                    node = node.WithExpression(memberAccess);
                }
                else if ("Less".Equals(memberName) ||
                         "LessOrEqual".Equals(memberName) ||
                         "Greater".Equals(memberName) ||
                         "GreaterOrEqual".Equals(memberName))
                {
                    node = TransformGreaterLess(node, memberAccess);
                }
                else if ("Zero".Equals(memberName))
                {
                    node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.EqualsExpression, 
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("NotZero".Equals(memberName))
                {
                    node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.NotEqualsExpression, 
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("IsInstanceOf".Equals(memberAccess.Name?.Identifier.ToString()))
                {
                    node = TransformIsInstanceOf(node, memberAccess);
                }
                else if ("Positive".Equals(memberName))
                {
                    node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.GreaterThanExpression,
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("Negative".Equals(memberName))
                {
                    node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.LessThanExpression,
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("IsEmpty".Equals(memberName))
                {
                    node = TransformEmptyExpression(node, memberAccess, false);
                }
                else if ("IsNotEmpty".Equals(memberName))
                {
                    node = TransformEmptyExpression(node, memberAccess, true);
                }
                else if ("IsNaN".Equals(memberName))
                {
                    node = TransformNaNExpression(node, memberAccess);
                }
                else if("IsAssignableFrom".Equals(memberAccess.Name?.Identifier.ToString()))
                {
                    node = TransformAssignableFromExpression(node, memberAccess, false);
                }
                else if ("IsNotAssignableFrom".Equals(memberAccess.Name?.Identifier.ToString()))
                {
                    node = TransformAssignableFromExpression(node, memberAccess, true);
                }
                else
                {
                    Unsupported.Add(new UnsupportedNodeInfo
                    {
                        Info = "Unsupported generic assertion expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if ("NUnit.Framework.CollectionAssert".Equals(frameworkInfo) &&
                node.Expression is MemberAccessExpressionSyntax collectionMemberAccess)
            {
                var memberName = collectionMemberAccess.Name?.ToString();
                if ("AllItemsAreInstancesOfType".Equals(memberName) || "AllItemsAreNotNull".Equals(memberName) || "AllItemsAreUnique".Equals(memberName)
                    || "AreEqual".Equals(memberName) || "AreEquivalent".Equals(memberName) || "AreNotEqual".Equals(memberName)
                    || "AreNotEquivalent".Equals(memberName) || "DoesNotContain".Equals(memberName)
                    || "IsSubsetOf".Equals(memberName) || "IsNotSubsetOf".Equals(memberName) || "Contains".Equals(memberName))
                {
                    //ignored
                    return node;
                }
                else if ("IsEmpty".Equals(memberName))
                {
                    node = TransformCollectionEmpty(node, collectionMemberAccess, false);
                }
                else if ("IsNotEmpty".Equals(memberName))
                {
                    node = TransformCollectionEmpty(node, collectionMemberAccess, true);
                }
                else
                {
                    Unsupported.Add(new UnsupportedNodeInfo
                    {
                        Info = "Unsupported collection assertion expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if ("NUnit.Framework.StringAssert".Equals(frameworkInfo) &&
                node.Expression is MemberAccessExpressionSyntax stringMemberAccess)
            {
                var memberName = stringMemberAccess.Name?.ToString();
                if ("Contains".Equals(memberName) || "StartsWith".Equals(memberName) || "EndsWith".Equals(memberName))
                {
                    //ignored
                    return node;
                }
                else if("DoesNotContain".Equals(memberName))
                {
                    node = TransformContainConstraintOrExpression(node, stringMemberAccess, true);
                    node = node.ChangeName("Assert");
                }
                else if("DoesNotStartsWith".Equals(memberName))
                {
                    node = TransformGenericStringAssertion(node, stringMemberAccess, "StartsWith", true);
                    node = node.ChangeName("Assert");
                }
                else if ("DoesNotEndWith".Equals(memberName))
                {
                    node = TransformGenericStringAssertion(node, stringMemberAccess, "EndsWith", true);
                    node = node.ChangeName("Assert");
                }
                else if ("AreEqualIgnoringCase".Equals(memberName))
                {
                    node = TransformGenericStringAssertion(node, stringMemberAccess, "Equals", 
                        false, SyntaxFactory.Argument(SyntaxFactory.IdentifierName("StringComparison.OrdinalIgnoreCase")));
                    node = node.ChangeName("Assert");
                }
                else if ("AreNotEqualIgnoringCase".Equals(memberName))
                {
                    node = TransformGenericStringAssertion(node, stringMemberAccess, "Equals", 
                        true, SyntaxFactory.Argument(SyntaxFactory.IdentifierName("StringComparison.OrdinalIgnoreCase")));
                    node = node.ChangeName("Assert");
                }
                else if("IsMatch".Equals(memberName))
                {
                    node = TransformIsMatchExpression(node, stringMemberAccess, false);
                }
                else if ("DoesNotMatch".Equals(memberName))
                {
                    node = TransformIsMatchExpression(node, stringMemberAccess, true);
                }
                else
                {
                    Unsupported.Add(new UnsupportedNodeInfo
                    {
                        Info = "Unsupported string assertion expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if ("NUnit.Framework.DirectoryAssert".Equals(frameworkInfo) &&
                node.Expression is MemberAccessExpressionSyntax directoryMemberAccess)
            {
                var memberName = directoryMemberAccess.Name?.ToString();
                if ("Exists".Equals(memberName))
                {
                    node = TransformDirectoryExists(node, directoryMemberAccess, false);
                }
                else if ("DoesNotExist".Equals(memberName))
                {
                    node = TransformDirectoryExists(node, directoryMemberAccess, true);
                }
                else
                {
                    Unsupported.Add(new UnsupportedNodeInfo
                    {
                        Info = "Unsupported directory assertion expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if ("NUnit.Framework.FileAssert".Equals(frameworkInfo) &&
                node.Expression is MemberAccessExpressionSyntax fileMemberAccess)
            {
                var memberName = fileMemberAccess.Name?.ToString();
                if ("Exists".Equals(memberName))
                {
                    node = TransformFileExists(node, fileMemberAccess, false);
                }
                else if ("DoesNotExist".Equals(memberName))
                {
                    node = TransformFileExists(node, fileMemberAccess, true);
                }
                else
                {
                    Unsupported.Add(new UnsupportedNodeInfo
                    {
                        Info = "Unsupported directory assertion expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if(frameworkInfo?.StartsWith("NUnit.Framework") ?? false)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported assertion expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformThrowsAssertion(InvocationExpressionSyntax node,
            MemberAccessExpressionSyntax memberAccess, bool isAsync)
        {
            string name = isAsync ? "ThrowsExceptionAsync" : "ThrowsException";
            if (memberAccess.Name is GenericNameSyntax)
            {
                memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(name));
                node = node.WithExpression(memberAccess);
                return node;
            }
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];

            if (arg0.Expression is TypeOfExpressionSyntax typeExpression)
            {
                var type = typeExpression.Type;
                var list = new SeparatedSyntaxList<TypeSyntax>();
                list = list.Add(type);

                memberAccess = memberAccess.WithName(SyntaxFactory.GenericName(name)
                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(list)));

                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.Add(arg1);

                node = node.WithExpression(memberAccess).WithArgumentList(
                    SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());

            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported throws expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            return node;
        }

        private InvocationExpressionSyntax TransformAssignableFromExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 1)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var arg0 = node.ArgumentList.Arguments[0];
            var name = hasNot ? "IsFalse" : "IsTrue";
            if (memberAccess.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1)//arg0.gettype().isassignablefrom(generic)
            {
                var expression = SyntaxFactory.TypeOfExpression(genericNameSyntax.TypeArgumentList.Arguments[0]);
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, "GetType().IsAssignableFrom", SyntaxFactory.Argument(expression));
                node = TransformSimpleAssertWithArguments(node, memberAccess, name, 1, SyntaxFactory.Argument(invocation));
            }
            else if (node.ArgumentList.Arguments.Count >= 2)//arg1.gettype().isassignablefrom(arg0)
            {
                var arg1 = node.ArgumentList.Arguments[1];
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg1.Expression, "GetType().IsAssignableFrom", SyntaxFactory.Argument(arg0.Expression));
                node = TransformSimpleAssertWithArguments(node, memberAccess, name, 2, SyntaxFactory.Argument(invocation));
            }

            return node;
        }

        private InvocationExpressionSyntax TransformIsMatchExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];

            var matchTypeArgument = SyntaxFactory.Argument(MSTestSyntaxFactory.CreateObjectInstance(typeof(System.Text.RegularExpressions.Regex).FullName,
                            arg0)).NormalizeWhitespace();
            string name = hasNot ? "DoesNotMatch" : "Matches";
            node = TransformSimpleAssertWithArguments(node, memberAccess, name, 2, arg1, matchTypeArgument);
            return node;
        }

        private InvocationExpressionSyntax TransformCollectionEmpty(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported DirectoryAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];

            var isEmptyExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                arg.Expression, SyntaxFactory.IdentifierName("Count"));

            var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, isEmptyExpression, SyntaxFactory.ParseExpression("0"));

            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, SyntaxFactory.Argument(binaryExpression));
            node = node.ChangeName("Assert");
            return node;
        }

        private InvocationExpressionSyntax TransformConstraintAssertion(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess)
        {
            //check if we have "not"
            bool hasNot = constraintMemberAccess.Expression is MemberAccessExpressionSyntax internalConstraintMA
                && (internalConstraintMA.Name.ToString().Equals("Not") || internalConstraintMA.Name.ToString().Equals("No"));
            var contstraintType = constraintMemberAccess.Expression.ToString();

            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);


            if (type.ConvertedType?.SpecialType == SpecialType.System_String)
            {
                node = TransformStringConstraints(node, memberAccess, constraintMemberAccess, hasNot, out bool hasChanged);
                if (hasChanged)
                    return node;
            }
            if (contstraintType.StartsWith("Is.All"))
            {
                node = TransformIsAllConstraints(node, memberAccess, constraintMemberAccess, hasNot);
            }
            else if (contstraintType.StartsWith("Is"))
            {
                node = TransformIsConstraints(node, memberAccess, constraintMemberAccess, hasNot);
            }
            else if (contstraintType.StartsWith("Does"))
            {
                node = TransformDoesConstraints(node, memberAccess, constraintMemberAccess, hasNot);
            }
            else if (contstraintType.StartsWith("Contains"))
            {
                node = TransformContainsConstraints(node, memberAccess, constraintMemberAccess, hasNot);
            }
            else if (contstraintType.StartsWith("Has"))
            {
                node = TransformHasConstraints(node, memberAccess, constraintMemberAccess, hasNot);
            }
            return node;
        }

        private InvocationExpressionSyntax TransformIsAllConstraints(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            if ("Null".Equals(constraintName) && hasNot)
            {
                node = TransformSimpleConstraint(node, memberAccess, "AllItemsAreNotNull");
                node = node.ChangeName("CollectionAssert");
            }
            else if("Unique".Equals(constraintName) && !hasNot)
            {
                node = TransformSimpleConstraint(node, memberAccess, "AllItemsAreUnique");
                node = node.ChangeName("CollectionAssert");
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported  constraint invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformHasConstraints(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            if ("Member".Equals(constraintName))
            {
                node = TransformContainsCollectionConstraint(node, memberAccess, hasNot);
            }
            else if ("Exactly".Equals(constraintName))
            {
                node = TransformCollectionExactlyConstraint(node, memberAccess, hasNot);
            }
            return node;
        }

        private InvocationExpressionSyntax TransformCollectionExactlyConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg1 = node.ArgumentList.Arguments[1];
            if (arg1.Expression is InvocationExpressionSyntax invocationExpression)
            {
                var amount = invocationExpression.ArgumentList.Arguments[0].ToString();
                node = TransformCollectionAmountConstraint(node, memberAccess, hasNot, amount);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported CollectionExactly constraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformContainsCollectionConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            if (arg1.Expression is InvocationExpressionSyntax arg1Expression)
            {
                var nodeName = hasNot ? "DoesNotContain" : "Contains";
                var newArg1 = arg1Expression.ArgumentList.Arguments[0];
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg0, newArg1);
                node = node.ChangeName("CollectionAssert");
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported ContainsConstraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformContainsConstraints(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            if ("Item".Equals(constraintName))
            {
                node = TransformContainsCollectionConstraint(node, memberAccess, hasNot);
            }
            else if ("Key".Equals(constraintName))
            {
                node = TransformContainsKeyConstraint(node, memberAccess, hasNot);
            }
            else if ("Value".Equals(constraintName))
            {
                node = TransformContainsValueConstraint(node, memberAccess, hasNot);
            }
            return node;
        }

        private InvocationExpressionSyntax TransformContainsValueConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];

            if (arg1.Expression is InvocationExpressionSyntax invocationExpression)
            {
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, "ContainsValue", invocationExpression.ArgumentList.Arguments[0]);
                var newArg = SyntaxFactory.Argument(invocation);
                var nodeName = hasNot ? "IsFalse" : "IsTrue";
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, newArg);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported ContainsValue arguments",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformContainsKeyConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];

            if (arg1.Expression is InvocationExpressionSyntax invocationExpression)
            {
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, "ContainsKey", invocationExpression.ArgumentList.Arguments[0]);
                var newArg = SyntaxFactory.Argument(invocation);
                var nodeName = hasNot ? "IsFalse" : "IsTrue";
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, newArg);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported ContainsKey arguments",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformDoesConstraints(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess, 
            MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            if ("Exist".Equals(constraintName))
            {
                node = TransformExistDirectoryConstraint(node, memberAccess, hasNot);
            }
            else if ("ContainKey".Equals(constraintName))
            {
                node = TransformContainsKeyConstraint(node, memberAccess, hasNot);
            }
            else if ("ContainValue".Equals(constraintName))
            {
                node = TransformContainsValueConstraint(node, memberAccess, hasNot);
            }
            else if ("Contain".Equals(constraintName))
            {
                node = TransformContainConstraintOrExpression(node, memberAccess, hasNot);
            }
            return node;
        }

        private InvocationExpressionSyntax TransformExistDirectoryConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported Does.Exist expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);

            ExpressionSyntax directoryExists = null;
            if (_semanticModel.TypeSymbolMatchesType(type.ConvertedType, typeof(System.IO.DirectoryInfo))
                || _semanticModel.TypeSymbolMatchesType(type.ConvertedType, typeof(System.IO.FileInfo)))
            {
                directoryExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg.Expression, SyntaxFactory.IdentifierName("Exists"));
            }
            if (directoryExists is null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported arguments in Does.Exist expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, SyntaxFactory.Argument(directoryExists));
            node = node.ChangeName("Assert");

            return node;
        }

        private InvocationExpressionSyntax TransformIsConstraints(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess, 
             MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();

            if ("EqualTo".Equals(constraintName))
            {
                node = TransformEqualToConstraint(node, memberAccess, hasNot);
            }
            else if ("Null".Equals(constraintName))
            {
                node = hasNot ? TransformSimpleConstraint(node, memberAccess, "IsNotNull") :
                    TransformSimpleConstraint(node, memberAccess, "IsNull");
            }
            else if ("NaN".Equals(constraintName))
            {
                node = TransformNaNConstraint(node, memberAccess, hasNot);
            }
            else if ("Empty".Equals(constraintName))
            {
                node = TransformCollectionAmountConstraint(node, memberAccess, hasNot, "0");
            }
            else if ("EquivalentTo".Equals(constraintName))
            {
                node = TransformEquivalentToConstraint(node, memberAccess, hasNot);
            }
            else if ("SubsetOf".Equals(constraintName))
            {
                node = TransformSubsetOfConstraint(node, memberAccess, hasNot);
            }
            else if ("AssignableTo".Equals(constraintMemberAccess.Name?.Identifier.ToString()))
            {
                node = TransformAssignableConstraint(node, memberAccess, constraintMemberAccess, hasNot, false);
            }
            else if ("AssignableFrom".Equals(constraintMemberAccess.Name?.Identifier.ToString()))
            {
                node = TransformAssignableConstraint(node, memberAccess, constraintMemberAccess, hasNot, true);
            }
            else if ("True".Equals(constraintName))
            {
                node = hasNot ? TransformSimpleConstraint(node, memberAccess, "IsFalse") :
                    TransformSimpleConstraint(node, memberAccess, "IsTrue");
            }
            else if ("False".Equals(constraintName))
            {
                node = hasNot ? TransformSimpleConstraint(node, memberAccess, "IsTrue") :
                    TransformSimpleConstraint(node, memberAccess, "IsFalse");
            }
            else if ("Positive".Equals(constraintName))
            {
                node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.GreaterThanExpression,
                   node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 2, !hasNot);
            }
            else if ("Negative".Equals(constraintName))
            {
                node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, SyntaxKind.LessThanExpression,
                   node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 2, !hasNot);
            }
            else if ("LessThan".Equals(constraintName) ||
                         "LessThanOrEqualTo".Equals(constraintName) ||
                         "GreaterThan".Equals(constraintName) ||
                         "GreaterThanOrEqualTo".Equals(constraintName))
            {
                node = TransformGreaterLessConstraint(node, memberAccess, constraintMemberAccess, hasNot);
            }
            else if (!hasNot)
            {
                if("TypeOf".Equals(constraintMemberAccess.Name?.Identifier.ToString()) || "InstanceOf".Equals(constraintMemberAccess.Name?.Identifier.ToString()))
                {
                    node = TransformTypeOfConstraint(node, memberAccess, constraintMemberAccess);
                }
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported  constraint invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformAssignableConstraint(InvocationExpressionSyntax node,
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot, bool isFrom)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            var invocationName = isFrom ? "GetType().IsAssignableFrom" : "GetType().IsAssignableTo";
            ArgumentSyntax newArg0 = null;

            if (constraintMemberAccess.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
            {
                var expression = SyntaxFactory.TypeOfExpression(genericNameSyntax.TypeArgumentList.Arguments[0]);
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, invocationName, SyntaxFactory.Argument(expression));
                newArg0 = SyntaxFactory.Argument(invocation);
            }
            else if(arg1.Expression is InvocationExpressionSyntax invocationAccess)
            {
                var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, invocationName, invocationAccess.ArgumentList.Arguments[0]);
                newArg0 = SyntaxFactory.Argument(invocation);
            }
            if (newArg0 != null)
            {
                var nodeName = hasNot ? "IsFalse" : "IsTrue";
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, newArg0);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported Assignable constraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformCollectionAmountConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot, string amount)
        {

            var arg0 = node.ArgumentList.Arguments[0];
           
            var argCountExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg0.Expression, SyntaxFactory.IdentifierName("Count"));

            var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, argCountExpression,
                SyntaxFactory.ParseExpression(amount));

            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, SyntaxFactory.Argument(binaryExpression));
            return node;
        }

        private InvocationExpressionSyntax TransformSubsetOfConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            if (arg1.Expression is InvocationExpressionSyntax arg1Expression)
            {
                var nodeName = hasNot ? "IsNotSubsetOf" : "IsSubsetOf";
                var newArg1 = arg1Expression.ArgumentList.Arguments[0];
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg0, newArg1);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported SubsetOfConstraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformEquivalentToConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            if(arg1.Expression is InvocationExpressionSyntax arg1Expression)
            {
                var nodeName = hasNot ? "AreNotEquivalent" : "AreEquivalent";
                var newArg1 = arg1Expression.ArgumentList.Arguments[0];
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg0, newArg1);
                node = node.ChangeName("CollectionAssert");
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported EquivalentToConstraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            
            return node;
        }

        private InvocationExpressionSyntax TransformStringConstraints(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess,
            MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot, out bool hasChanged)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            if ("Empty".Equals(constraintName))
            {
                node = TransformEmptyStringConstraint(node, memberAccess, hasNot);
                hasChanged = true;
                return node;
            }
            else if ("Contain".Equals(constraintName))
            {
                node = TransformContainConstraintOrExpression(node, memberAccess, hasNot);
                hasChanged = true;
                return node;
            }
            else if ("Match".Equals(constraintName))
            {
                node = TransformRegexConstraint(node, memberAccess, hasNot);
                hasChanged = true;
                return node;
            }
            else if (!hasNot)
            {
                if ("EndWith".Equals(constraintName))
                {
                    node = TransformGenericStringAssertConstraint(node, memberAccess, "EndsWith");
                    hasChanged = true;
                    return node;
                }
                else if ("StartWith".Equals(constraintName))
                {
                    node = TransformGenericStringAssertConstraint(node, memberAccess, "StartsWith");
                    hasChanged = true;
                    return node;
                }
            }

            hasChanged = false;
            return node;
        }

        private InvocationExpressionSyntax TransformContainConstraintOrExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            var invocation = arg1.Expression as InvocationExpressionSyntax;
            var nodeName = hasNot ? "DoesNotContain" : "Contains";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg0, invocation.ArgumentList.Arguments[0]);
            node = node.ChangeName("StringAssert");
            return node;
        }

        private InvocationExpressionSyntax TransformEmptyStringConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg = node.ArgumentList.Arguments[0];
            var invocation = MSTestSyntaxFactory.CreateInvocation("string", "IsNullOrEmpty", arg);

            var newArg = SyntaxFactory.Argument(invocation);
            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, newArg);
            return node;
        }

        private InvocationExpressionSyntax TransformGenericStringAssertion(InvocationExpressionSyntax node,
            MemberAccessExpressionSyntax memberAccess, string invocationName, bool hasNot, params ArgumentSyntax[] arguments)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[0];
            var invocation = MSTestSyntaxFactory.CreateInvocation(arg0.Expression, invocationName, arg1);

            var newArg = SyntaxFactory.Argument(invocation);
            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, newArg);
            return node;
        }

        private InvocationExpressionSyntax TransformRegexConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            var arg1Expression = arg1.Expression as InvocationExpressionSyntax;
            var name = hasNot ? "DoesNotMatch" : "Matches";
            var internalArg1 = arg1Expression.ArgumentList.Arguments[0];
            var matchTypeArgument = SyntaxFactory.Argument(MSTestSyntaxFactory.CreateObjectInstance(typeof(System.Text.RegularExpressions.Regex).FullName,
                            internalArg1)).NormalizeWhitespace();
            node = TransformSimpleAssertWithArguments(node, memberAccess, name, 2, arg0, matchTypeArgument);
            node = node.ChangeName("StringAssert");
            return node;
        }

        private InvocationExpressionSyntax TransformGenericStringAssertConstraint(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess, 
            string invocationName)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            var arg1Expression = arg1.Expression as InvocationExpressionSyntax;

            var newArg1 = arg1Expression.ArgumentList.Arguments[0];
            node = TransformSimpleAssertWithArguments(node, memberAccess, invocationName, 2, arg0, newArg1);
            node = node.ChangeName("StringAssert");
            return node;
        }

        private InvocationExpressionSyntax TransformTypeOfConstraint(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess,
            MemberAccessExpressionSyntax constraintMemberAccess)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            ArgumentSyntax newArg1 = null;
            if (constraintMemberAccess.Name is GenericNameSyntax genericNameSyntax)
            {
                newArg1 = SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(genericNameSyntax.TypeArgumentList.Arguments[0]));
            }
            else if (arg1.Expression is InvocationExpressionSyntax invocationExpression)
            {
                newArg1 = invocationExpression.ArgumentList.Arguments[0];
            }
            if(newArg1!= null)
            {
                node = TransformSimpleAssertWithArguments(node, memberAccess, "IsInstanceOfType", 2, arg0, newArg1);
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported ExactTypeConstraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformNaNConstraint(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var arg = node.ArgumentList.Arguments[0];
            var invocation = MSTestSyntaxFactory.CreateInvocation("double", "IsNaN", arg);
            var doubleArg = SyntaxFactory.Argument(invocation);

            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, doubleArg);
            return node;
        }

        private InvocationExpressionSyntax TransformGreaterLessConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess, bool hasNot)
        {
            SyntaxKind compareOperator;
            switch (constraintMemberAccess.Name?.ToString())
            {
                case "LessThan":
                    compareOperator = SyntaxKind.LessThanExpression;
                    break;
                case "LessThanOrEqualTo":
                    compareOperator = SyntaxKind.LessThanOrEqualExpression;
                    break;
                case "GreaterThan":
                    compareOperator = SyntaxKind.GreaterThanExpression;
                    break;
                case "GreaterThanOrEqualTo":
                    compareOperator = SyntaxKind.GreaterThanOrEqualExpression;
                    break;
                default:
                    return node;
            }

            if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 2)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg0 = node.ArgumentList.Arguments[0].Expression;

            var arg1 = node.ArgumentList.Arguments[1];
            var arg1Expression = arg1.Expression as InvocationExpressionSyntax;

            node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, compareOperator,
              arg0, arg1Expression.ArgumentList.Arguments[0].Expression, 2, !hasNot);

            return node;
        }

        private InvocationExpressionSyntax TransformEqualToConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            if (arg1.Expression is InvocationExpressionSyntax arg1Expression)
            {
                var nodeName = hasNot ? "AreNotEqual" : "AreEqual";
                var newArg1 = arg1Expression.ArgumentList.Arguments[0];
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg0, newArg1);
                //node = node.ChangeName("CollectionAssert");
            }
            else
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported EqualToConstraint",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformSimpleAssertWithArguments(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, string nodeName,int skipAmount, params ArgumentSyntax[] arguments)
        {
            var argList = new SeparatedSyntaxList<ArgumentSyntax>();
            argList = argList.AddRange(arguments);

            var remainingArguments = node.ArgumentList.Arguments.Skip(skipAmount);
            if (remainingArguments.Any())
                argList = argList.AddRange(remainingArguments);

            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(nodeName));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            return node;
        }

        private InvocationExpressionSyntax TransformSimpleConstraint(InvocationExpressionSyntax node,
            MemberAccessExpressionSyntax memberAccess, string nodeName)
        {
            var arg = node.ArgumentList.Arguments[0];
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 2, arg);
            return node;
        }

        private InvocationExpressionSyntax TransformFileExists(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported FileAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);

            ExpressionSyntax fileExists = null;
            if (type.ConvertedType?.SpecialType == SpecialType.System_String)
            {
                fileExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("File"), SyntaxFactory.IdentifierName("Exists"));

                fileExists = SyntaxFactory.InvocationExpression(fileExists, node.ArgumentList);
            }
            else if (_semanticModel.TypeSymbolMatchesType(type.ConvertedType, typeof(System.IO.FileInfo)))
            {
                fileExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg.Expression, SyntaxFactory.IdentifierName("Exists"));
            }
            if (fileExists is null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported arguments in FileAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            else
            {
                var nodeName = hasNot ? "IsFalse" : "IsTrue";
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 1, SyntaxFactory.Argument(fileExists));
                node = node.ChangeName("Assert");
            }
            return node;
        }

        private InvocationExpressionSyntax TransformDirectoryExists(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported DirectoryAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);

            ExpressionSyntax directoryExists = null;
            if (type.ConvertedType?.SpecialType == SpecialType.System_String)
            {
                directoryExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("Directory"), SyntaxFactory.IdentifierName("Exists"));

                directoryExists = SyntaxFactory.InvocationExpression(directoryExists, node.ArgumentList);
            }
            else if (_semanticModel.TypeSymbolMatchesType(type.ConvertedType, typeof(System.IO.DirectoryInfo)))
            {
                directoryExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg.Expression, SyntaxFactory.IdentifierName("Exists"));
            }
            if (directoryExists is null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported arguments in DirectoryAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            else
            {
                var nodeName = hasNot ? "IsFalse" : "IsTrue";
                node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 1, SyntaxFactory.Argument(directoryExists));
                node = node.ChangeName("Assert");
            }
            return node;
        }

        private InvocationExpressionSyntax TransformNaNExpression(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var arg = node.ArgumentList.Arguments[0];
            var invocation = MSTestSyntaxFactory.CreateInvocation("double", "IsNaN", arg);
            node = TransformSimpleAssertWithArguments(node, memberAccess, "IsTrue", 1, SyntaxFactory.Argument(invocation));
            return node;
        }

        private InvocationExpressionSyntax TransformEmptyExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool hasNot)
        {
            if (node.ArgumentList == null)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);
            ArgumentSyntax resultArgument;
            if (type.ConvertedType?.SpecialType == SpecialType.System_String)
            {
                resultArgument = SyntaxFactory.Argument(MSTestSyntaxFactory.CreateInvocation("string", "IsNullOrEmpty", arg));
            }
            else
            {
                var isEmptyExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg.Expression, SyntaxFactory.IdentifierName("Count"));

                var binaryExpression = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, isEmptyExpression, SyntaxFactory.ParseExpression("0"));
                resultArgument = SyntaxFactory.Argument(binaryExpression);
            }
            var nodeName = hasNot ? "IsFalse" : "IsTrue";
            node = TransformSimpleAssertWithArguments(node, memberAccess, nodeName, 1, resultArgument);

            return node;
        }

        private InvocationExpressionSyntax TransformIsInstanceOf(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess)
        {
            if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 1)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var arg0 = node.ArgumentList.Arguments[0];

            if (memberAccess.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.TypeArgumentList.Arguments.Count == 1)
            {
                var expression = SyntaxFactory.TypeOfExpression(genericNameSyntax.TypeArgumentList.Arguments[0]);
                node = TransformSimpleAssertWithArguments(node, memberAccess, "IsInstanceOfType", 1, arg0, SyntaxFactory.Argument(expression));
            }
            else if (node.ArgumentList.Arguments.Count >= 2)
            {
                var arg1 = node.ArgumentList.Arguments[1];
                node = TransformSimpleAssertWithArguments(node, memberAccess, "IsInstanceOfType", 2, arg1, arg0);
            }

            return node;
        }

        private InvocationExpressionSyntax TransformGreaterLess(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess)
        {
            SyntaxKind compareOperator;
            switch (memberAccess.Name?.ToString())
            {
                case "Less":
                    compareOperator = SyntaxKind.LessThanExpression;
                    break;
                case "LessOrEqual":
                    compareOperator = SyntaxKind.LessThanOrEqualExpression;
                    break;
                case "Greater":
                    compareOperator = SyntaxKind.GreaterThanExpression;
                    break;
                case "GreaterOrEqual":
                    compareOperator = SyntaxKind.GreaterThanOrEqualExpression;
                    break;
                default:
                    return node;
            }

            if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 2)
            {
                Unsupported.Add(new UnsupportedNodeInfo
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg0 = node.ArgumentList.Arguments[0].Expression;
            var arg1 = node.ArgumentList.Arguments[1].Expression;

            node = MSTestSyntaxFactory.CreateComparisonExpression(node, memberAccess, compareOperator,
              arg0, arg1, 2);

            return node;
        }
    }
}
