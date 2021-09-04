using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NUnitMigrator.Core.Rewriter
{
    public class Rewriter : CSharpSyntaxRewriter
    {
        public readonly List<RewriterError> Errors;
        private readonly SemanticModel _semanticModel;

        private readonly MethodState _methodState;
        private readonly ClassState _classState;

        public Rewriter(SemanticModel model) : base()
        {
            _semanticModel = model;
            _methodState = new MethodState();
            _classState = new ClassState();
            Errors = new List<RewriterError>();
        }


        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _classState.Clear();
            node = base.VisitClassDeclaration(node) as ClassDeclarationSyntax;
            if (node != null)
            {
                if (_classState.IsClassAtrributeNeeded)
                {
                    node = node.AddAttributeWithName(RewriterData.MSTestData.TEST_CLASS_ATTRIBUTE);
                }
                if (_classState.RemovedAttributes.Count > 0)
                {
                    foreach (var attribute in _classState.RemovedAttributes)
                    {
                        node = node.RemoveAttribute(attribute, true);
                    }
                }
            }
            return node;
        }
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _methodState.Clear();
            _methodState.CurrentMethod = node;
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
            }


            return node;
        }

        public override SyntaxNode VisitAttribute(AttributeSyntax attribute)
        {
            attribute = (AttributeSyntax)base.VisitAttribute(attribute);
            return HandleAttribute(attribute);
        }
       
        private SyntaxNode HandleAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            switch (name)
            {
                case RewriterData.NUnitData.TEST_FIXTURE_ATTRIBUTE:
                    if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
                        throw new NotImplementedException();
                    attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.TEST_CLASS_ATTRIBUTE));
                    _classState.IsClassAtrributeNeeded = false;
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
                            Errors.Add(new RewriterError
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

        //mostly legacy code
        private AttributeSyntax TransformTestCaseAttribute(AttributeSyntax attribute)
        {
            attribute = attribute.WithName(SyntaxFactory.IdentifierName(RewriterData.MSTestData.DATA_ROW_ATTRIBUTE));
            _methodState.AddedAttributes.Add(SyntaxFactory.Attribute(SyntaxFactory.ParseName(RewriterData.MSTestData.TEST_METHOD_ATTRIBUTE)));

            AttributeArgumentSyntax explicitArgument = null;
            AttributeArgumentSyntax ignoreArgument = null;
            AttributeArgumentSyntax reasonArgument = null;

            attribute.ArgumentList.Arguments.ToList().ForEach(arg =>
            {
                var argument = arg.ToString();
                if (RewriterData.NUnitData.TestNameRegex.IsMatch(argument))
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("\"TestName\"")));
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(arg.Expression));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("TestProperty"), argumentListSyntax);
                    _methodState.AddedAttributes.Add(newAttribute);
                }
                else if (RewriterData.NUnitData.AuthorRegex.IsMatch(argument))
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(arg.Expression));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Owner"), argumentListSyntax);
                    _methodState.AddedAttributes.Add(newAttribute);
                }
                else if (RewriterData.NUnitData.CategoryRegex.IsMatch(argument))
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(arg.Expression));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("TestCategory"), argumentListSyntax);
                    _methodState.AddedAttributes.Add(newAttribute);

                }
                else if (RewriterData.NUnitData.DescriptionRegex.IsMatch(argument))
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(arg.Expression));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Description"), argumentListSyntax);
                    _methodState.AddedAttributes.Add(newAttribute);
                }
                else if (RewriterData.NUnitData.ExplicitRegex.IsMatch(argument))
                {
                    explicitArgument = arg;
                }
                else if (RewriterData.NUnitData.IgnoreRegex.IsMatch(argument))
                {
                    ignoreArgument = arg;
                }
                else if (RewriterData.NUnitData.IgnoreReasonRegex.IsMatch(argument))
                {
                    ignoreArgument = arg;
                }
                else if (RewriterData.NUnitData.ReasonRegex.IsMatch(argument))
                {
                    reasonArgument = arg;
                }
                else if (RewriterData.NUnitData.TestOfRegex.IsMatch(argument))
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("\"" + arg.Expression + "\"")));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Description"), argumentListSyntax);
                    _methodState.AddedAttributes.Add(newAttribute);
                }

            });
            if (explicitArgument != null && explicitArgument.Expression.ToString().Equals("true"))
            {
                AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Explicit"));
                if (reasonArgument != null)
                {
                    SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                    argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(reasonArgument.Expression));
                    AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                    newAttribute = newAttribute.WithArgumentList(argumentListSyntax);
                }
                _methodState.AddedAttributes.Add(newAttribute);
            }
            if (ignoreArgument != null)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> argumentsList = new SeparatedSyntaxList<AttributeArgumentSyntax>();
                argumentsList = argumentsList.Add(SyntaxFactory.AttributeArgument(ignoreArgument.Expression));
                AttributeArgumentListSyntax argumentListSyntax = SyntaxFactory.AttributeArgumentList(argumentsList);
                AttributeSyntax newAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("Ignore"), argumentListSyntax);
                _methodState.AddedAttributes.Add(newAttribute);
            }
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
                Errors.Add(new RewriterError
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
                Errors.Add(new RewriterError
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

            node = HandleInvocationExpression(node, info);
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
                        if (secondArgument.Expression is MemberAccessExpressionSyntax constraintMemberAccess)
                        {
                            node = TransformConstraintAssertion(node, memberAccess, constraintMemberAccess);
                        }
                        else if(secondArgument.Expression is InvocationExpressionSyntax constraintInvocationExpression 
                            && constraintInvocationExpression.Expression is MemberAccessExpressionSyntax invocationConstraintMA)
                        {
                            node = TransformConstraintAssertion(node, memberAccess, invocationConstraintMA);
                        }
                        else if (_semanticModel.HasBooleanResult(node.ArgumentList.Arguments[0].Expression))
                        {
                            //this is bad
                            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
                            node = node.WithExpression(memberAccess);
                        }
                        else
                        {
                            Errors.Add(new RewriterError
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
                            Errors.Add(new RewriterError
                            {
                                Info = "Unsupported invocation expression",
                                Location = node.GetLocation(),
                                NodeName = node.ToString()
                            });
                        }
                    }
                }
                else if ("Pass".Equals(memberName) || "Fail".Equals(memberName) || "Ignore".Equals(memberName) || "Inconclusive".Equals(memberName)
                    || "AreEqual".Equals(memberName) || "AreNotEqual".Equals(memberName) || "AreSame".Equals(memberName) || "AreNotSame".Equals(memberName))
                {
                    //ignored
                    return node;
                }
                else if ("Throws".Equals(memberName))
                {
                    memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("ThrowsException"));
                    node = node.WithExpression(memberAccess);
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
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.EqualsExpression, 
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("NotZero".Equals(memberName))
                {
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.NotEqualsExpression, 
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("IsInstanceOf".Equals(memberAccess.Name?.Identifier.ToString()))
                {
                    node = TransformIsInstanceOf(node, memberAccess);
                }
                else if ("Positive".Equals(memberName))
                {
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.GreaterThanExpression,
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("Negative".Equals(memberName))
                {
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.LessThanExpression,
                        node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 1);
                }
                else if ("IsEmpty".Equals(memberName))
                {
                    node = TransformEmptyExpression(node, memberAccess, true);
                }
                else if ("IsNotEmpty".Equals(memberName))
                {
                    node = TransformEmptyExpression(node, memberAccess, false);
                }
                else if ("IsNaN".Equals(memberName))
                {
                    node = TransformNaNExpression(node, memberAccess);
                }
                else
                {
                    Errors.Add(new RewriterError
                    {
                        Info = "Unsupported assertion expression",
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
                    || "AreNotEquivalent".Equals(memberName) || "Contains".Equals(memberName) || "DoesNotContain".Equals(memberName)
                    || "IsSubsetOf".Equals(memberName) || "IsNotSubsetOf".Equals(memberName))
                {
                    //ignored
                    return node;
                }
                else
                {
                    Errors.Add(new RewriterError
                    {
                        Info = "Unsupported collection assert expression",
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
                else
                {
                    Errors.Add(new RewriterError
                    {
                        Info = "Unsupported string assert expression",
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
                    node = TransformDirectoryExists(node, directoryMemberAccess, true);
                }
                else if ("DoesNotExist".Equals(memberName))
                {
                    node = TransformDirectoryExists(node, directoryMemberAccess, false);
                }
                else
                {
                    Errors.Add(new RewriterError
                    {
                        Info = "Unsupported directory assert expression",
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
                    node = TransformFileExists(node, fileMemberAccess, true);
                }
                else if ("DoesNotExist".Equals(memberName))
                {
                    node = TransformFileExists(node, fileMemberAccess, false);
                }
                else
                {
                    Errors.Add(new RewriterError
                    {
                        Info = "Unsupported directory assert expression",
                        Location = node.GetLocation(),
                        NodeName = node.ToString()
                    });
                }
            }
            else if(frameworkInfo?.StartsWith("NUnit.") ?? false)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported assert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }

            return node;
        }

        private InvocationExpressionSyntax TransformConstraintAssertion(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess)
        {
            var constraintName = constraintMemberAccess.Name?.ToString();
            //check if we have "not"
            bool hasNot = constraintMemberAccess.Expression is MemberAccessExpressionSyntax internalConstraintMA
                && internalConstraintMA.Name.ToString().Equals("Not");
            if ("EqualTo".Equals(constraintName))
            {
                node = TransformEqualToConstraint(node, memberAccess, hasNot);
            }
            else if ("Null".Equals(constraintName))
            {
                node = hasNot ? TransformSimpleConstraint(node, memberAccess, "IsNotNull") :
                    TransformSimpleConstraint(node, memberAccess, "IsNull");
            }
            else if(!hasNot)
            {
                //so we will not translate not true to true etc.
                if ("True".Equals(constraintName))
                {
                    node = TransformSimpleConstraint(node, memberAccess, "IsTrue");
                }
                else if ("False".Equals(constraintName))
                {
                    node = TransformSimpleConstraint(node, memberAccess, "IsFalse");
                }
                else if ("LessThan".Equals(constraintName) ||
                         "LessThanOrEqualTo".Equals(constraintName) ||
                         "GreaterThan".Equals(constraintName) ||
                         "GreaterThanOrEqualTo".Equals(constraintName))
                {
                    node = TransformGreaterLessConstraint(node, memberAccess, constraintMemberAccess);
                }
                else if ("Positive".Equals(constraintName))
                {
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.GreaterThanExpression,
                       node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 2);
                }
                else if ("Negative".Equals(constraintName))
                {
                    node = TransformComparisonExpression(node, memberAccess, SyntaxKind.LessThanExpression,
                       node.ArgumentList.Arguments[0].Expression, SyntaxFactory.ParseExpression("0"), 2);
                }
            }
            else
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported  constraint invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            return node;
        }

        private InvocationExpressionSyntax TransformGreaterLessConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, MemberAccessExpressionSyntax constraintMemberAccess)
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
                Errors.Add(new RewriterError
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

            node = TransformComparisonExpression(node, memberAccess, compareOperator,
              arg0, arg1Expression.ArgumentList.Arguments[0].Expression, 2);

            return node;
        }

        private InvocationExpressionSyntax TransformEqualToConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool areNotEqual)
        {
            var arg0 = node.ArgumentList.Arguments[0];
            var arg1 = node.ArgumentList.Arguments[1];
            var arg1Expression = arg1.Expression as InvocationExpressionSyntax;

            var argList = new SeparatedSyntaxList<ArgumentSyntax>();
            argList = argList.Add(arg0);
            argList = argList.Add(arg1Expression.ArgumentList.Arguments[0]);


            var remainingArguments = node.ArgumentList.Arguments.Skip(2);
            if (remainingArguments.Any())
                argList = argList.AddRange(remainingArguments);

            memberAccess = areNotEqual ? memberAccess.WithName(SyntaxFactory.IdentifierName("AreNotEqual"))
                : memberAccess.WithName(SyntaxFactory.IdentifierName("AreEqual"));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            return node;
        }

        private InvocationExpressionSyntax TransformSimpleConstraint(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, string nodeName)
        {
            var arg = node.ArgumentList.Arguments[0];
            var argList = new SeparatedSyntaxList<ArgumentSyntax>();
            argList = argList.Add(arg);

            var remainingArguments = node.ArgumentList.Arguments.Skip(2);
            if (remainingArguments.Any())
                argList = argList.AddRange(remainingArguments);

            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(nodeName));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            return node;
        }

        private InvocationExpressionSyntax TransformFileExists(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool doesExist)
        {
            if (node.ArgumentList == null)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported FileAssert expression",
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
                    SyntaxFactory.IdentifierName("File"), SyntaxFactory.IdentifierName("Exists"));

                directoryExists = SyntaxFactory.InvocationExpression(directoryExists, node.ArgumentList);
            }
            else if (_semanticModel.TypeSymbolMatchesType(type.ConvertedType, typeof(System.IO.FileInfo)))
            {
                directoryExists = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    arg.Expression, SyntaxFactory.IdentifierName("Exists"));
            }
            if (directoryExists is null)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported arguments in FileAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            else
            {
                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.Add(SyntaxFactory.Argument(directoryExists));

                var remainingArguments = node.ArgumentList.Arguments.Skip(1);
                if (remainingArguments.Any())
                    argList = argList.AddRange(remainingArguments);

                var trivia = memberAccess.GetLeadingTrivia();

                memberAccess = doesExist ?
                    memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"))
                    : memberAccess.WithName(SyntaxFactory.IdentifierName("IsFalse"));

                memberAccess = memberAccess.WithExpression(SyntaxFactory.IdentifierName("Assert"))
                    .WithLeadingTrivia(trivia);
                node = node.WithExpression(memberAccess).WithArgumentList(
                    SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            }
            return node;
        }

        private InvocationExpressionSyntax TransformDirectoryExists(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool doesExist)
        {
            if (node.ArgumentList == null)
            {
                Errors.Add(new RewriterError
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
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported arguments in DirectoryAssert expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
            }
            else
            {
                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.Add(SyntaxFactory.Argument(directoryExists));

                var remainingArguments = node.ArgumentList.Arguments.Skip(1);
                if (remainingArguments.Any())
                    argList = argList.AddRange(remainingArguments);

                var trivia = memberAccess.GetLeadingTrivia();

                memberAccess = doesExist ? 
                    memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue")) 
                    : memberAccess.WithName(SyntaxFactory.IdentifierName("IsFalse"));

                memberAccess = memberAccess.WithExpression(SyntaxFactory.IdentifierName("Assert"))
                    .WithLeadingTrivia(trivia);
                node = node.WithExpression(memberAccess).WithArgumentList(
                    SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            }
            return node;
        }

        private InvocationExpressionSyntax TransformNaNExpression(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess)
        {
            if (node.ArgumentList == null)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);

            if (type.ConvertedType?.SpecialType != SpecialType.System_Double)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported arguments in invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }

            var argList = new SeparatedSyntaxList<ArgumentSyntax>();

            var isNanExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
                SyntaxFactory.IdentifierName("double"), SyntaxFactory.IdentifierName("IsNaN"));

            argList = argList.Add(SyntaxFactory.Argument(SyntaxFactory.InvocationExpression(isNanExpression, node.ArgumentList)));

            var remainingArguments = node.ArgumentList.Arguments.Skip(1);
            if (remainingArguments.Any())
                argList = argList.AddRange(remainingArguments);

            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());

            return node;
        }

        private InvocationExpressionSyntax TransformEmptyExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, bool isEmpty)
        {
            if (node.ArgumentList == null)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg = node.ArgumentList.Arguments[0];
            var type = _semanticModel.GetTypeInfo(arg.Expression);

            if (type.ConvertedType?.SpecialType != SpecialType.System_String)
            {
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported arguments in invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var argList = new SeparatedSyntaxList<ArgumentSyntax>();

            var stringIsEmpty = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("string"), SyntaxFactory.IdentifierName("IsNullOrEmpty"));

            argList = argList.Add(SyntaxFactory.Argument(SyntaxFactory.InvocationExpression(stringIsEmpty, node.ArgumentList)));

            var remainingArguments = node.ArgumentList.Arguments.Skip(1);
            if (remainingArguments.Any())
                argList = argList.AddRange(remainingArguments);

            memberAccess = isEmpty ? memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue")) 
                : memberAccess.WithName(SyntaxFactory.IdentifierName("IsFalse"));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());

            return node;
        }

        private InvocationExpressionSyntax TransformIsInstanceOf(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberAccess)
        {
            if (node.ArgumentList == null || node.ArgumentList.Arguments.Count < 1)
            {
                Errors.Add(new RewriterError
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
                var remainingArguments = node.ArgumentList.Arguments.Skip(1);

                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.Add(arg0);
                argList = argList.Add(SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(genericNameSyntax.TypeArgumentList.Arguments[0])));
                if (remainingArguments.Any())
                {
                    argList = argList.AddRange(remainingArguments);
                }

                memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsInstanceOfType"));
                node = node.WithExpression(memberAccess).WithArgumentList(SyntaxFactory.ArgumentList(argList)
                    .NormalizeWhitespace());
            }
            else if (node.ArgumentList.Arguments.Count >= 2)
            {
                var arg1 = node.ArgumentList.Arguments[1];

                var argList = new SeparatedSyntaxList<ArgumentSyntax>();
                argList = argList.Add(arg1);
                argList = argList.Add(arg0);
                var remainingArguments = node.ArgumentList.Arguments.Skip(2);
                if (remainingArguments.Any())
                {
                    argList = argList.AddRange(remainingArguments);
                }

                memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsInstanceOfType"));
                node = node.WithExpression(memberAccess).WithArgumentList(SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
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
                Errors.Add(new RewriterError
                {
                    Info = "Unsupported invocation expression",
                    Location = node.GetLocation(),
                    NodeName = node.ToString()
                });
                return node;
            }
            var arg0 = node.ArgumentList.Arguments[0].Expression;
            var arg1 = node.ArgumentList.Arguments[1].Expression;

            node = TransformComparisonExpression(node, memberAccess, compareOperator,
              arg0, arg1, 2);

            return node;
        }

        //does not look good
        private InvocationExpressionSyntax TransformComparisonExpression(InvocationExpressionSyntax node, 
            MemberAccessExpressionSyntax memberAccess, SyntaxKind compareOperator, 
            ExpressionSyntax arg0, ExpressionSyntax arg1, int initialArgAmount)
        {
            var argList = new SeparatedSyntaxList<ArgumentSyntax>();
            var binaryExpression = SyntaxFactory.BinaryExpression(compareOperator, arg0, arg1);
            argList = argList.Add(SyntaxFactory.Argument(binaryExpression).NormalizeWhitespace());
            var remainingArguments = node.ArgumentList.Arguments.Skip(initialArgAmount);
            if (remainingArguments.Any())
            {
                argList = argList.AddRange(remainingArguments);
            }
            memberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("IsTrue"));
            node = node.WithExpression(memberAccess).WithArgumentList(
                SyntaxFactory.ArgumentList(argList).NormalizeWhitespace());
            return node;
        }
    }
}
