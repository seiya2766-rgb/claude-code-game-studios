using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class ComboResolverTests
    {
        private const float Duration = 4f;
        private const float AnnihilateMultiplier = 4f;
        private const float ShatterMultiplier = 5f;
        private const float DetonateMultiplier = 1f;
        private const float DetonateDamage = 120f;

        private static ComboResolver CreatePrototypeResolver()
        {
            return new ComboResolver(new[]
            {
                new ComboRule("annihilate", "殲滅", new[] { StatusType.Gather }, new[] { AttackTag.Meteor }, AnnihilateMultiplier),
                new ComboRule("shatter", "砕氷", new[] { StatusType.Freeze }, new[] { AttackTag.WardenFinisher, AttackTag.ChargedShot }, ShatterMultiplier),
                new ComboRule("detonate", "爆雷", new[] { StatusType.Burn, StatusType.Shock }, null, DetonateMultiplier, DetonateDamage),
            });
        }

        [Test]
        public void TryResolveHit_MatchingStatusAndTag_FiresAndConsumes()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Gather, Duration);

            // Act
            bool fired = resolver.TryResolveHit(target, AttackTag.Meteor, out ComboRule rule);

            // Assert
            Assert.IsTrue(fired);
            Assert.AreEqual("annihilate", rule.Id);
            Assert.IsFalse(target.Has(StatusType.Gather));
        }

        [Test]
        public void TryResolveHit_WrongTag_DoesNotFire()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Freeze, Duration);

            // Act
            bool fired = resolver.TryResolveHit(target, AttackTag.ElementBolt, out ComboRule rule);

            // Assert
            Assert.IsFalse(fired);
            Assert.IsNull(rule);
            Assert.IsTrue(target.Has(StatusType.Freeze));
        }

        [Test]
        public void TryResolveHit_MissingStatus_DoesNotFire()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);

            // Act
            bool fired = resolver.TryResolveHit(target, AttackTag.WardenFinisher, out _);

            // Assert
            Assert.IsFalse(fired);
        }

        [Test]
        public void TryResolveHit_AnyListedTrigger_Fires()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Freeze, Duration);

            // Act
            bool fired = resolver.TryResolveHit(target, AttackTag.ChargedShot, out ComboRule rule);

            // Assert
            Assert.IsTrue(fired);
            Assert.AreEqual("shatter", rule.Id);
        }

        [Test]
        public void TryResolveHit_StatusTriggeredRule_IsIgnored()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Burn, Duration);
            target.Apply(StatusType.Shock, Duration);

            // Act
            bool fired = resolver.TryResolveHit(target, AttackTag.Generic, out _);

            // Assert
            Assert.IsFalse(fired);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void TryResolveStatus_BothStatusesPresent_FiresAndConsumesBoth()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Burn, Duration);
            target.Apply(StatusType.Shock, Duration);

            // Act
            bool fired = resolver.TryResolveStatus(target, out ComboRule rule);

            // Assert
            Assert.IsTrue(fired);
            Assert.AreEqual("detonate", rule.Id);
            Assert.AreEqual(DetonateDamage, rule.BaseDamage);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void TryResolveStatus_OnlyOneStatus_DoesNotFire()
        {
            // Arrange
            var resolver = CreatePrototypeResolver();
            var target = new StatusContainer(2);
            target.Apply(StatusType.Burn, Duration);

            // Act
            bool fired = resolver.TryResolveStatus(target, out _);

            // Assert
            Assert.IsFalse(fired);
            Assert.IsTrue(target.Has(StatusType.Burn));
        }

        [Test]
        public void TryResolveHit_TwoRulesMatch_FirstInTableWins()
        {
            // Arrange
            var resolver = new ComboResolver(new[]
            {
                new ComboRule("first", "first", new[] { StatusType.Freeze }, new[] { AttackTag.Meteor }, ShatterMultiplier),
                new ComboRule("second", "second", new[] { StatusType.Freeze }, new[] { AttackTag.Meteor }, AnnihilateMultiplier),
            });
            var target = new StatusContainer(2);
            target.Apply(StatusType.Freeze, Duration);

            // Act
            resolver.TryResolveHit(target, AttackTag.Meteor, out ComboRule rule);

            // Assert
            Assert.AreEqual("first", rule.Id);
        }
    }
}
