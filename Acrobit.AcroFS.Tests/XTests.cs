using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Gita.GFS.Tests
{
    

    // Assumptions:

    public interface IFoo
    {
        Bar Bar { get; set; }
        string Name { get; set; }
        int Value { get; set; }
        bool DoSomething(string value);
        bool DoSomething(int number, string value);
        string DoSomethingStringy(string value);
        bool TryParse(string value, out string outputValue);
        bool Submit(ref Bar bar);
        int GetCount();
        bool Add(int value);
    }

    public class Bar
    {
        public virtual Baz Baz { get; set; }
        public virtual bool Submit() { return false; }
    }

    public class Baz
    {
        public virtual string Name { get; set; }
    }

    [TestClass]
    public class XTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething("ping")).Returns(true);

            var t = mock.Object.DoSomething("test");
            var t2 = mock.Object.DoSomething("ping");

            mock.Verify(foo => foo.DoSomething("ping"));
            Assert.IsTrue(mock.Object.DoSomething("ping"));
            Assert.IsFalse(mock.Object.DoSomething("asdf"));

        }
    }
}
