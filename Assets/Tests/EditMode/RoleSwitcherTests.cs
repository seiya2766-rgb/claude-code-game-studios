using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class RoleSwitcherTests
    {
        private const float SwitchDuration = 0.4f;
        private const float StrikeWindow = 2f;
        private const float StrikeBonus = 0.5f;
        private static readonly RoleId[] PrototypeRoles = { RoleId.Warden, RoleId.Arcanist };

        private static RoleSwitcher CreateSwitcher()
        {
            return new RoleSwitcher(PrototypeRoles, RoleId.Warden, SwitchDuration, StrikeWindow, StrikeBonus);
        }

        [Test]
        public void TrySwitch_AvailableRole_StartsSwitching()
        {
            // Arrange
            var switcher = CreateSwitcher();

            // Act
            bool started = switcher.TrySwitch(RoleId.Arcanist);

            // Assert
            Assert.IsTrue(started);
            Assert.IsTrue(switcher.IsSwitching);
            Assert.AreEqual(RoleId.Arcanist, switcher.Current);
            Assert.AreEqual(RoleId.Warden, switcher.Previous);
        }

        [Test]
        public void TrySwitch_UnavailableRole_IsRefused()
        {
            // Arrange
            var switcher = CreateSwitcher();

            // Act
            bool started = switcher.TrySwitch(RoleId.Hunter);

            // Assert
            Assert.IsFalse(started);
            Assert.AreEqual(RoleId.Warden, switcher.Current);
        }

        [Test]
        public void TrySwitch_CurrentRole_IsRefused()
        {
            // Arrange
            var switcher = CreateSwitcher();

            // Act / Assert
            Assert.IsFalse(switcher.TrySwitch(RoleId.Warden));
        }

        [Test]
        public void TrySwitch_DuringSwitch_IsRefused()
        {
            // Arrange
            var switcher = CreateSwitcher();
            switcher.TrySwitch(RoleId.Arcanist);

            // Act / Assert
            Assert.IsFalse(switcher.TrySwitch(RoleId.Warden));
        }

        [Test]
        public void ConsumeStrikeMultiplier_AfterSwitchCompletes_GivesBonusOnce()
        {
            // Arrange
            var switcher = CreateSwitcher();
            switcher.TrySwitch(RoleId.Arcanist);
            switcher.Tick(SwitchDuration + 0.01f);

            // Act
            float first = switcher.ConsumeStrikeMultiplier();
            float second = switcher.ConsumeStrikeMultiplier();

            // Assert
            Assert.AreEqual(1f + StrikeBonus, first, 1e-5f);
            Assert.AreEqual(1f, second, 1e-5f);
        }

        [Test]
        public void ConsumeStrikeMultiplier_DuringSwitch_GivesNoBonus()
        {
            // Arrange
            var switcher = CreateSwitcher();
            switcher.TrySwitch(RoleId.Arcanist);

            // Act
            float multiplier = switcher.ConsumeStrikeMultiplier();

            // Assert
            Assert.AreEqual(1f, multiplier, 1e-5f);
        }

        [Test]
        public void ConsumeStrikeMultiplier_AfterWindowExpires_GivesNoBonus()
        {
            // Arrange
            var switcher = CreateSwitcher();
            switcher.TrySwitch(RoleId.Arcanist);
            switcher.Tick(SwitchDuration + 0.01f);
            switcher.Tick(StrikeWindow + 0.01f);

            // Act
            float multiplier = switcher.ConsumeStrikeMultiplier();

            // Assert
            Assert.AreEqual(1f, multiplier, 1e-5f);
        }

        [Test]
        public void ConsumeStrikeMultiplier_BeforeAnySwitch_GivesNoBonus()
        {
            // Arrange
            var switcher = CreateSwitcher();

            // Act / Assert
            Assert.AreEqual(1f, switcher.ConsumeStrikeMultiplier(), 1e-5f);
        }

        [Test]
        public void Tick_SwitchDuration_RaisesCompletedOnce()
        {
            // Arrange
            var switcher = CreateSwitcher();
            int completed = 0;
            switcher.SwitchCompleted += _ => completed++;
            switcher.TrySwitch(RoleId.Arcanist);

            // Act
            switcher.Tick(SwitchDuration + 0.01f);
            switcher.Tick(SwitchDuration);

            // Assert
            Assert.AreEqual(1, completed);
            Assert.IsFalse(switcher.IsSwitching);
        }
    }
}
