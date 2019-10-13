using System;
using Crawler.Logic;
using Xunit;

namespace Tests.UnitTests
{
    public class ItemTests
    {
        [Fact]
        public void ShouldGetRoot()
        {
            var parent1 = new Item("", "");
            var parent2 = new Item("", "");
            var parent3 = new Item("", "");

            parent1.AddItem(parent2);
            parent2.AddItem(parent3);

            Assert.Equal(parent3.GetRoot(), parent1);
        }

        [Fact]
        public void ShouldThrowsIfCountOfParentsMoreThanOne()
        {
            var parent1 = new Item("", "");
            var parent2 = new Item("", "");
            var child = new Item("", "");

            parent1.AddItem(child);
            Assert.Throws<InvalidOperationException>(() => parent2.AddItem(child));
        }
    }
}