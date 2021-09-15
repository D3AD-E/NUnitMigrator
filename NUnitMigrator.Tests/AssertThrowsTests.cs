using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class AssertThrowsTests
    {

        [TestMethod]
        public void TestWithProperty()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    string Dummy() { return ""ParamName""; }
    void Test()
    {
        Assert.That(() => Dummy(), Throws.TypeOf<ArgumentException>().With.Property(""ParamName"").Contains(""arg0""));
        Assert.That(() => Dummy(), Throws.TypeOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).Contains(""arg0""));
    }
}
";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    string Dummy() { return ""ParamName""; }
    void Test()
    {
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Dummy()).ParamName,""arg0"");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Dummy()).ParamName,""arg0"");
    }
}
";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestWithMessage()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.Contains(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.StartsWith(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.StartWith(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.EndsWith(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.EndWith(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.Match(""the message""));
        Assert.That(() => Dummy(), Throws.TypeOf<InvalidOperationException>().With.Message.Matches(""the message""));
        Assert.That(() => Dummy(), Throws.Exception.Message.Contains(""the message""));
    }
}
";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        StringAssert.StartsWith(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        StringAssert.StartsWith(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        StringAssert.EndsWith(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        StringAssert.EndsWith(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        Assert.AreEqual(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,""the message"");
        StringAssert.Matches(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,new System.Text.RegularExpressions.Regex(""the message""));
        StringAssert.Matches(Assert.ThrowsException<InvalidOperationException>(() => Dummy()).Message,new System.Text.RegularExpressions.Regex(""the message""));
        StringAssert.Contains(Assert.ThrowsException<Exception>(() => Dummy()).Message,""the message"");
    }
}
";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestThrowsMoreArgs()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.That(Dummy, Throws.ArgumentNullException, ""message {0}"", 1);
    }
}
";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.ThrowsException<ArgumentNullException>(Dummy,""message {0}"",1);
    }
}
";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestThrowsTypeOfStaticAccessor()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.That(Dummy, Throws.ArgumentNullException);
        Assert.That(() => { int i = 0; i++; }, Throws.ArgumentNullException);
        Assert.That(() => Dummy(), Throws.ArgumentNullException);
    }
}
";
            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.ThrowsException<ArgumentNullException>(Dummy);
        Assert.ThrowsException<ArgumentNullException>(() => { int i = 0; i++; });
        Assert.ThrowsException<ArgumentNullException>(() => Dummy());
    }
}
";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }

        [TestMethod]
        public void TestThrowsTypeOf()
        {
            const string input = @"
using NUnit.Framework;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.That(Dummy, Throws.ArgumentNullException);
        Assert.That(() => { int i = 0; i++; }, Throws.ArgumentNullException);
        Assert.That(() => Dummy(), Throws.ArgumentNullException);
        Assert.That(() => Dummy(), Throws.Exception.TypeOf<OutOfMemoryException>());
        Assert.That(() => Dummy(), Throws.TypeOf<OutOfMemoryException>());
    }
}
";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{ 
    void Dummy() { }
    void Test()
    {
        Assert.ThrowsException<ArgumentNullException>(Dummy);
        Assert.ThrowsException<ArgumentNullException>(() => { int i = 0; i++; });
        Assert.ThrowsException<ArgumentNullException>(() => Dummy());
        Assert.ThrowsException<OutOfMemoryException>(() => Dummy());
        Assert.ThrowsException<OutOfMemoryException>(() => Dummy());
    }
}
";
            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
        
    }
}
