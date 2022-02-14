using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bouvet.DevelopmentKit.Editor
{
    /// <summary>
    /// Editor script for rotation and using object in the Unity Editor play mode for debugging purposes.
    /// The interactions simulated pretend to be coming from the Right Interaction Beam.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MouseInteractions : MonoBehaviour
    {
    #if UNITY_EDITOR
        public InputManager inputManager;

        [Range(0.0f, 5.0f)]
        public float m_LookSensitivity = 0.003f;

        private float m_MouseX;
        private float m_MouseY;
        private float m_MouseZ;

        private RaycastHit hit;
        private InputSource inputSource;
        private float timeSinceClick;
        private bool holdingSomething;
        private Interactable interactable;

        private void Start()
        {
            inputManager = InputManager.Instance;
            inputSource = new InputSource();
            inputSource.inputSourceKind = InputSourceKind.InteractionBeamRight;
        }

        private void Rotate()
        {
            m_MouseX += Mouse.current.delta.ReadValue().x * m_LookSensitivity;
            m_MouseY += Mouse.current.delta.ReadValue().y * m_LookSensitivity;
            transform.localEulerAngles = Vector3.left * m_MouseY + Vector3.up * m_MouseX;
        }

        // LMB: 0
        // RMB: 1
        private void Update()
        {
            if (!Application.isFocused)
                return;

            if (Mouse.current.rightButton.isPressed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Rotate();
            }
            else
                Cursor.lockState = CursorLockMode.None;

            if (!inputManager.inputSettings.UseHands)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                timeSinceClick = Time.time;
                Ray ray = Cursor.lockState == CursorLockMode.Locked ? new Ray(transform.position, transform.forward) : Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit, 12f))
                {
                    interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable)
                    {
                        inputSource.collidedObject = hit.collider.gameObject;
                        inputManager.GetHandGestureListener().InputDown(inputSource);
                    }
                }
                else
                    inputSource.collidedObject = null;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                if (!holdingSomething && inputSource.collidedObject != null)
                    StartHolding();

                if (holdingSomething)
                {
                    if (Mouse.current.rightButton.isPressed)
                        inputManager.GetHandGestureListener().ManipulationUpdated(inputSource);
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                        inputManager.rightGripPoint.position = ray.origin + ray.direction * m_MouseZ;
                        inputManager.GetHandGestureListener().ManipulationUpdated(inputSource);
                    }
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Ray ray = Cursor.lockState == CursorLockMode.Locked ? new Ray(transform.position, transform.forward) : Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit, inputManager.inputSettings.InteractionBeamsDistance))
                {
                    interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable)
                    {
                        inputSource.collidedObject = hit.collider.gameObject;
                        inputManager.GetHandGestureListener().InputUp(inputSource);
                    }
                }
                else
                    inputSource.collidedObject = null;

                StopHolding();
            }
        }

        private void StartHolding()
        {
            if (!interactable)
            {
                return;
            }

            inputManager.rightGripPoint.parent = transform;
            inputManager.rightGripPoint.position = hit.point;
            m_MouseZ = Vector3.Distance(transform.position, inputManager.rightGripPoint.position);
            inputManager.GetHandGestureListener().ManipulationStarted(inputSource);
            holdingSomething = true;
        }

        private void StopHolding()
        {
            if (holdingSomething)
            {
                inputManager.GetHandGestureListener().ManipulationEnded(inputSource);
                interactable = null;
            }

            holdingSomething = false;
        }
    #endif
    }
}