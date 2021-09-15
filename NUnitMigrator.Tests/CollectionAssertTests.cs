using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class CollectionAssertTests
    {
        [TestMethod]
        public void TestIsEmpty()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
public class A
{
    public void Test()
    {
        var source = new List<int>();
        CollectionAssert.IsEmpty(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
public class A
{
    public void Test()
    {
        var source = new List<int>();
        Assert.IsTrue(source.Count == 0);
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestIsNotEmpty()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
public class A
{
    public void Test()
    {
        var source = new List<int>();
        CollectionAssert.IsNotEmpty(source);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
public class A
{
    public void Test()
    {
        var source = new List<int>();
        Assert.IsFalse(source.Count == 0);
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
