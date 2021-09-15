using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class AssertConstraintTests
    {
        [TestMethod]
        public void TestIsNaN()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        double i = new double();
        Assert.That(5, Is.NaN);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        double i = new double();
        Assert.IsTrue(double.IsNaN(5));
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        [TestMethod]
        public void TestDoesExistDirectoryInfo()
        {
            const string input = @"
using NUnit.Framework;
using System.IO;
public class A
{
    public void Test()
    {
        DirectoryInfo source = new DirectoryInfo(""./Test"");
        Assert.That(source, Does.Exist);
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
        public void TestIsNotTrue()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(true, Is.Not.True);
        Assert.That(true, Is.Not.True, ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsFalse(true);
        Assert.IsFalse(true, ""message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsInstanceOf()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(""Hello"", Is.InstanceOf(typeof(string)));
        Assert.That(""Hello"", Is.InstanceOf<string>());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsInstanceOfType(""Hello"", typeof(string));
        Assert.IsInstanceOfType(""Hello"", typeof(string));
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestDoesStartWith()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(""Hello"", Does.StartWith(""He"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        StringAssert.StartsWith(""Hello"", ""He"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestStringContains()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(""Hello"", Does.Contain(""He"");
        Assert.That(""Hello"", Does.Not.Contain(""He"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsTrue(""Hello"".Contains(""He""));
        Assert.IsFalse(""Hello"".Contains(""He""));
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
        public void TestIsNotNullOther()
        {
            const string input = @"
using NUnit.Framework;
using System.Collections.Generic;
public class A
{ 
    void Test()
    {
        var b = new List<string>();
        Assert.That(b, Is.All.Not.Null);
        Assert.That(b, Is.All.Not.Null, ""message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
public class A
{ 
    void Test()
    {
        var b = new List<string>();
        CollectionAssert.AllItemsAreNotNull(b);
        CollectionAssert.AllItemsAreNotNull(b, ""message"");
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
