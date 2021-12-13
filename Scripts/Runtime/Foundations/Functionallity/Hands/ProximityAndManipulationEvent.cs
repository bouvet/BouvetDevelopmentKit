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
        private GameObject newObject;
        private GameObject currentObject;
        private bool somethingInProximity;
        private Collider[] hits = new Collider[20];

        internal bool CurrentlyManipulating;
        private GameObject currentManipulationObject;
        private HandGestureListener handGestureListener;
        private InputSource handInputSource;
        private InputSource indexFingerInputSource;
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
                handInputSource.worldPosition = TypeHelpers.MakeSystemVector3(transform.position);
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
                handInputSource.collidedObjectIdentifier = null;
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
            handInputSource.collidedObjectIdentifier = null;
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
                            closestDistance = Vector3.Distance(transform.position, hits[i].ClosestPoint(transform.position));
                            closestIndex = i;
                            somethingInProximity = true;
                        }
                    }

                    if (somethingInProximity)
                    {
                        newObject = hits[closestIndex].transform.gameObject;
                        if (!currentObject.Equals(newObject))
                        {
                            BdkLogger.Log($"Entered contact with new object: {hits[closestIndex].name}:{closestIndex}:{hits.Length}", LogSeverity.Info);
                            if (currentObject != null)
                            {
                                handGestureListener.ProximityEnded(handInputSource);
                            }

                            handInputSource.collidedObjectIdentifier = hits[closestIndex].transform.gameObject;
                            handGestureListener.ProximityStarted(handInputSource);
                        }

                        currentObject = newObject;
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
                currentObject = null;
                hits = new Collider[10];
                handGestureListener.ProximityEnded(handInputSource);
                handInputSource.collidedObjectIdentifier = null;
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
                handInputSource.collidedObjectIdentifier = currentManipulationObject;
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
            handGestureListener = inputManager.GetHandGestureListener();
            if (handGestureListener == null)
            {
                BdkLogger.Log($"Couldn't find HandGestureListener. Trying again!", LogSeverity.Info);
                Invoke(nameof(Setup), 0.2f);
                return;
            }

            if (handGestureListener.TryGetHandInputSource((isRightHand ? InputSourceKind.HandRight : InputSourceKind.HandLeft), out handInputSource) && handInputSource != null)
            {
                SetupIndexFinger();
                setupComplete = true;
                BdkLogger.Log($"Found HandInputSource", LogSeverity.Info);
            }
            else
            {
                BdkLogger.Log($"Couldn't find HandInputSource. Trying again!", LogSeverity.Info);
                Invoke(nameof(Setup), 0.2f);
            }
        }

        private void SetupIndexFinger()
        {
            indexFingerInputSource = new InputSource();
            if (isRightHand)
            {
                indexFingerInputSource.inputSourceKind = InputSourceKind.IndexFingerRight;
            }
            else
            {
                indexFingerInputSource.inputSourceKind = InputSourceKind.IndexFingerLeft;
            }
            inputManager.AddInputSource(indexFingerInputSource);
        }
#pragma warning restore CS0649
    }
}