using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnitMigrator.Core.RewriterLogic;
using System;
using System.Collections.Generic;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class AttributesTests
    {
        [TestMethod]
        public void TestConvertAttributes()
        {
            const string input = @"
using NUnit.Framework;
using System;
[Serializable]
public class A
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
[Serializable]
public class A
{
    [TestInitialize]
    public void Setup()
    {
    }

    [TestMethod]
    public void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestAuthor()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [Author(""Name"")]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    [Owner(""Name"")]
    public void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }
        [TestMethod]
        public void TestAuthorMoreArguments()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [Author(""Name"", ""Email"")]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    //""Email""
    [Owner(""Name"")]
    public void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }

        [TestMethod]
        public void TestRemoval()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [Retry(1)]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    public void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }

        [TestMethod]
        public void TestRemovalHarder()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [Author(""Name"")]
    [Retry(1)]
    [Test]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    [Owner(""Name"")]
    [TestMethod]
    public void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }

        [TestMethod]
        public void TestCulture()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [SetUICulture(""CultureTest"")]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    public void Test()
    {
        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(""CultureTest"");
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }


        [TestMethod]
        public void TestCultureHard()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [SetUICulture(""CultureTest"")]
    public void Test()
    {
        int a = 0;
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    public void Test()
    {
        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(""CultureTest"");
        int a = 0;
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }

        [TestMethod]
        public void TestStaticMethod()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [OneTimeSetUp]
    public void Test()
    {

    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    [ClassInitialize]
    public static void Test()
    {

    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);

        }

        [TestMethod]
        public void TestUnsupported()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    [RequiresThread]
    public void Test()
    {

    }
}";

            var expected = TestSupport.RunTest(input);
            Assert.IsTrue(expected.Errors.Count > 0);
        }

        [TestMethod]
        public void TestCase()
        {
            const string input = @"
using NUnit.Framework;
[TestFixture]
public class A
{ 
    [TestCase(1,2,3)]
    void Test(int a, int b, int c) 
    {

    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    [DataRow(1,2,3)]
    void Test(int a, int b, int c) 
    {

    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestCaseComplicated()
        {
            const string input = @"
using NUnit.Framework;
[TestFixture]
public class A
{ 
    [TestCase(1, 2, TestOf = typeof(string))]
    void Test(int a, int b, int c) 
    {

    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    [DataRow(1,2)]
    [Description(""typeof(string)"")]
    void Test(int a, int b, int c) 
    {

    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestCaseSource()
        {
            const string input = @"
using NUnit.Framework;
[TestFixture]
public class A
{ 
    [TestCaseSource(""Method"")]
    [TestCaseSource(nameof(Method))]
    [TestCaseSource(typeof(A), ""Method"")]
    void Test()
    {

    }

    IEnumerable<object[]> Method() { return null; }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    [DynamicData(""Method"")]
    [DynamicData(""Method"")]
    [DynamicData(""Method"", typeof(A))]
    void Test()
    {

    }

    IEnumerable<object[]> Method() { return null; }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
