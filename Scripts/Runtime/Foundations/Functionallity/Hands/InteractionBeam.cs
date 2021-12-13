using System;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using static Bouvet.DevelopmentKit.Internal.Utils.TypeHelpers;

#if WINDOWS_UWP || DOTNETWINRT_PRESENT
using Windows.Perception.People;
#endif

/// <summary>
/// This class deals with interaction beams. 
/// </summary>
namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
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

        private readonly Vector3 originOffset = new Vector3(0.175f, -0.35f, -0.2f);

        private Interactable currentInteractable;
        internal bool currentlyVisible;
        internal InteractionBeamCursor cursor; // Main object on the cursor. Used to position and scale the cursor correctly
        internal float handRotation; // Current rotation of the hand compared to the user

        private RaycastHit hit;
        private RaycastHit hitUI;

        internal bool holdingSomething;
        private Transform hololensTransform; // Transform of the Hololens

        private InputSource interactionBeamInputSource;
        private float interactionStartDistance;
        private float interactionStartObjectDistance;
        private LineRenderer lineRenderer; // Line renderer used to draw the visual representation of the interaction beam
        private Transform origin; // Transform from which the Hololens calculates the direction of the ray
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

                        interactionBeamInputSource.collidedObjectIdentifier.Equals(currentInteractable.gameObject);
                        inputManager.GetHandGestureListener().ManipulationStarted(interactionBeamInputSource);
                        inputManager.GetHandGestureListener().InputDown(interactionBeamInputSource);
                        if (!currentInteractable.gameObject.GetComponent<InteractableButton>())
                        {
                            interactionStartObjectDistance = Vector3.Distance(rayStart.position, hit.point);
                            interactionStartDistance = Vector3.Distance(rayStart.position, origin.position);
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

        internal void UpdateTargetTransfrom(Transform newTransform, bool alignWithTransform = false)
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
                    if (inputManager.GetCursorState(isRightHand) == CursorState.InteractionBeamCursor && UpdateInteractionBeamRotation())
                    {
                        if (!currentlyVisible)
                        {
                            SetInteractionBeamVisibillity(true);
                        }

                        rayStart.rotation = Quaternion.Slerp(rayStart.rotation, origin.rotation, 0.5f);
                        DrawQuadraticBezierCurve(rayStart.position + rayStart.forward / 5f, rayStart.TransformPoint(Vector3.forward), rayTarget.position);

                        UpdateCursor(rayTarget.position, Quaternion.FromToRotation(Vector3.forward, hit.normal), source.pinchDistance);
                        // If not holding anything
                        if (!holdingSomething)
                        {
                            // If the raycast hits something
                            if (Physics.Raycast(rayStart.position, rayStart.forward, out hit, inputManager.inputSettings.InteractionBeamsDistance))
                            {
                                if (hit.collider.gameObject.layer != 5 && Physics.Raycast(rayStart.position, rayStart.forward, out hitUI, inputManager.inputSettings.InteractionBeamsDistance, 1 << 5))
                                {
                                    hit = hitUI;
                                }

                                rayStart.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(rayStart.position, hit.point));
                                Interactable interactable = hit.collider.GetComponent<Interactable>();
                                if (interactable && !interactable.Equals(currentInteractable))
                                {
                                    DisconnectInteractable();
                                    currentInteractable = interactable;
                                    interactionBeamInputSource.collidedObjectIdentifier = hit.collider.gameObject;
                                    interactable.OnFocusBegin();
                                }
                                else if (!interactable && currentInteractable)
                                {
                                    DisconnectInteractable();
                                }
                                else
                                {
                                    interactionBeamInputSource.collidedObjectIdentifier = hit.collider.gameObject;
                                }
                            }
                            // If the raycast does not hit anything
                            else
                            {
                                rayStart.transform.localScale = new Vector3(1f, 1f, 10f);
                                cursor.UpdateCursorRotation(Vector3.zero);
                                currentInteractable = null;
                                interactionBeamInputSource.collidedObjectIdentifier = null;
                            }
                        }
                        // If holding something (ray will then always be visible)
                        else if (holdingSomething)
                        {
                            float newDistance = Vector3.Distance(rayStart.position, origin.position);
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
            interactionBeamInputSource.worldPosition = MakeSystemVector3(position);
            interactionBeamInputSource.worldRotation = MakeSystemQuaternion(rotation);
        }

        /// <summary>
        /// Sets the rotation/path of the interaction beam
        /// This is a bit overengineered and could probably be done another way
        /// </summary>
        private bool UpdateInteractionBeamRotation()
        {
            //TODO: There is an error when the hand is moved to the edge of the FOV.

            // Calculate angle to place origin
            float distance = Vector3.Distance(hololensTransform.position.XZ(), rayStart.position.XZ());
            if (distance <= originOffset.x) // Override if hands are too close to the body
            {
                return false;
            }

            double degrees = Math.Acos(originOffset.x) * (180f / Math.PI) + 15f;

            // Position origin correct
            origin.position = new Vector3(hololensTransform.position.x, rayStart.position.y, hololensTransform.position.z);
            origin.LookAt(rayStart);
            origin.Rotate(Vector3.up, isRightHand ? (float) degrees : (float) -degrees);
            origin.position = hololensTransform.position + origin.forward * originOffset.x + Vector3.up * originOffset.y + rayStart.forward * originOffset.z;
            origin.LookAt(rayStart);

            // Fix rotation to "ignore" head rotation
            (origin.rotation * Quaternion.Inverse(hololensTransform.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            origin.Rotate(axis, -angle / 10f);

            return true;
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
            Vector3 B = new Vector3(0, 0, 0);
            for (int i = 0; i < curveSmoothness; i++)
            {
                B = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * Vector3.Lerp(start, middle, curveAmount) + t * t * end;
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
            interactionBeamInputSource.collidedObjectIdentifier = null;
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
            origin = new GameObject().transform;
            origin.gameObject.name = "Origin";
            rayStart = new GameObject().transform;
            rayStart.parent = transform;
            rayStart.gameObject.name = "RayStart";
            cursor = Instantiate(cursorPrefab, visualComponents.transform).GetComponent<InteractionBeamCursor>();
            if (isRightHand)
            {
                rayStart.gameObject.AddComponent<AttachedToJoint>().SetupAttachedJoint(inputManager, InputSourceKind.HandRight, JointName.IndexProximal, null, true, false);
            }
            else
            {
                rayStart.gameObject.AddComponent<AttachedToJoint>().SetupAttachedJoint(inputManager, InputSourceKind.HandLeft, JointName.IndexProximal, null, true, false);
            }

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
#pragma warning restore CS0649
}