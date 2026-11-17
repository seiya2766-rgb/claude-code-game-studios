using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class HealthPoolTests
    {
        private const float BaseMax = 500f;
        private const float WardenMultiplier = 1.5f;
        private const float ArcanistMultiplier = 0.8f;

        [Test]
        public void SetMultiplier_KeepsHealthFraction()
        {
            // Arrange
            var health = new HealthPool(BaseMax, WardenMultiplier);
            health.Damage(health.Max / 2f);

            // Act
            health.SetMultiplier(ArcanistMultiplier);

            // Assert
            Assert.AreEqual(0.5f, health.Fraction, 1e-5f);
            Assert.AreEqual(BaseMax * ArcanistMultiplier / 2f, health.Current, 1e-3f);
        }

        [Test]
        public void Damage_MoreThanCurrent_StopsAtZeroAndRaisesDiedOnce()
        {
            // Arrange
            var health = new HealthPool(BaseMax);
            int died = 0;
            health.Died += () => died++;

            // Act
            float taken = health.Damage(BaseMax * 2f);
            health.Damage(BaseMax);

            // Assert
            Assert.AreEqual(BaseMax, taken, 1e-3f);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(1, died);
        }

        [Test]
        public void Heal_AboveMax_IsCapped()
        {
            // Arrange
            var health = new HealthPool(BaseMax);
            health.Damage(BaseMax / 10f);

            // Act
            float healed = health.Heal(BaseMax);

            // Assert
            Assert.AreEqual(BaseMax / 10f, healed, 1e-3f);
            Assert.AreEqual(health.Max, health.Current, 1e-3f);
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            // Arrange
            var health = new HealthPool(BaseMax);
            health.Damage(BaseMax);

            // Act
            float healed = health.Heal(BaseMax);

            // Assert
            Assert.AreEqual(0f, healed);
            Assert.IsTrue(health.IsDead);
        }
    }
}
