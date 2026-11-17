using System.Collections.Generic;
using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class StatusContainerTests
    {
        private const int Capacity = 2;
        private const float Duration = 4f;

        [Test]
        public void Apply_NewStatus_IsHeld()
        {
            // Arrange
            var statuses = new StatusContainer(Capacity);

            // Act
            statuses.Apply(StatusType.Freeze, Duration);

            // Assert
            Assert.IsTrue(statuses.Has(StatusType.Freeze));
            Assert.AreEqual(1, statuses.Count);
        }

        [Test]
        public void Apply_WhenFull_EvictsOldest()
        {
            // Arrange
            var statuses = new StatusContainer(Capacity);
            statuses.Apply(StatusType.Burn, Duration);
            statuses.Apply(StatusType.Freeze, Duration);

            // Act
            statuses.Apply(StatusType.Shock, Duration);

            // Assert
            Assert.IsFalse(statuses.Has(StatusType.Burn));
            Assert.IsTrue(statuses.Has(StatusType.Freeze));
            Assert.IsTrue(statuses.Has(StatusType.Shock));
            Assert.AreEqual(Capacity, statuses.Count);
        }

        [Test]
        public void Apply_ExistingStatus_RefreshesWithoutEvicting()
        {
            // Arrange
            var statuses = new StatusContainer(Capacity);
            statuses.Apply(StatusType.Burn, Duration);
            statuses.Apply(StatusType.Freeze, Duration);
            statuses.Tick(Duration / 2f);

            // Act
            statuses.Apply(StatusType.Burn, Duration);

            // Assert
            Assert.AreEqual(Capacity, statuses.Count);
            Assert.AreEqual(Duration, statuses.RemainingTime(StatusType.Burn), 1e-4f);
            Assert.IsTrue(statuses.Has(StatusType.Freeze));
        }

        [Test]
        public void Tick_PastDuration_RemovesStatus()
        {
            // Arrange
            var statuses = new StatusContainer(Capacity);
            statuses.Apply(StatusType.Gather, Duration);
            var removed = new List<StatusType>();
            statuses.Removed += removed.Add;

            // Act
            statuses.Tick(Duration + 0.01f);

            // Assert
            Assert.IsFalse(statuses.Has(StatusType.Gather));
            CollectionAssert.AreEqual(new[] { StatusType.Gather }, removed);
        }

        [Test]
        public void Tick_BeforeDuration_KeepsStatus()
        {
            // Arrange
            var statuses = new StatusContainer(Capacity);
            statuses.Apply(StatusType.Gather, Duration);

            // Act
            statuses.Tick(Duration - 0.01f);

            // Assert
            Assert.IsTrue(statuses.Has(StatusType.Gather));
        }

        [Test]
        public void SetCapacity_Smaller_EvictsOldestFirst()
        {
            // Arrange
            var statuses = new StatusContainer(3);
            statuses.Apply(StatusType.Burn, Duration);
            statuses.Apply(StatusType.Freeze, Duration);
            statuses.Apply(StatusType.Shock, Duration);

            // Act
            statuses.SetCapacity(1);

            // Assert
            var held = new List<StatusType>();
            statuses.CopyTo(held);
            CollectionAssert.AreEqual(new[] { StatusType.Shock }, held);
        }
    }
}
