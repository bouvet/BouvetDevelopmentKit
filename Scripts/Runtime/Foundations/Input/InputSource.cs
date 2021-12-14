using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
    /// <summary>
    /// Class that represents the different input sources and their relevant values
    /// </summary>
    public class InputSource
    {
        public InputSourceKind inputSourceKind;
        public Vector3 worldPosition;
        public Quaternion worldRotation;
        public Vector3 forwardVector;
        public string message;
        public GameObject collidedObject;
        public float pinchDistance;
        public bool active;
    }
}