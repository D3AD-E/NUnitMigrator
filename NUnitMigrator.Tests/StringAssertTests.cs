using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NUnitMigrator.Tests
{
    [TestClass]
    public class StringAssertTests
    {
        [TestMethod]
        public void MatchesTest()
        {
            const string input = @"
using NUnit.Framework;
public class A
{
    public void Test()
    {
        string a = ""*"";
        string b = ""b"";
        StringAssert.IsMatch(a, b);
    }
}";

            const string expected = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
public class A
{
    public void Test()
    {
        string a = ""*"";
        string b = ""b"";
        StringAssert.Matches(b, new System.Text.RegularExpressions.Regex(a));
    }
}";

            var actual = TestSupport.RunTest(input);
            Assert.AreEqual(expected, actual.Text);
            Assert.IsTrue(actual.Errors.Count == 0);
        }
    }
}
