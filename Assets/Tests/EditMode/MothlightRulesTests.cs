using NUnit.Framework;
using UnityEngine;

namespace Mothlight.Tests
{
    public sealed class MothlightRulesTests
    {
        [TestCase(0, 6, 6)]
        [TestCase(6, 6, 12)]
        [TestCase(20, 6, 21)]
        [TestCase(21, 6, 21)]
        [TestCase(4, -2, 4)]
        public void DeliveryIsBoundedAndNeverNegative(int current, int carried, int expected)
        { Assert.That(MothlightRules.Delivered(current, carried), Is.EqualTo(expected)); }

        [Test]
        public void WinRequiresAllThreePetals()
        { Assert.That(MothlightRules.Won(20), Is.False); Assert.That(MothlightRules.Won(21), Is.True); }

        [Test]
        public void LossComesFromThreeHitsOrTheEndOfNight()
        { Assert.That(MothlightRules.Lost(2, 1), Is.False); Assert.That(MothlightRules.Lost(3, 1), Is.True); Assert.That(MothlightRules.Lost(0, 0), Is.True); }

        [TestCase(-7.55f, 0f, -3f, 2f, 0f, 2f)]
        [TestCase(7.55f, 0f, 3f, -2f, 0f, -2f)]
        [TestCase(0f, -4f, 2f, -3f, 2f, 0f)]
        [TestCase(0f, 4f, -2f, 3f, -2f, 0f)]
        public void BoundsStopOnlyVelocityPushingOutward(float x, float y, float vx, float vy, float expectedX, float expectedY)
        {
            var result = MothlightRules.RemoveOutwardVelocityAtBounds(new Vector2(x, y), new Vector2(vx, vy), 7.55f, 4f);
            Assert.That(result.x, Is.EqualTo(expectedX)); Assert.That(result.y, Is.EqualTo(expectedY));
        }

        [Test]
        public void CornerCollisionStopsBothBlockedAxesButKeepsInwardInputAvailable()
        {
            var outward = MothlightRules.RemoveOutwardVelocityAtBounds(new Vector2(7.55f, 4f), new Vector2(4f, 3f), 7.55f, 4f);
            Assert.That(outward, Is.EqualTo(Vector2.zero));
            var inward = MothlightRules.RemoveOutwardVelocityAtBounds(new Vector2(7.55f, 4f), new Vector2(-4f, -3f), 7.55f, 4f);
            Assert.That(inward, Is.EqualTo(new Vector2(-4f, -3f)));
        }
    }
}
