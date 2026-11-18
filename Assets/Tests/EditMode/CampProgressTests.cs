using System.Collections.Generic;
using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class CampProgressTests
    {
        private static readonly RoleId[] BaseRoles = { RoleId.Warden, RoleId.Arcanist };

        /// <summary>The unlock order decided for the first camp (design/game-concept.md, "野営地").</summary>
        private static CampUnlockRule[] DesignRules()
        {
            return new[]
            {
                new CampUnlockRule(CampFeature.Garo, CampMilestone.RunReturned, 0),
                new CampUnlockRule(CampFeature.DepartureGate, CampMilestone.RunReturned, 0),
                new CampUnlockRule(CampFeature.Forge, CampMilestone.RunReturned, 1),
                new CampUnlockRule(CampFeature.Archive, CampMilestone.EnemyRecorded, 1),
                new CampUnlockRule(CampFeature.HunterRole, CampMilestone.Area1BossDefeated, 1),
                new CampUnlockRule(CampFeature.Watchtower, CampMilestone.Area1BossDefeated, 1),
                new CampUnlockRule(CampFeature.VillageMistCleared, CampMilestone.Area1BossDefeated, 1),
                new CampUnlockRule(CampFeature.OracleRole, CampMilestone.Area2BossDefeated, 1),
                new CampUnlockRule(CampFeature.Wardrobe, CampMilestone.RunReturned, 3),
            };
        }

        private static CampProgress CreateProgress() => new CampProgress(DesignRules(), BaseRoles);

        [Test]
        public void New_OnlyGaroAndGate_AndFirstRun()
        {
            // Arrange / Act
            var progress = CreateProgress();

            // Assert
            Assert.IsTrue(progress.IsUnlocked(CampFeature.Garo));
            Assert.IsTrue(progress.IsUnlocked(CampFeature.DepartureGate));
            Assert.IsFalse(progress.IsUnlocked(CampFeature.Forge));
            Assert.IsFalse(progress.IsUnlocked(CampFeature.Archive));
            Assert.IsTrue(progress.IsFirstRun);
            Assert.AreEqual(0f, progress.Restoration);
        }

        [Test]
        public void New_OnlyBaseRolesUsable()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            List<RoleId> roles = progress.UnlockedRoles();

            // Assert
            CollectionAssert.AreEqual(BaseRoles, roles);
            Assert.IsFalse(progress.IsRoleUnlocked(RoleId.Hunter));
            Assert.IsFalse(progress.IsRoleUnlocked(RoleId.Oracle));
        }

        [Test]
        public void Record_FirstReturn_UnlocksForgeAndEndsFirstRun()
        {
            // Arrange
            var progress = CreateProgress();
            var raised = new List<CampFeature>();
            progress.Unlocked += raised.Add;

            // Act
            List<CampFeature> unlocked = progress.Record(CampMilestone.RunReturned);

            // Assert
            CollectionAssert.AreEqual(new[] { CampFeature.Forge }, unlocked);
            CollectionAssert.AreEqual(new[] { CampFeature.Forge }, raised);
            Assert.IsFalse(progress.IsFirstRun);
        }

        [Test]
        public void Record_FirstEnemy_UnlocksArchive()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            progress.Record(CampMilestone.EnemyRecorded);

            // Assert
            Assert.IsTrue(progress.IsUnlocked(CampFeature.Archive));
        }

        [Test]
        public void Record_Area1Boss_UnlocksHunterWatchtowerAndMistInTableOrder()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            List<CampFeature> unlocked = progress.Record(CampMilestone.Area1BossDefeated);

            // Assert
            CollectionAssert.AreEqual(new[] { CampFeature.HunterRole, CampFeature.Watchtower, CampFeature.VillageMistCleared }, unlocked);
            Assert.IsTrue(progress.IsRoleUnlocked(RoleId.Hunter));
            Assert.IsFalse(progress.IsRoleUnlocked(RoleId.Oracle));
        }

        [Test]
        public void Record_Area2Boss_UnlocksOracle()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            progress.Record(CampMilestone.Area2BossDefeated);

            // Assert
            Assert.IsTrue(progress.IsRoleUnlocked(RoleId.Oracle));
        }

        [Test]
        public void Record_SecondAndThirdReturn_WardrobeOnlyAtThree()
        {
            // Arrange
            var progress = CreateProgress();
            progress.Record(CampMilestone.RunReturned);
            progress.Record(CampMilestone.RunReturned);
            bool afterTwo = progress.IsUnlocked(CampFeature.Wardrobe);

            // Act
            List<CampFeature> unlocked = progress.Record(CampMilestone.RunReturned);

            // Assert
            Assert.IsFalse(afterTwo);
            CollectionAssert.AreEqual(new[] { CampFeature.Wardrobe }, unlocked);
        }

        [Test]
        public void Record_AlreadyUnlocked_DoesNotRaiseAgain()
        {
            // Arrange
            var progress = CreateProgress();
            progress.Record(CampMilestone.EnemyRecorded);
            int raised = 0;
            progress.Unlocked += _ => raised++;

            // Act
            progress.Record(CampMilestone.EnemyRecorded);

            // Assert
            Assert.AreEqual(0, raised);
        }

        [Test]
        public void Restoration_EverythingBack_IsOne()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            progress.Record(CampMilestone.RunReturned, 3);
            progress.Record(CampMilestone.EnemyRecorded);
            progress.Record(CampMilestone.Area1BossDefeated);
            progress.Record(CampMilestone.Area2BossDefeated);

            // Assert
            Assert.AreEqual(1f, progress.Restoration, 1e-5f);
        }

        [Test]
        public void Restoration_OneOfSevenBack_IsOneSeventh()
        {
            // Arrange: 7 features are not there at the start (all but Garo and the gate).
            var progress = CreateProgress();

            // Act
            progress.Record(CampMilestone.RunReturned);

            // Assert
            Assert.AreEqual(1f / 7f, progress.Restoration, 1e-5f);
        }

        [Test]
        public void LoadCounts_RestoresUnlocksWithoutEvents()
        {
            // Arrange
            var saved = CreateProgress();
            saved.Record(CampMilestone.RunReturned);
            saved.Record(CampMilestone.Area1BossDefeated);
            int[] counts = saved.ToCounts();
            var loaded = CreateProgress();
            int raised = 0;
            loaded.Unlocked += _ => raised++;

            // Act
            loaded.LoadCounts(counts);

            // Assert
            Assert.AreEqual(0, raised);
            Assert.IsTrue(loaded.IsUnlocked(CampFeature.Forge));
            Assert.IsTrue(loaded.IsRoleUnlocked(RoleId.Hunter));
            Assert.AreEqual(1, loaded.Count(CampMilestone.RunReturned));
        }

        [Test]
        public void LoadCounts_ZeroAfterProgress_LocksAgain()
        {
            // Arrange
            var progress = CreateProgress();
            progress.Record(CampMilestone.Area1BossDefeated);

            // Act
            progress.LoadCounts(new int[0]);

            // Assert
            Assert.IsFalse(progress.IsRoleUnlocked(RoleId.Hunter));
            Assert.IsTrue(progress.IsUnlocked(CampFeature.Garo));
            Assert.IsTrue(progress.IsFirstRun);
        }

        [Test]
        public void LoadCounts_NullOrShortOrNegative_IsSafe()
        {
            // Arrange
            var progress = CreateProgress();

            // Act
            progress.LoadCounts(null);
            progress.LoadCounts(new[] { -5 });

            // Assert
            Assert.AreEqual(0, progress.Count(CampMilestone.RunReturned));
            Assert.AreEqual(0, progress.Count(CampMilestone.Area2BossDefeated));
        }
    }
}
