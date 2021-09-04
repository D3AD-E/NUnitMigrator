using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class AssertConstraintTests
    {
        [TestMethod]
        public void TestIsTrue()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(true, Is.True);
        Assert.That(true, Is.True, ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        Assert.IsTrue(true);
        Assert.IsTrue(true, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsNotNull()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(null, Is.Not.Null);
        Assert.That(null, Is.Not.Null, ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        Assert.IsNotNull(null);
        Assert.IsNotNull(null, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsEqualTo()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        string second = null;
        Assert.That(null, Is.EqualTo(second));
        Assert.That(null, Is.EqualTo(second), ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        string second = null;
        Assert.AreEqual(null, second);
        Assert.AreEqual(null, second, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsGreaterOrEqual()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        var second = 2;
        Assert.That(1, Is.GreaterThanOrEqualTo(second));
        Assert.That(1, Is.GreaterThanOrEqualTo(second), ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        var second = 2;
        Assert.IsTrue(1 >= second);
        Assert.IsTrue(1 >= second, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsNegative()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(-1, Is.Negative);
        Assert.That(-1, Is.Negative, ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        Assert.IsTrue(-1 < 0);
        Assert.IsTrue(-1 < 0, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsGreaterThan()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        var second = 2;
        Assert.That(1, Is.GreaterThan(second));
        Assert.That(1, Is.GreaterThan(second), ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        var second = 2;
        Assert.IsTrue(1 > second);
        Assert.IsTrue(1 > second, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsNotEqualTo()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        string second = ""str"";
        Assert.That(null, Is.Not.EqualTo(second));
        Assert.That(null, Is.Not.EqualTo(second), ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public class A
{ 
    void Test()
    {
        string second = ""str"";
        Assert.AreNotEqual(null, second);
        Assert.AreNotEqual(null, second, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
