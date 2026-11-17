using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class TimedResourceTests
    {
        private const int DashMax = 2;
        private const float DashRecharge = 3f;
        private const float DashInvulnerable = 0.2f;
        private const float CooldownSeconds = 12f;

        [Test]
        public void TryDash_NoCharges_IsRefused()
        {
            // Arrange
            var dash = new DashCharges(DashMax, DashRecharge, DashInvulnerable);
            dash.TryDash();
            dash.TryDash();

            // Act / Assert
            Assert.IsFalse(dash.TryDash());
        }

        [Test]
        public void Tick_OneRecharge_RestoresOneCharge()
        {
            // Arrange
            var dash = new DashCharges(DashMax, DashRecharge, DashInvulnerable);
            dash.TryDash();
            dash.TryDash();

            // Act
            dash.Tick(DashRecharge + 0.01f);

            // Assert
            Assert.AreEqual(1, dash.Charges);
        }

        [Test]
        public void TryDash_GrantsInvulnerabilityThatEnds()
        {
            // Arrange
            var dash = new DashCharges(DashMax, DashRecharge, DashInvulnerable);

            // Act
            dash.TryDash();
            bool during = dash.IsInvulnerable;
            dash.Tick(DashInvulnerable + 0.01f);

            // Assert
            Assert.IsTrue(during);
            Assert.IsFalse(dash.IsInvulnerable);
        }

        [Test]
        public void Cooldown_TicksToReady()
        {
            // Arrange
            var cooldown = new Cooldown(CooldownSeconds);
            cooldown.TryUse();

            // Act
            bool readyEarly = cooldown.IsReady;
            cooldown.Tick(CooldownSeconds);

            // Assert
            Assert.IsFalse(readyEarly);
            Assert.IsTrue(cooldown.IsReady);
        }

        [Test]
        public void Cooldown_TryUseWhileCooling_IsRefused()
        {
            // Arrange
            var cooldown = new Cooldown(CooldownSeconds);
            cooldown.TryUse();

            // Act / Assert
            Assert.IsFalse(cooldown.TryUse());
        }

        [Test]
        public void MeleeComboChain_PressesInWindow_ReachFinisherThenLoop()
        {
            // Arrange
            const float interval = 0.3f;
            const float window = 0.8f;
            var chain = new MeleeComboChain(3, interval, window);

            // Act
            chain.TryAttack(out int first);
            chain.Tick(interval);
            chain.TryAttack(out int second);
            chain.Tick(interval);
            chain.TryAttack(out int third);
            chain.Tick(interval);
            chain.TryAttack(out int fourth);

            // Assert
            Assert.AreEqual(0, first);
            Assert.AreEqual(1, second);
            Assert.AreEqual(2, third);
            Assert.IsTrue(chain.IsFinisher(third));
            Assert.AreEqual(0, fourth);
        }

        [Test]
        public void MeleeComboChain_PauseBeyondWindow_RestartsChain()
        {
            // Arrange
            const float interval = 0.3f;
            const float window = 0.8f;
            var chain = new MeleeComboChain(3, interval, window);
            chain.TryAttack(out _);

            // Act
            chain.Tick(window + 0.01f);
            chain.TryAttack(out int step);

            // Assert
            Assert.AreEqual(0, step);
        }

        [Test]
        public void MeleeComboChain_PressDuringRecovery_IsRefused()
        {
            // Arrange
            var chain = new MeleeComboChain(3, 0.3f, 0.8f);
            chain.TryAttack(out _);

            // Act
            bool swung = chain.TryAttack(out int step);

            // Assert
            Assert.IsFalse(swung);
            Assert.AreEqual(-1, step);
        }

        [Test]
        public void UltimateGauge_FullThenConsume_Empties()
        {
            // Arrange
            var gauge = new UltimateGauge(100f);
            gauge.Add(150f);

            // Act
            bool fired = gauge.TryConsume();

            // Assert
            Assert.IsTrue(fired);
            Assert.AreEqual(0f, gauge.Value);
        }

        [Test]
        public void DamageCalculator_MultipliesAllFactors()
        {
            // 150 base × 3 weak point × 1.5 switch strike × 5 shatter = 3375
            Assert.AreEqual(3375f, DamageCalculator.Compute(150f, 3f, 1.5f, 5f), 1e-2f);
        }
    }
}
