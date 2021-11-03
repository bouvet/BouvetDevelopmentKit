using UnityEngine;

namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
    internal class InteractionBeamCursor : MonoBehaviour
    {
        [SerializeField]
        private Transform cursorAnchor; // Main object on the cursor. Used to position and scale the cursor correctly

        [SerializeField]
        private GameObject defaultCursor; // Cursor default model 

        [SerializeField]
        private GameObject pinchingCursor; // Cursor pinching model

        [SerializeField]
        private GameObject movingUpDown; // Cursor moving up or down model

        [SerializeField]
        private GameObject movingLeftRight; // Cursor moving left or right model

        private bool currentlyPinching;

        public void UpdateCursor(Vector3 position, Quaternion rotation, float pinchDistance)
        {
            cursorAnchor.position = position;
            cursorAnchor.rotation = rotation;

            if (pinchDistance < 0.03f && !currentlyPinching)
            {
                currentlyPinching = true;
                pinchingCursor.SetActive(currentlyPinching);
                defaultCursor.SetActive(!currentlyPinching);
            }
            else if (pinchDistance > 0.05f && currentlyPinching)
            {
                currentlyPinching = false;
                pinchingCursor.SetActive(currentlyPinching);
                defaultCursor.SetActive(!currentlyPinching);
            }
        }

        public void SetMoveState(bool state)
        {
            movingLeftRight.SetActive(state);
            movingUpDown.SetActive(state);
        }

        public void UpdateCursorPosition(Vector3 position)
        {
            cursorAnchor.position = position;
        }

        public void UpdateCursorRotation(Quaternion rotation)
        {
            cursorAnchor.rotation = rotation;
        }

        public void UpdateCursorRotation(Vector3 rotation)
        {
            cursorAnchor.localEulerAngles = rotation;
        }
    }
#pragma warning restore CS0649
}