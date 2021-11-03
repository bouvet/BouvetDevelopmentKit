using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bouvet.DevelopmentKit.Input.Hands
{
    public class ProximityAndManipulationEvent : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        internal InputManager inputManager;

        [SerializeField]
        internal InteractionBeam interactionBeam;

        [SerializeField]
        internal float touchDistance = 0.01f;

        public bool isRightHand;

        private float closestDistance;
        private int closestIndex;
        private int newObjectId;
        private int currentObjectId;
        private bool somethingInProximity;
        private Collider[] hits = new Collider[20];

        internal bool CurrentlyManipulating;
        private GameObject currentManipulationObject;
        private HandGestureListenerInternal handGestureListener;
        private InputSource handInputSource;
        private Quaternion quaternionCache;
        private bool inProximity;
        private bool setupComplete;

        private void Start()
        {
            if (inputManager.inputSettings.UseManipulation)
            {
                inputManager.OnInputUp += InputManager_OnInputUp;
                inputManager.OnInputDown += InputManager_OnInputDown;
            }

            inputManager.OnInputUpdated += InputManager_OnInputUpdated;
            Invoke(nameof(Setup), 0.2f);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (setupComplete)
            {
                handInputSource.worldPosition = ValueConverter.MakeSystemVector3(transform.position);
                InputManager_OnInputUpdated(handInputSource);
            }
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                InputManager_OnInputDown(handInputSource);
            }
#endif
        }

        private void OnDisable()
        {
            if (setupComplete)
            {
                handInputSource.collidedObjectIdentifier = 0;
                handGestureListener.ProximityEnded(handInputSource);
                inProximity = false;
            }
        }

        private void InputManager_OnInputUp(InputSource inputSource)
        {
            if (inputSource.inputSourceKind != handInputSource.inputSourceKind)
            {
                return;
            }
            CurrentlyManipulating = false;
            handInputSource.collidedObjectIdentifier = 0;
            handGestureListener.ManipulationEnded(handInputSource);
        }

        private void InputManager_OnInputUpdated(InputSource inputSource)
        {
            if (!setupComplete)
            {
                return;
            }

            somethingInProximity = false;

            if (CurrentlyManipulating)
            {
                MoveGripPoint();
                handGestureListener.ManipulationUpdated(handInputSource);
            }
            else
            {
                hits = Physics.OverlapSphere(transform.position, touchDistance);
                if (hits.Length > 0)
                {
                    closestIndex = -1;
                    closestDistance = float.MaxValue;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (hits[i]
                            && !hits[i].transform.gameObject.tag.Equals("IgnoreProximity")
                            && hits[i].transform.gameObject.GetComponent<Interactable>()
                            && closestDistance > Vector3.Distance(transform.position, hits[i].ClosestPoint(transform.position)))
                        {
                            closestDistance = Vector3.Distance(transform.position, hits[i].transform.position);
                            closestIndex = i;
                            somethingInProximity = true;
                        }
                    }

                    if (somethingInProximity)
                    {
                        newObjectId = inputManager.GetId(hits[closestIndex].transform.gameObject);
                        if (currentObjectId != newObjectId)
                        {
                            BdkLogger.Log($"Entered contact with new object: {hits[closestIndex].name}:{closestIndex}:{hits.Length}", LogSeverity.Info);
                            if (currentObjectId != -1)
                            {
                                handGestureListener.ProximityEnded(handInputSource);
                            }

                            handInputSource.collidedObjectIdentifier = newObjectId;
                            handGestureListener.ProximityStarted(handInputSource);
                        }

                        currentObjectId = newObjectId;
                        inProximity = true;

                        MoveGripPoint();
                    }
                    else
                    {
                        somethingInProximity = false;
                    }
                }
                else
                {
                    somethingInProximity = false;
                }
            }

            if (!somethingInProximity && !CurrentlyManipulating)
            {
                inProximity = false;
                currentObjectId = -1;
                hits = new Collider[10];
                handGestureListener.ProximityEnded(handInputSource);
                handInputSource.collidedObjectIdentifier = 0;
            }

            if (inProximity)
            {
                handGestureListener.ProximityUpdated(handInputSource);
            }
        }

        private void MoveGripPoint()
        {
            if (!interactionBeam || !interactionBeam.holdingSomething)
            {
                if (inputManager.TryGetHandJointTransform(handInputSource.inputSourceKind, JointName.IndexTip, out Vector3 indexPos, out quaternionCache)
                    && inputManager.TryGetHandJointTransform(handInputSource.inputSourceKind, JointName.ThumbTip, out Vector3 thumbPos, out quaternionCache))
                {
                    if (isRightHand)
                    {
                        inputManager.rightGripPoint.position = Vector3.Lerp(indexPos, thumbPos, 0.5f);
                    }
                    else
                    {
                        inputManager.leftGripPoint.position = Vector3.Lerp(indexPos, thumbPos, 0.5f);
                    }
                }
            }
        }

        private void InputManager_OnInputDown(InputSource inputSource)
        {
            if (inputSource.inputSourceKind != handInputSource.inputSourceKind)
            {
                return;
            }

            UpdateClosestGrabbable();
            if (currentManipulationObject)
            {
                CurrentlyManipulating = true;
                handInputSource.collidedObjectIdentifier = inputManager.GetId(currentManipulationObject);
                MoveGripPoint();
                BdkLogger.Log($"Manipulation started", LogSeverity.Info);
                handGestureListener.ManipulationStarted(handInputSource);
            }
        }

        private void UpdateClosestGrabbable()
        {
            closestIndex = -1;
            closestDistance = float.MaxValue;
            currentManipulationObject = null;
            somethingInProximity = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i]
                    && !hits[i].transform.gameObject.tag.Equals("IgnoreProximity")
                    && (hits[i].transform.gameObject.GetComponent<Grabbable>() || hits[i].transform.gameObject.GetComponent<ReferToGrabbable>())
                    && closestDistance > Vector3.Distance(transform.position, hits[i].ClosestPoint(transform.position)))
                {
                    closestDistance = Vector3.Distance(transform.position, hits[i].transform.position);
                    closestIndex = i;
                    somethingInProximity = true;
                }
            }

            if (somethingInProximity)
            {
                currentManipulationObject = hits[closestIndex].gameObject;
                BdkLogger.Log($"Closest: " + hits[closestIndex].gameObject.name, LogSeverity.Info);
            }
        }

        private void Setup()
        {
            handGestureListener = inputManager.GetHandGestureListenerInternal();
            if (handGestureListener == null)
            {
                BdkLogger.Log($"Couldn't find HandGestureListener. Trying again!", LogSeverity.Info);
                Invoke(nameof(Setup), 0.2f);
                return;
            }

            if (handGestureListener.TryGetHandInputSource((isRightHand ? InputSourceKind.HandRight : InputSourceKind.HandLeft), out handInputSource) && handInputSource != null)
            {
                setupComplete = true;
                BdkLogger.Log($"Found HandInputSource", LogSeverity.Info);
            }
            else
            {
                BdkLogger.Log($"Couldn't find HandInputSource. Trying again!", LogSeverity.Info);
                Invoke(nameof(Setup), 0.2f);
            }
            //if (handGestureListener)
            //{
            //    handInputSource = isRightHand ? handGestureListener.RightHandInputSource : handGestureListener.LeftHandInputSource;
            //}

        }
#pragma warning restore CS0649
    }
}