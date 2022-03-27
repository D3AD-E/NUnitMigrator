using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class OptionsTests
    {
        [TestMethod]
        public void TestCommentUnsupported()
        {
            const string input = @"
using NUnit.Framework;
using System.Collections.Generic;
public class A
{
    [Test]
    public void Test()
    {
        var testme = new List<int>();
        CollectionAssert.IsOrdered(testme);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
public class A
{
    [TestMethod]
    public void Test()
    {
        var testme = new List<int>();
        /*CollectionAssert.IsOrdered(testme)*/;
    }
}";
            var actual = TestSupport.RunTest(input, new Core.RewriterLogic.Data.RewriterOptions { CommentUnsupported = true });
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count > 0);
        }

        [TestMethod]
        public void TestDoNotCommentUnsupported()
        {
            const string input = @"
using NUnit.Framework;
using System.Collections.Generic;
public class A
{
    [Test]
    public void Test()
    {
        var testme = new List<int>();
        CollectionAssert.IsOrdered(testme);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
public class A
{
    [TestMethod]
    public void Test()
    {
        var testme = new List<int>();
        CollectionAssert.IsOrdered(testme);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count > 0);
        }
    }
}
