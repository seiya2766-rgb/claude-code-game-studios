using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class AttackTokenPoolTests
    {
        private const int MeleeTokens = 3;
        private const int RangedTokens = 2;

        [Test]
        public void TryAcquire_BeyondCapacity_IsRefused()
        {
            // Arrange
            var pool = new AttackTokenPool(MeleeTokens, RangedTokens);
            for (int id = 0; id < MeleeTokens; id++) pool.TryAcquire(id, AttackKind.Melee);

            // Act
            bool acquired = pool.TryAcquire(MeleeTokens, AttackKind.Melee);

            // Assert
            Assert.IsFalse(acquired);
            Assert.AreEqual(MeleeTokens, pool.InUse(AttackKind.Melee));
        }

        [Test]
        public void TryAcquire_MeleeFull_RangedStillAvailable()
        {
            // Arrange
            var pool = new AttackTokenPool(MeleeTokens, RangedTokens);
            for (int id = 0; id < MeleeTokens; id++) pool.TryAcquire(id, AttackKind.Melee);

            // Act
            bool acquired = pool.TryAcquire(100, AttackKind.Ranged);

            // Assert
            Assert.IsTrue(acquired);
        }

        [Test]
        public void Release_FreesTokenForAnother()
        {
            // Arrange
            var pool = new AttackTokenPool(1, RangedTokens);
            pool.TryAcquire(1, AttackKind.Melee);

            // Act
            pool.Release(1);
            bool acquired = pool.TryAcquire(2, AttackKind.Melee);

            // Assert
            Assert.IsTrue(acquired);
            Assert.IsFalse(pool.Holds(1));
        }

        [Test]
        public void Release_Twice_DoesNotGoNegative()
        {
            // Arrange
            var pool = new AttackTokenPool(MeleeTokens, RangedTokens);
            pool.TryAcquire(1, AttackKind.Melee);

            // Act
            pool.Release(1);
            pool.Release(1);

            // Assert
            Assert.AreEqual(0, pool.InUse(AttackKind.Melee));
        }

        [Test]
        public void TryAcquire_AlreadyHolding_DoesNotTakeSecondToken()
        {
            // Arrange
            var pool = new AttackTokenPool(MeleeTokens, RangedTokens);
            pool.TryAcquire(1, AttackKind.Melee);

            // Act
            bool again = pool.TryAcquire(1, AttackKind.Melee);

            // Assert
            Assert.IsTrue(again);
            Assert.AreEqual(1, pool.InUse(AttackKind.Melee));
        }
    }
}
