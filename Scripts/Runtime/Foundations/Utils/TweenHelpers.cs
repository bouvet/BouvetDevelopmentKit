using UnityEngine;

namespace Bouvet.DevelopmentKit
{
    public static class TweenHelpers
    {
        private static float EaseInOut(float startPosition, float endPosition, float duration)
        {
            if (duration <= 0)
                return startPosition;
            else if (duration >= 1)
                return endPosition;

            return startPosition + (endPosition - startPosition) * duration * duration * (3.0f - 2.0f * duration);
        }

        /// <summary>Tween motion curve using Ease in and Ease out</summary>
        /// <returns>Updated Vector 3 position</returns>
        public static Vector3 EaseInOut(Vector3 startPosition, Vector3 endPosition, float duration)
        {
            if (duration <= 0)
                return startPosition;
            else if (duration >= 1)
                return endPosition;

            return new Vector3(EaseInOut(startPosition.x, endPosition.x, duration), EaseInOut(startPosition.y, endPosition.y, duration), EaseInOut(startPosition.z, endPosition.z, duration));
        }

        public static float QuadraticBezier(float startPosition, float endPosition, float p, float duration)
        {
            if (duration <= 0)
                return startPosition;
            else if (duration >= 1)
                return endPosition;

            float st = duration * duration;
            return startPosition + 2 * p * duration - 2 * startPosition * duration + endPosition * st - 2 * p * st + startPosition * st;
        }

        /// <summary>Tween motion curve using Quadratic Bezier</summary>
        /// <param name="parabola">Changes the animation curve direction, average between startPosition and endPosition will make it linear curve.</param>
        /// <returns>Updated Vector 3 position</returns>
        public static Vector3 QuadraticBezier(Vector3 startPosition, Vector3 endPosition, Vector3 parabola, float duration)
        {
            if (duration <= 0)
                return startPosition;
            else if (duration >= 1)
                return endPosition;

            return new Vector3(
              QuadraticBezier(startPosition.x, endPosition.x, parabola.x, duration),
              QuadraticBezier(startPosition.y, endPosition.y, parabola.y, duration),
              QuadraticBezier(startPosition.z, endPosition.z, parabola.z, duration));
        }
    }
}
