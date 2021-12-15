using UnityEngine;

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    public static class TypeHelpers
    {
        /// <summary>
        /// Converts a System.Numerics.Vector3 to a UnityEngine.Vector3
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 MakeUnityVector3(System.Numerics.Vector3 position)
        {
            return new Vector3(position.X, position.Y, -position.Z);
        }

        /// <summary>
        /// Converts a System.Numerics.Quaternion to a UnityEngine.Quaternion
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static Quaternion MakeUnityQuaternion(System.Numerics.Quaternion orientation)
        {
            return new Quaternion(-orientation.X, -orientation.Y, orientation.Z, orientation.W);
        }

        /// <summary>
        /// Converts a UnityEngine.Vector3 to a System.Numerics.Vector3
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static System.Numerics.Vector3 MakeSystemVector3(Vector3 position)
        {
            return new System.Numerics.Vector3(position.x, position.y, -position.z);
        }

        /// <summary>
        /// Converts a UnityEngine.Quaternion to a System.Numerics.Quaternion
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns></returns>v
        public static System.Numerics.Quaternion MakeSystemQuaternion(Quaternion orientation)
        {
            return new System.Numerics.Quaternion(-orientation.x, -orientation.y, orientation.z, orientation.w);
        }


        /// <summary>
        /// Maps a vector 3 to the XZ plane i.e. ignores the Y value.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>v
        public static Vector3 XZ(this Vector3 vector) => Vector3.Scale(vector, Vector3.forward + Vector3.right);
    }
}