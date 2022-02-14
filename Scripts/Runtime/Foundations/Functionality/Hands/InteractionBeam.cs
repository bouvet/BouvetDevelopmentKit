using System;
using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

#if WINDOWS_UWP || DOTNETWINRT_PRESENT
using Windows.Perception.People;
#endif

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    [RequireComponent(typeof(LineRenderer))]
    public class InteractionBeam : MonoBehaviour
    {
        [SerializeField]
        internal InputManager inputManager;

        [SerializeField]
        private GameObject cursorPrefab; // Cursor prefab that must have the PinchCursor script attached     

        [SerializeField]
        internal bool isRightHand; // Bool that decides whether the current interaction beam is connected to the right or left hand

        [SerializeField]
        [Range(1, 128)]
        private int curveSmoothness = 64; // Amount of points on the interaction beam curve.

        [SerializeField]
        [Range(0.1f, 0.8f)]
        private float curveAmount = 0.5f; // Weight of the curve. Lower values results in curve being curved all the way. Higher values results in the curve being straight at first and the curve a lot towards the end


        private Interactable currentInteractable;
        internal bool currentlyVisible;
        internal InteractionBeamCursor cursor; // Main object on the cursor. Used to position and scale the cursor correctly
        internal float handRotation; // Current rotation of the hand compared to the user

        private RaycastHit hit;
        private RaycastHit hitUi;

        internal bool holdingSomething;
        private Transform hololensTransform; // Transform of the Hololens

        private InputSource interactionBeamInputSource;
        private float interactionStartDistance;
        private float interactionStartObjectDistance;
        private LineRenderer lineRenderer; // Line renderer used to draw the visual representation of the interaction beam
        internal bool palmFacingHololens;
        private Transform rayStart; // Start of the ray (inside the palm of the user)
        internal Transform rayTarget; // Transfrom of the point where the ray hits an object 

        private GameObject visualComponents;

        /// <summary>
        /// Sets up the interaction beams
        /// </summary>
        private void Start()
        {
            SetupEventListeners();
            SetupGameObjects();
            SetupLineRenderer();
            SetupInputSource();
            cursor.UpdateCursorPosition(rayTarget.position);
            SetInteractionBeamVisibillity(false);
        }

        /// <summary>
        /// When the player pinches down the interaction beam grabs onto the target ray if that object is an interactable object
        /// </summary>
        /// <param name="source"></param>
        internal void InputDown(InputSource source)
        {
            try
            {
                if (inputManager.GetCursorState(isRightHand) == CursorState.InteractionBeamCursor && (source.inputSourceKind == InputSourceKind.HandRight && isRightHand || source.inputSourceKind == InputSourceKind.HandLeft && !isRightHand))
                {
                    if (currentInteractable)
                    {
                        if (isRightHand)
                        {
                            inputManager.rightGripPoint.parent = rayStart;
                            inputManager.rightGripPoint.position = hit.point;
                        }
                        else
                        {
                            inputManager.leftGripPoint.parent = rayStart;
                            inputManager.leftGripPoint.position = hit.point;
                        }

                        interactionBeamInputSource.collidedObject = currentInteractable.gameObject;
                        inputManager.GetHandGestureListener().ManipulationStarted(interactionBeamInputSource);
                        inputManager.GetHandGestureListener().InputDown(interactionBeamInputSource);
                        if (!currentInteractable.gameObject.GetComponent<InteractableButton>())
                        {
                            interactionStartObjectDistance = Vector3.Distance(rayStart.position, hit.point);
                            interactionStartDistance = (hololensTransform.position - rayStart.position).XZ().magnitude;
                            rayTarget.position = hit.point;
                            rayTarget.parent = currentInteractable.transform;
                            holdingSomething = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error in InteractionBeam with InputDown.", e);
            }
        }

        public void UpdateTargetTransfrom(Transform newTransform, bool alignWithTransform = false)
        {
            CheckRayTarget();
            if (alignWithTransform)
            {
                rayTarget.position = newTransform.position;
            }

            rayTarget.parent = newTransform;
        }


        /// <summary>
        /// When the player stops pinching the interaction beam release the interactable object
        /// </summary>
        /// <param name="source"></param>
        internal void InputUp(InputSource source)
        {
            if (source.inputSourceKind == InputSourceKind.HandRight && isRightHand || source.inputSourceKind == InputSourceKind.HandLeft && !isRightHand)
            {
                inputManager.GetHandGestureListener().InputUp(interactionBeamInputSource);
                if (currentInteractable)
                {
                    inputManager.GetHandGestureListener().ManipulationEnded(interactionBeamInputSource);
                    DisconnectInteractable();
                    rayTarget.parent = rayStart;
                    rayTarget.localPosition = Vector3.forward;
                    if (isRightHand)
                    {
                        inputManager.rightGripPoint.parent = null;
                    }
                    else
                    {
                        inputManager.leftGripPoint.parent = null;
                    }

                    holdingSomething = false;
                    UpdateCursor(rayTarget.position, Quaternion.Euler(Vector3.zero), source.pinchDistance);
                }
            }
        }

        /// <summary>
        /// Input update is called every frame. If the hand is visible and is not in close proximity of a grabbable object, it will update its ray.
        /// This also updates the object it is currently holding if it is holding one. 
        /// </summary>
        /// <param name="source"></param>
        internal void InputUpdate(InputSource source)
        {
            try
            {
                if (source.inputSourceKind == InputSourceKind.HandRight && isRightHand
                    || source.inputSourceKind == InputSourceKind.HandLeft && !isRightHand)
                {
                    if (inputManager.GetCursorState(isRightHand) == CursorState.InteractionBeamCursor && !RayStartTooCloseToBody())
                    {
                        if (!currentlyVisible)
                        {
                            SetInteractionBeamVisibillity(true);
                        }


                        rayStart.rotation = Quaternion.Slerp(rayStart.rotation, GetInteractionBeamRotation(), 0.5f);
                        DrawQuadraticBezierCurve(rayStart.position + rayStart.forward / 5f, rayStart.TransformPoint(Vector3.forward), rayTarget.position);

                        UpdateCursor(rayTarget.position, Quaternion.FromToRotation(Vector3.forward, hit.normal), source.pinchDistance);
                        // If not holding anything
                        if (!holdingSomething)
                        {
                            // If the raycast hits something
                            if (Physics.Raycast(rayStart.position, rayStart.forward, out hit, inputManager.inputSettings.InteractionBeamsDistance))
                            {
                                if (hit.collider.gameObject.layer != 5 && Physics.Raycast(rayStart.position, rayStart.forward, out hitUi, inputManager.inputSettings.InteractionBeamsDistance, 1 << 5))
                                {
                                    hit = hitUi;
                                }

                                rayStart.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(rayStart.position, hit.point));
                                Interactable interactable = hit.collider.GetComponent<Interactable>();
                                if (interactable && !interactable.Equals(currentInteractable))
                                {
                                    DisconnectInteractable();
                                    currentInteractable = interactable;
                                    interactionBeamInputSource.collidedObject = hit.collider.gameObject;
                                    interactable.OnFocusBegin();
                                }
                                else if (!interactable && currentInteractable)
                                {
                                    DisconnectInteractable();
                                }
                                else
                                {
                                    interactionBeamInputSource.collidedObject = hit.collider.gameObject;
                                }
                            }
                            // If the raycast does not hit anything
                            else
                            {
                                rayStart.transform.localScale = new Vector3(1f, 1f, 10f);
                                cursor.UpdateCursorRotation(Vector3.zero);
                                currentInteractable = null;
                                interactionBeamInputSource.collidedObject = null;
                            }
                        }
                        // If holding something (ray will then always be visible)
                        else if (holdingSomething)
                        {
                            float newDistance = (hololensTransform.position - rayStart.position).XZ().magnitude;
                            rayStart.transform.localScale = new Vector3(1f, 1f, interactionStartObjectDistance + inputManager.inputSettings.InteractionBeamDepthMultiplier * (newDistance - interactionStartDistance));
                            if (currentInteractable)
                            {
                                inputManager.GetHandGestureListener().ManipulationUpdated(interactionBeamInputSource);
                            }
                        }
                    }
                    else if (currentlyVisible)
                    {
                        DisableInteractionBeam();
                    }
                }
            }
            catch (Exception e)
            {
                CheckRayTarget();
                BdkLogger.LogException("Error in InteractionBeam Input.", e);
            }
        }

        private void UpdateCursor(Vector3 position, Quaternion rotation, float pinchDistance)
        {
            cursor.UpdateCursor(position, rotation, pinchDistance);
            interactionBeamInputSource.worldPosition = position;
            interactionBeamInputSource.worldRotation = rotation;
        }

        /// <summary>
        /// The interaction beam rotation is given by an axis between a virtual "origin" point and the start of the interaction beam.
        /// origin is defined with a certain offset from the hololens and the start of the interaction beam, which means the offset
        ///rotates with the rotation of the hand relative to the head. The rotation of the hand and head makes no differance, only their relative position.
        /// </summary>
        private Quaternion GetInteractionBeamRotation()
        {
            // Find coordinate system looking at the hand from the head, independant of y azis
            Vector3 forward = (rayStart.position - hololensTransform.position).XZ().normalized;
            Quaternion look = Quaternion.LookRotation(forward, Vector3.up);

            Vector3 offset = new Vector3((isRightHand ? 1 : -1) * 0.23f, -0.334f, -0.21f);
            return Matrix4x4.LookAt(hololensTransform.position + look * offset, rayStart.position, Vector3.up).rotation;
        }

        private bool RayStartTooCloseToBody()
        {
            float distance = Vector3.ProjectOnPlane(hololensTransform.position - rayStart.position, Vector3.up).magnitude;
            return distance < 0.175f;
        }

        private void CheckRayTarget()
        {
            if (!rayTarget)
            {
                rayTarget = new GameObject("RayTarget").transform;
                rayTarget.parent = rayStart;
                rayTarget.localPosition = Vector3.forward;
                holdingSomething = false;
            }
        }

        private void InputManager_OnHandRotationToggle(InputSource inputSource, float rotationState)
        {
            if (isRightHand && inputSource.inputSourceKind == InputSourceKind.HandRight
                || !isRightHand && inputSource.inputSourceKind == InputSourceKind.HandLeft)
            {
                handRotation = rotationState;
            }
        }

        /// <summary>
        /// Function that takes in three points in space and draws a curve between them. Smoothness can be set in the inspector.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="middle"></param>
        /// <param name="end"></param>
        private void DrawQuadraticBezierCurve(Vector3 start, Vector3 middle, Vector3 end)
        {
            float t = 0f;
            for (int i = 0; i < curveSmoothness; i++)
            {
                Vector3 B = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * Vector3.Lerp(start, middle, curveAmount) + t * t * end;
                lineRenderer.SetPosition(i, B);
                t += 1 / (float) curveSmoothness;
            }
        }

        /// <summary>
        /// Toggles interaction beam off if the hand is moved out of the tracking space.
        /// </summary>
        /// <param name="source"></param>
        internal void SourceLost(InputSource source)
        {
            if (source.inputSourceKind == InputSourceKind.HandRight && isRightHand || source.inputSourceKind == InputSourceKind.HandLeft && !isRightHand)
            {
                DisableInteractionBeam();
            }
        }

        /// <summary>
        /// Disables and disconnects all interactables when the user moves their hand outside of the tracked area
        /// </summary>
        internal void DisableInteractionBeam()
        {
            inputManager.GetHandGestureListener().InputUp(interactionBeamInputSource);
            inputManager.GetHandGestureListener().ManipulationEnded(interactionBeamInputSource);
            SetInteractionBeamVisibillity(false);
            DisconnectInteractable();
            rayTarget.parent = rayStart;
            rayTarget.localPosition = Vector3.forward;
            holdingSomething = false;
        }

        /// <summary>
        /// Toggles the visible components of the interaction beam on and off
        /// </summary>
        /// <param name="value"></param>
        private void SetInteractionBeamVisibillity(bool value)
        {
            currentlyVisible = value;
            lineRenderer.enabled = value;
            visualComponents.SetActive(value);
            interactionBeamInputSource.active = value;
        }

        /// <summary>
        /// Releases the interactable object and resets the interaction beam to be ready to grab another object.
        /// </summary>
        private void DisconnectInteractable()
        {
            if (currentInteractable)
            {
                currentInteractable.OnFocusEnd();
            }

            currentInteractable = null;
            interactionBeamInputSource.collidedObject = null;
        }

#region Setup of interaction beam

        private void SetupEventListeners()
        {
            if (inputManager.inputSettings.UseInteractionBeams)
            {
                inputManager.OnInputDown += InputDown;
                inputManager.OnInputUpdated += InputUpdate;
                inputManager.OnInputUp += InputUp;
                inputManager.OnSourceLost += SourceLost;
                inputManager.OnHandRotationToggle += InputManager_OnHandRotationToggle;
            }
        }

        private void SetupGameObjects()
        {
            hololensTransform = Camera.main.transform;
            visualComponents = new GameObject();
            visualComponents.transform.parent = transform;
            visualComponents.gameObject.name = "VisualComponents";
            rayStart = new GameObject().transform;
            rayStart.parent = transform;
            rayStart.gameObject.name = "RayStart";
            cursor = Instantiate(cursorPrefab, visualComponents.transform).GetComponent<InteractionBeamCursor>();
            rayStart.gameObject.AddComponent<AttachedToJoint>().SetupAttachedJoint(inputManager, isRightHand ? InputSourceKind.HandRight : InputSourceKind.HandLeft, JointName.IndexProximal, matchRotation: false);

            rayTarget = new GameObject().transform;
            rayTarget.parent = rayStart;
            rayTarget.gameObject.name = "RayTarget";
            rayTarget.localPosition = Vector3.forward;
        }

        private void SetupLineRenderer()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = curveSmoothness;
        }

        private void SetupInputSource()
        {
            interactionBeamInputSource = new InputSource();
            interactionBeamInputSource.inputSourceKind = isRightHand ? InputSourceKind.InteractionBeamRight : InputSourceKind.InteractionBeamLeft;
            inputManager.AddInputSource(interactionBeamInputSource);
        }

#endregion
    }
}