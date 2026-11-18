using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class DiscoverySetTests
    {
        [Test]
        public void Discover_FirstTime_ReturnsTrueThenFalse()
        {
            // Arrange
            var set = new DiscoverySet();

            // Act
            bool first = set.Discover("shatter");
            bool second = set.Discover("shatter");

            // Assert
            Assert.IsTrue(first);
            Assert.IsFalse(second);
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void Discover_BlankId_IsIgnored()
        {
            // Arrange
            var set = new DiscoverySet();

            // Act / Assert
            Assert.IsFalse(set.Discover(""));
            Assert.IsFalse(set.Discover(null));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void Load_SkipsDuplicatesAndKeepsOrder()
        {
            // Arrange
            var set = new DiscoverySet();

            // Act
            set.Load(new[] { "detonate", "shatter", "detonate", "" });

            // Assert
            CollectionAssert.AreEqual(new[] { "detonate", "shatter" }, set.Ids);
            Assert.IsTrue(set.Contains("shatter"));
        }
    }
}
