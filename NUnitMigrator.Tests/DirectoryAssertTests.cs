using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class DirectoryAssertTests
    {
        [TestMethod]
        public void TestExistsDirectoryInfo()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryInfo source = new DirectoryInfo(""./Test"");
        DirectoryAssert.Exists(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryInfo source = new DirectoryInfo(""./Test"");
        Assert.IsTrue(source.Exists);
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestExistsString()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryAssert.Exists(""./Test"");
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        Assert.IsTrue(Directory.Exists(""./Test""));
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestDoesNotExistsDirectoryInfo()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryInfo source = new DirectoryInfo(""./Test"");
        DirectoryAssert.DoesNotExist(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryInfo source = new DirectoryInfo(""./Test"");
        Assert.IsFalse(source.Exists);
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestDoesNotExistsString()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryAssert.DoesNotExist(""./Test"");
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        Assert.IsFalse(Directory.Exists(""./Test""));
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
