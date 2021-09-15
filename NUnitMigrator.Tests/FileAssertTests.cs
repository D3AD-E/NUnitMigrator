using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class FileAssertTests
    {
        [TestMethod]
        public void TestExistsFileInfo()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        FileInfo source = new FileInfo(""./Test"");
        FileAssert.Exists(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        FileInfo source = new FileInfo(""./Test"");
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
        FileAssert.Exists(""./Test"");
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        Assert.IsTrue(File.Exists(""./Test""));
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestDoesNotExistsFileInfo()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        FileInfo source = new FileInfo(""./Test"");
        FileAssert.DoesNotExist(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        FileInfo source = new FileInfo(""./Test"");
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
        FileAssert.DoesNotExist(""./Test"");
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
public class A
{
    public void Test()
    {
        Assert.IsFalse(File.Exists(""./Test""));
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
