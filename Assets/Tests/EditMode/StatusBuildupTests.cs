using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class StatusBuildupTests
    {
        private const float Threshold = 100f;
        private const float BoltPoints = 25f;
        private const float Decay = 10f;

        [Test]
        public void Add_ReachingThreshold_TriggersAndResets()
        {
            // Arrange
            var buildup = new StatusBuildup(Threshold, Decay);
            buildup.Add(StatusType.Freeze, BoltPoints);
            buildup.Add(StatusType.Freeze, BoltPoints);
            buildup.Add(StatusType.Freeze, BoltPoints);

            // Act
            bool triggered = buildup.Add(StatusType.Freeze, BoltPoints);

            // Assert
            Assert.IsTrue(triggered);
            Assert.AreEqual(0f, buildup.Value(StatusType.Freeze));
        }

        [Test]
        public void Add_BelowThreshold_DoesNotTrigger()
        {
            // Arrange
            var buildup = new StatusBuildup(Threshold, Decay);

            // Act
            bool triggered = buildup.Add(StatusType.Burn, BoltPoints);

            // Assert
            Assert.IsFalse(triggered);
            Assert.AreEqual(BoltPoints, buildup.Value(StatusType.Burn));
        }

        [Test]
        public void Add_SeparateStatuses_HaveSeparateMeters()
        {
            // Arrange
            var buildup = new StatusBuildup(Threshold, Decay);

            // Act
            buildup.Add(StatusType.Burn, BoltPoints);
            buildup.Add(StatusType.Shock, BoltPoints * 2f);

            // Assert
            Assert.AreEqual(BoltPoints, buildup.Value(StatusType.Burn));
            Assert.AreEqual(BoltPoints * 2f, buildup.Value(StatusType.Shock));
        }

        [Test]
        public void Tick_DrainsButNotBelowZero()
        {
            // Arrange
            var buildup = new StatusBuildup(Threshold, Decay);
            buildup.Add(StatusType.Burn, BoltPoints);

            // Act
            buildup.Tick(1f);
            float afterOne = buildup.Value(StatusType.Burn);
            buildup.Tick(100f);

            // Assert
            Assert.AreEqual(BoltPoints - Decay, afterOne, 1e-4f);
            Assert.AreEqual(0f, buildup.Value(StatusType.Burn));
        }
    }
}
