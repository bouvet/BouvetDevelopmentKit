using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
    /// <summary>
    /// Class that represents the different input sources and their relevant values
    /// </summary>
    public class InputSource
    {
        public InputSourceKind inputSourceKind;
        public System.Numerics.Vector3 worldPosition;
        public System.Numerics.Quaternion worldRotation;
        public System.Numerics.Vector3 forwardVector;
        public string message;
        public GameObject collidedObjectIdentifier;
        public float pinchDistance;
        public bool active;
    }
}