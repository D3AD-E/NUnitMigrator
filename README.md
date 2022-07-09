# NUnitMigrator

> My bachelor diploma thesis at Warsaw University of Technology for specialisation Computer Systems and Networks

# Comparison of NUnit and MSTest frameworks

The following basic terms will be used in this work:

- **Unit test** is a method of verifying whether the software works as the programmer expected by writing small test methods fragments (units) of the main program - objects and methods. Unit tests ensure that the software produced meets the requirements and works appropriately. Writing tests unit is also an excellent opportunity to consider the extreme cases for the code under test and make sure that the software works not only during the execution of the typical scenarios. Good code can react to various errors, handle them, and signal what happened legibly.
- **The NUnit platform** is an open-source project from the _xUnit_ family. The _xUnit_ family was created in 1999 when Kent Beck published the unit testing platform for Smalltalk. First, JUnit was created based on _SmalltalkUnit_ (or _SUnit_). Junit is a unit testing framework for Java applications. Based on _Junit CppUnit, PyUnit, XMLUnit_, and versions for many other languages, including _NUnit,_ were created. It currently exists over one hundred development platforms based on the _xUnit_ architecture. They are known as family _xUnit_ tools. They are all free and open-source software. As mentioned earlier, _NUnit_ is based on _JUnit_. But from version 3.0 it was rewritten. The migration will be performed from _NUnit_ v3.
- **MSTest** is part of the standard kit of Microsoft tools for testing. Therefore, this development platform is widely used and assisted. The migration will be performed into _MSTest_ v2.

## Test structure

Test structure is the same for both _MSTest_ and _NUnit_. Test file is composed of a test class that contains multiple methods. It is considered good practice to group tests by the functionality that they test and put such groups into separate classes. The methods are decorated with specific attributes that determine whether a method is a test, a setup procedure, or a procedure run after a test. If the method is a test, it should contain an assertion. The attributes can also provide arguments for the methods or enhance the method with specific properties, such as: making the test fail if it is not completed in a certain amount of time, etc.

## Test procedure

The process of test execution is the same in both _NUnit_ and _MSTest_ frameworks.

1. The method marked with _OneTimeSetUp_ or _ClassInitialize_ attribute is run if it is present. The method contains code that must be used before any of the tests in the test class run and allocates resources to be used by the test class.
2. The method marked with _SetUp_ or _TestInitialize_ attribute is run if it is present. The method is used to allocate and configure resources needed by all tests in the test class.
3. The unit test is run. Such methods are usually marked with _Test_ or _TestMethod_ attributes.
4. The method marked with _TearDown_ or _TestCleanup_ attribute is run. The method frees resources obtained by all the tests in the test class.
5. Steps 2-4 are repeated for every test in the test class.
6. The method marked with _OneTimeTearDown_ or _ClassCleanup_ attribute is run if it is present. The method contains code to be used after all the tests in the test class have run and frees resources obtained by the test class.

## Differences between MSTest and NUnit

There are multiple significant differences between _NUnit_ and _MSTest_:

- **Syntax** – attributes and assertions with the same functionality can have different names in _NUnit_ and _MSTest_. For example: _TestAttribute_ and _TestMethodAttribute_.
- **Assertions** – in _NUnit_ there are two types of assertions: classical model and constraint model. Contrary to _MSTest_ where there is only the classical model.
- **Test run order** – in _NUnit_ we can determine in which order the tests will be performed by marking the methods with Order attribute. In _MSTest_ all tests are run only in alphabetical order.
- **Theory** is a particular type of test in _NUnit_, used to verify a general statement about the system under development. The theory itself is responsible for ensuring that all data supplied meets its assumptions. It does this by use of the _Assume.That_. If the assumption is not satisfied for a particular test case, that case returns an Inconclusive result rather than a Success or Failure. Such functionality is not available in _MSTest_.
- **Conditional test execution** – in _NUnit_ tests may or may not be performed based on specific dynamic attributes such as Culture or Platform. In _MSTest_ it should be decided beforehand whether the test runs or not.

Before translating any tests, we need to change the using directive from _NUnit - NUnit.Framework_ to _MSTest - Microsoft.VisualStudio.TestTools.UnitTesting_.

If the migration is done through the extension, the _MSTest_ packages will be installed: _MSTest.TestAdapter_ and _MSTest.TestFramework_.

The following chapters provide descriptions of the _NUnit_ assertions and attributes that will be translated to corresponding _MSTest_ ones.

# Project of NUnitMigrator

_NUnitMigrator_ is written in C # using the .NET Compiler Platform._NUnitMigrator_ code consists of four Projects: App, Core, Extension, and Tests.The purpose and content of the above projects will be discussed in this chapter.

The core of the _NUnitMigrator_ – translation of unit tests is done with the use of .NET Compiler Platform. Therefore, before discussing their implementation, this technology must also be addressed.

## NUnitMigrator core implementation with .NET Compiler Platform v. 3.11.0.0

_Roslyn_ or ._NET Compiler Platform_is an open-source library that performs the compilation of Visual Basic and C# source code. It also provides an API for building code analysis tools. _NUnitMigrator_ is based on _Roslyn_ functionality because it allows access to code modification, rearrangement, and analysis.

The core object of code to be analyzed is the syntax tree. It is the core structure used for compilation, code analysis, binding, refactoring, and code generation. The main features of the syntax tree are:

- Syntax trees hold all source information in full complexity. Full complexity means that the syntax tree contains every bit of data found within the source text, all grammar and lexical tokens, and everything else in between, including white spaces, comments, and preprocessor directives. Syntax trees can recognize syntax errors.
- Syntax trees can produce the exact text that they were parsed from. From any syntax node, it&#39;s possible to get its text representation. This ability means that syntax trees can be used to construct and edit source text.
- Syntax trees are immutable and thread-safe. After a tree is instantiated, it is a copy of the present state of the code and can never be changed. Factory methods can be used to create and modify syntax trees by creating additional versions of the tree. The trees reuse the existing nodes. Thus a new version of the tree can be created quickly with a minor increase in memory usage.

Syntax nodes are the primary elements of syntax trees. They represent declarations, statements, clauses, and expressions.

All syntax nodes are non-terminal nodes in the syntax tree, which means they always have other nodes and tokens as children. Each node has a parent node that can be accessed through the _SyntaxNode.Parent_ property. Because nodes and, therefore, trees are immutable, the parent of a node never changes. The root of the tree has a null parent.

Following children classes of _SyntaxNode_ are used in _NUnitMigrator:_

- **MemberDeclarationSyntax, ClassDeclarationSyntax, MethodDeclarationSyntax** - represent classes and methods in a syntax tree. Most analysis is done on these nodes.
- **AttributeListSyntax, AttributeSyntax,**  **AttributeArgumentList** - represent one or more attributes.
- **ExpressionSyntax, InvocationExpressionSyntax, MemberAccessExpressionSyntax** – represent assertions, method calls, function calls.
- **UsingDirectiveSyntax** – represents using directive.

Syntax tokens are the simplest possible components of the language grammar, representing the smallest syntactic fragments of the code. They can never be parents of any node and consist of keywords, identifiers, literals, and punctuation.

Syntax trivia represents the parts of the source text that are primarily insignificant for general understanding of the code—for example, white spaces, comments, and preprocessor directives.

When source text is parsed, multiple trivia objects are associated with tokens. Usually, a token has any trivia after it on the same line up to the following token. Any trivia after that line is associated with the next token.

Each node, token, or trivia has a _SyntaxNode.RawKind_ property allows for easy distinguishing of syntax node types with the same node class. For tokens and trivia, this property is the only way to distinguish one type of element from another.

Additionally, semantics is used to determine whether the node should be translated. The semantic model encapsulates the language rules, giving an easy way to match identifiers with the correct program element being referenced correctly.

Every namespace, type, method, property, field, event, parameter, or local variable is represented by a symbol. A symbol represents a distinct element declared by the source code or imported from an assembly as metadata. Symbols are similar to the CLR type system as represented by the _System.Reflection_ API.

## General algorithm

In this section, the process of translation is described from the more general view. Not diving into details of migration, as it was described in Section 4 and 5, but rather giving an explanation about the core processes inside _NUnitMigrator_ after the program was provided with a solution to work on.

In the following chapters, the algorithm of migration process (_Migrate(…)_ code, used in Code 27 is described.The migration process can be divided into three phases: Analysis, Creation, and Translation.

### Analysis

During the analysis phase, modified versions of syntax tree are not created yet. Only the information about the correct way of translation is passed to the next phase. The analysis is most often immediately followed by translation.

In general, there are three outcomes of the analysis phase:

- **Direct translation**. It is the most often outcome. When the node can be translated on the spot and no changes in other parts of the file are needed.
- **Delayed translation**. When the correct node translation requires additional changes in some other parts of the file. Thus, the information about needed changes is recorded in State classes. Ex. _UICulture_ requires changes inside the method body, and the information is written inside _UICultureExpressionsState_
- **No translation**. When the node is considered unsupported and cannot be translated correctly. The information about node location and type is stored. If the setting to &quot;comment the unsupported nodes&quot; is set to true, then the node will be commented in the translation part. Otherwise analysis will not be followed by any translation

### Creation

The creation phase is pre-translation generation of whole attributes, exceptions, and invocations. It is performed in _MSTestSyntaxFactory_ class. However, modified versions of syntax tree are not created yet. This phase is optional and is often skipped because pre-translation generation of nodes is only needed in complex cases where changes cannot be made on the spot.

- **Exception creation** – used in translation of _Throws_ assertion. It is performed by _ThrowsExceptionSyntax, ThrowsExceptionNaked, ThrowsExceptionWithMatch_ methods
- **Attribute creation** is sometimes used when the outcome of the analysis phase is delayed translation. It is performed by CreateAttribute method
- **Invocation** – used for assert translation. It is performed by _CreateInvocation_ method

### Translation

During translation phase, the modified version of syntax tree is created. There are three types of changes:

- **Node removal.** Removal of the node is only performed during attribute translation. It is done with _RemoveAttribute_ method extension that removes the attribute, saving the leading and trailing trivia. This method can be called on _AttributeList, MethodDeclarationSyntax_ and _ClassDeclarationSyntax_.
- **Node addition.** Addition of the node is only performed during attribute translation. It is the least used method during translation, if quite rarely a completely new element is required. However, when necessary, it can be done with _AddAttributeWithName, AddAttribute, AddAttributes, AddExpression_ method extensions. Most often, uses the results from creation phase.
- **Node replacement.** It is a generalization of multiple types of replacements
  - Commenting out. If the setting to &quot;comment the unsupported nodes&quot; is set to true and the node is considered _Unsupported_, the node will be commented. In other words, all internal code of the node will be unchanged but transformed into comments, so the compiler would not process it.
  - Attribute replacement. Due to Roslyn restrictions, it is impossible to replace only the part of the attribute. Thus, if any changes are required, the whole attribute will be replaced. Such type of replacement is most often used if the outcome of analysis phase is _Immediate translation._
  - Internal assertion replacement. Used when only part of the assertion must be changed. Ex. Translation of _Assert.Null_ into _Assert.IsNull_. It is usually done inline, without any calls to external methods.
  - Whole assertion replacement. Used when the whole assertion or the core part must be changed. Ex. Assertions that require reversal of argument order or assertions that need to be translated via translation into _Assert.IsTrue._

## Structure

- **CSharpSynaxRewriter** - _.NET Complier Platform_ class. It is used for reading and rewriting the test files
- **IClearable** – interface for resetting data inside of class
- **ValuesRangeState** – data class to store correct translation of _ValuesRange_ attribute
- **AuthorState** – data class to store correct translation of Autor attribute. Ex. Whether the email translation is needed
- **UICultureExpressionsState** – data class to store accurate translation of _UICulture_ attribute
- **MethodState** – data class to store information about the currently processed method, uses _ValuesRangeState, AuthorState, UICultureState_
- **ExceptionSyntax** - data class to store information about the currently processed exception
- **ClassState** – data class to store information about the currently processed class
- **MSTestData** – static class that contains info about attributes contained in _MSTest_ framework
- **NUnitData** – static class that includes info on attributes contained in _NUnit_ framework
- **RewriterData** – static class that encapsulates _MSTestData_ and _NUnitData_
- **UnsupportedNodeInfo** – data class to store information about the unsupported nodes (location, description)
- **RewriterOptions** – data class, used in initialization to store options for rewriter. Currently, the only option is to comment the unsupported nodes or not.
- **ExceptionParser** – helper class for translation of Throws assertions
- **MSTestSyntaxFactory** - helper class for creation of _MSTest_ syntax
- **RewriterExtentions** – helper class for creating extensions on _Roslyn_ classes
- **Rewriter** – the main class that performs translation

## Usage

There is one significant difference between the console application and the extension:

- If you use the extension, the _MSTest_ package will be automatically installed
- If you use the console application, you will have to install the _MSTest_ package with a _NuGet_ package manager.

But the advantage of the console application is that you do not need to open an instance of Visual studio to use it.

### Console application

_NUnitMigrator_ can be used as a standalone console application. The console application is created in App project. To run the application, you need to follow such steps:

1. You can run the application directly from the source code. Right-click the _App_ project and click the _Set as Startup Project_
2. Specify the command line arguments if needed (_-c_ flag will comment the unsupported nodes)
3. Run the project by clicking _Start_ button on top with the green arrow next to it.
4. The file dialog will pop up. Select the solution you want to migrate. Click _OK_
5. The file dialog will pop up. Select the project you want to migrate. Click _OK_. If you&#39;re going to migrate the whole solution instead, just close the file dialog window.
6. You will see the following output. The information is presented about the unsupported nodes amount, their code representation, as well as files that were skipped due to lack of _NUnit_ test code. At the end, the total amount of changes and total unsupported nodes amount is shown.


### Visual Studio extension

_NUnitMigrator_ can be used as an extension to the Integrated Development Environment (IDE) Visual Studio 2019. The extension is created in the _Extension_ project. After building this project, a _.vsix_ extension file is created. This file is a ready-made extension. It can be installed through the Visual Studio Installer program. To run the extension, you need to follow such steps:

1. To create a Visual Studio extension, it is necessary to install a set of software tools (Software Development Kit, SDK) Visual Studio extension development. You can do it in the menu _Tools-\&gt; Get tools and Features_.
2. You can run the extension directly from the source code. Right-click the _Extension_ project and click the _Set as Startup Project_
3. Run the project by clicking the &quot;_Current instance_&quot; button on top with the green arrow next to it. This will launch the other instance of Visual Studio with the extension enabled.
4. Open the solution that you want to migrate
5. Click on the project you want to migrate
6. Click on _Tools-\&gt;Migrate_ project

To change the settings of the extension:

1. Go to _Options-\&gt;NUnitMigrator_
2. Change the option value


1. Click _OK_
