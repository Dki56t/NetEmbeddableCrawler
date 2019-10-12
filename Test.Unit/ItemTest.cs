using System;
using Crawler.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Unit
{
    [TestClass]
    public class ItemTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestParentOnlyOne()
        {
            var parent1 = new Item("", "");
            var parent2 = new Item("", "");
            var child = new Item("", "");

            parent1.AddItem(child);
            parent2.AddItem(child);
        }

        [TestMethod]
        public void TestGetRoot()
        {
            var parent1 = new Item("", "");
            var parent2 = new Item("", "");
            var parent3 = new Item("", "");

            parent1.AddItem(parent2);
            parent2.AddItem(parent3);

            Assert.AreEqual(parent3.GetRoot(), parent1);
        }
    }
}