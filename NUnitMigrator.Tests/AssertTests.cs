using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class AssertTests
    {
        [TestMethod]
        public void TestNull()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.Null(null);
        Assert.NotNull(null);
        Assert.Null(null, ""The Message"");
        Assert.NotNull(null, ""The Message"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsNull(null);
        Assert.IsNotNull(null);
        Assert.IsNull(null, ""The Message"");
        Assert.IsNotNull(null, ""The Message"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestBooleanThat()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.That(true);
        Assert.That(true, ""message"");
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
        public void TestLess()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.Less(1, 2);
        Assert.Less(B(), C());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.IsTrue(1 < 2);
        Assert.IsTrue(B() < C());
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestLessOrEqual()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.LessOrEqual(1, 2);
        Assert.LessOrEqual(B(), C());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.IsTrue(1 <= 2);
        Assert.IsTrue(B() <= C());
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestGreater()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.Greater(1, 2);
        Assert.Greater(B(), C());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.IsTrue(1 > 2);
        Assert.IsTrue(B() > C());
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestGreaterOrEqual()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.GreaterOrEqual(1, 2);
        Assert.GreaterOrEqual(B(), C());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    int C() { return 2; }
    void Test()
    {
        Assert.IsTrue(1 >= 2);
        Assert.IsTrue(B() >= C());
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestInstanceOfType()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.IsInstanceOf(typeof(Type), GetType());
        Assert.IsInstanceOf(typeof(Type), GetType(), ""Something"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsInstanceOfType(GetType(), typeof(Type));
        Assert.IsInstanceOfType(GetType(), typeof(Type), ""Something"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestInstanceOfTypeGeneric()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        Assert.IsInstanceOf<Type>(GetType());
        Assert.IsInstanceOf<Type>(GetType(), ""Something"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        Assert.IsInstanceOfType(GetType(), typeof(Type));
        Assert.IsInstanceOfType(GetType(), typeof(Type), ""Something"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestZero()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.Zero(0);
        Assert.Zero(B());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.IsTrue(0 == 0);
        Assert.IsTrue(B() == 0);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsAssignableFrom()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        int a = 0;
        Assert.IsAssignableFrom(typeof(int), a);
        Assert.IsAssignableFrom<int>(a);
        Assert.IsNotAssignableFrom<int>(a);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        int a = 0;
        Assert.IsTrue(a.GetType().IsAssignableFrom(typeof(int)));
        Assert.IsTrue(a.GetType().IsAssignableFrom(typeof(int)));
        Assert.IsFalse(a.GetType().IsAssignableFrom(typeof(int)));
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestThrows()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void B() { throw new OutOfMemoryException(); }
    void Test()
    {
        Assert.Throws(typeof(OutOfMemoryException), B);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void B() { throw new OutOfMemoryException(); }
    void Test()
    {
        Assert.ThrowsException<OutOfMemoryException>(B);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestComment()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.Zero(1, ""This is not zero"");
        Assert.Zero(B(), ""This is not zero"");
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.IsTrue(1 == 0, ""This is not zero"");
        Assert.IsTrue(B() == 0, ""This is not zero"");
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestNotZero()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.NotZero(1);
        Assert.NotZero(B());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.IsTrue(1 != 0);
        Assert.IsTrue(B() != 0);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestPositive()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.Positive(1);
        Assert.Positive(B());
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    int B() { return 1; }
    void Test()
    {
        Assert.IsTrue(1 > 0);
        Assert.IsTrue(B() > 0);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestIsEmpty()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Test()
    {
        string b = ""b"";
        Assert.IsEmpty(b);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        string b = ""b"";
        Assert.IsTrue(string.IsNullOrEmpty(b));
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
public class A
{ 
    void Test()
    {
        string b = ""b"";
        Assert.IsNotEmpty(b);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Test()
    {
        string b = ""b"";
        Assert.IsFalse(string.IsNullOrEmpty(b));
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestCollectionAssert()
        {
            const string input = @"
using NUnit.Framework;
using System.Collections.Generic;
public class A
{ 
    void Test()
    {
        var testMe = new List<string>();
        CollectionAssert.AllItemsAreNotNull(testMe);
    }
}";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
public class A
{ 
    void Test()
    {
        var testMe = new List<string>();
        CollectionAssert.AllItemsAreNotNull(testMe);
    }
}";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
