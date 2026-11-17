using NUnit.Framework;
using RuneBrigade.Core;

namespace RuneBrigade.Tests
{
    public class PartyScalingTests
    {
        private const float HealthPerExtra = 0.6f;
        private const float CountPerExtra = 0.4f;

        [TestCase(1, 1f)]
        [TestCase(2, 1.6f)]
        [TestCase(4, 2.8f)]
        public void HealthMultiplier_ByPlayers_MatchesFormula(int players, float expected)
        {
            Assert.AreEqual(expected, PartyScaling.HealthMultiplier(players, HealthPerExtra), 1e-5f);
        }

        [TestCase(1, 1f)]
        [TestCase(2, 1.4f)]
        [TestCase(4, 2.2f)]
        public void CountMultiplier_ByPlayers_MatchesFormula(int players, float expected)
        {
            Assert.AreEqual(expected, PartyScaling.CountMultiplier(players, CountPerExtra), 1e-5f);
        }

        [Test]
        public void HealthMultiplier_ZeroPlayers_TreatedAsSolo()
        {
            Assert.AreEqual(1f, PartyScaling.HealthMultiplier(0, HealthPerExtra), 1e-5f);
        }

        [TestCase(5, 1, 5)]
        [TestCase(5, 2, 7)]
        [TestCase(8, 4, 18)]
        [TestCase(0, 4, 0)]
        public void ScaledCount_RoundsHalfUp(int baseCount, int players, int expected)
        {
            Assert.AreEqual(expected, PartyScaling.ScaledCount(baseCount, players, CountPerExtra));
        }
    }
}
