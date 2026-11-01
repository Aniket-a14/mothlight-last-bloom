namespace Mothlight
{
    using UnityEngine;

    public static class MothlightRules
    {
        public const int PetalsToWake = 3;
        public const int LightPerPetal = 7;
        public const int CarryLimit = 6;
        public const int HitsAllowed = 3;
        public const float NightSeconds = 180f;

        public static int Delivered(int current, int carried)
        {
            if (carried < 0) carried = 0;
            int total = current + carried;
            return total > PetalsToWake * LightPerPetal ? PetalsToWake * LightPerPetal : total;
        }

        public static bool Won(int delivered) { return delivered >= PetalsToWake * LightPerPetal; }
        public static bool Lost(int hits, float timeLeft) { return hits >= HitsAllowed || timeLeft <= 0f; }

        public static Vector2 RemoveOutwardVelocityAtBounds(Vector2 position, Vector2 velocity, float halfWidth, float halfHeight)
        {
            if (position.x <= -halfWidth && velocity.x < 0f || position.x >= halfWidth && velocity.x > 0f) velocity.x = 0f;
            if (position.y <= -halfHeight && velocity.y < 0f || position.y >= halfHeight && velocity.y > 0f) velocity.y = 0f;
            return velocity;
        }
    }
}
