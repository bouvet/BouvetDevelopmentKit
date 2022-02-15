using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Input.Hands;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    public class ProximityAndManipulationEvent : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        internal InputManager inputManager;

        [SerializeField]
        internal InteractionBeam interactionBeam;

        [SerializeField]
        internal float indexFingerTouchDistance = 0.01f;

        [SerializeField]
        internal float handTouchDistance = 0.2f;

        public bool isRightHand;

        private GameObject newObject;
        private Collider[] indexHits = new Collider[10];
        private Collider[] handHits = new Collider[10];

        internal bool CurrentlyManipulating = false;
        private bool IndexInProximityPreviousUpdate;
        private bool HandInProximityPreviousUpdate;
        private HandGestureListener handGestureListener;
        private InputSource handInputSource;
        private InputSource indexFingerInputSource;
        private Quaternion quaternionCache;
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
                InputManager_OnInputUpdated(handInputSource);
            }
            if (!CurrentlyManipulating && Keyboard.current.gKey.wasPressedThisFrame)
            {
                InputManager_OnInputDown(handInputSource);
            }
            if (CurrentlyManipulating && Keyboard.current.gKey.wasReleasedThisFrame)
            {
                InputManager_OnInputUp(handInputSource);
            }
#endif
        }

        private void OnDisable()
        {
            if (setupComplete)
            {
                indexFingerInputSource.collidedObject = null;
                handGestureListener.ProximityEnded(indexFingerInputSource);
            }
        }

        private void InputManager_OnInputUp(InputSource inputSource)
        {
            if (inputSource.inputSourceKind != handInputSource.inputSourceKind)
            {
                return;
            }
            CurrentlyManipulating = false;

            BdkLogger.Log($"Manipulation ended", LogSeverity.Info);
            handGestureListener.ManipulationEnded(handInputSource);
            handInputSource.collidedObject = null;
        }

        private void InputManager_OnInputUpdated(InputSource inputSource)
        {
            if (!setupComplete)
            {
                return;
            }
            if (CurrentlyManipulating)
            {
                MoveGripPoint();
                handGestureListener.ManipulationUpdated(handInputSource);
            }
            else
            {
                HandProximityCheck();
                IndexFingerProximityCheck();
            }
        }

        private void InputManager_OnInputDown(InputSource inputSource)
        {
            if (inputSource.inputSourceKind != handInputSource.inputSourceKind)
            {
                return;
            }
            if (UpdateHandCollision(out Vector3 handPos) && TryGetClosestGrabbableByOrigin(handPos, handHits, out newObject))
            {
                CurrentlyManipulating = true;
                handInputSource.collidedObject = newObject;
                MoveGripPoint();
                handGestureListener.ManipulationStarted(handInputSource);
                BdkLogger.Log($"Manipulation started", LogSeverity.Info);
            }
        }

        private bool UpdateIndexCollision(out Vector3 worldPos)
        {
            worldPos = Vector3.zero;
            if (!Application.isEditor && !inputManager.TryGetHandJointTransform(handInputSource.inputSourceKind, JointName.IndexTip, out worldPos, out quaternionCache))
                return false;
#if UNITY_EDITOR
            worldPos = transform.position;
#endif
            indexFingerInputSource.worldPosition = worldPos;
            indexHits = Physics.OverlapSphere(worldPos, indexFingerTouchDistance);
            return indexHits.Length != 0;
        }

        private bool UpdateHandCollision(out Vector3 worldPos)
        {
            worldPos = Vector3.zero;
            if (!Application.isEditor && !inputManager.TryGetHandJointTransform(handInputSource.inputSourceKind, JointName.Palm, out worldPos, out quaternionCache))
                return false;
#if UNITY_EDITOR
            worldPos = transform.position;
#endif
            handInputSource.worldPosition = worldPos;
            handHits = Physics.OverlapSphere(worldPos, handTouchDistance);
            return handHits.Length != 0;
        }

        private void IndexFingerProximityCheck()
        {
            bool inProximity = false;
            if (UpdateIndexCollision(out Vector3 worldPos))
            {
                if (TryGetClosestInteractibleByOrigin(worldPos, indexHits, out newObject))
                {
                    if (!indexFingerInputSource.collidedObject || !indexFingerInputSource.collidedObject.Equals(newObject))
                    {
                        BdkLogger.Log($"Index entered contact with new object: {newObject.name}:{indexHits.Length}", LogSeverity.Info);
                        if (indexFingerInputSource.collidedObject != null)
                        {
                            handGestureListener.ProximityEnded(indexFingerInputSource);
                        }
                        indexFingerInputSource.collidedObject = newObject;
                        handGestureListener.ProximityStarted(indexFingerInputSource);
                        indexFingerInputSource.collidedObject = newObject;
                    }
                    inProximity = true;
                    IndexInProximityPreviousUpdate = true;
                }

            }
            else if (!CurrentlyManipulating && IndexInProximityPreviousUpdate)
            {
                IndexInProximityPreviousUpdate = false;
                handGestureListener.ProximityEnded(indexFingerInputSource);
                indexFingerInputSource.collidedObject = null;
            }

            if (inProximity)
            {
                handGestureListener.ProximityUpdated(indexFingerInputSource);
            }
        }

        private void HandProximityCheck()
        {
            bool inProximity = false;
            if (UpdateHandCollision(out Vector3 palmPos))
            {
                if (TryGetClosestGrabbableByOrigin(palmPos, handHits, out newObject))
                {
                    if (!handInputSource.collidedObject || !handInputSource.collidedObject.Equals(newObject))
                    {
                        BdkLogger.Log($"Hand entered contact with new object: {newObject.name}:{handHits.Length}", LogSeverity.Info);
                        if (handInputSource.collidedObject != null)
                        {
                            handGestureListener.ProximityEnded(handInputSource);
                        }

                        handInputSource.collidedObject = newObject;
                        handGestureListener.ProximityStarted(handInputSource);

                        handInputSource.collidedObject = newObject;
                    }
                    inProximity = true;
                    HandInProximityPreviousUpdate = true;
                }
            }
            else if (!CurrentlyManipulating && HandInProximityPreviousUpdate)
            {
                HandInProximityPreviousUpdate = false;
                handGestureListener.ProximityEnded(handInputSource);
                handInputSource.collidedObject = null;
            }

            if (inProximity)
            {
                handGestureListener.ProximityUpdated(handInputSource);
            }
        }

        private bool TryGetClosestInteractibleByOrigin(Vector3 worldPosition, Collider[] colliders, out GameObject newObject)
        {
            newObject = null;
            bool somethingInProximity = false;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i]
                    && !colliders[i].transform.gameObject.tag.Equals("IgnoreProximity")
                    && colliders[i].transform.gameObject.GetComponent<Interactable>()
                    && closestDistance > Vector3.Distance(worldPosition, colliders[i].transform.position))
                {
                    closestDistance = Vector3.Distance(worldPosition, colliders[i].transform.position);
                    newObject = colliders[i].transform.gameObject;
                    somethingInProximity = true;
                }
            }
            return somethingInProximity;
        }

        private bool TryGetClosestGrabbableByOrigin(Vector3 worldPosition, Collider[] colliders, out GameObject newObject)
        {
            newObject = null;
            bool somethingInProximity = false;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i]
                    && !colliders[i].transform.gameObject.tag.Equals("IgnoreProximity")
                    && (colliders[i].transform.gameObject.GetComponent<Grabbable>() || colliders[i].transform.gameObject.GetComponent<ReferToGrabbable>())
                    && closestDistance > Vector3.Distance(worldPosition, colliders[i].transform.position))
                {
                    closestDistance = Vector3.Distance(worldPosition, colliders[i].transform.position);
                    newObject = colliders[i].transform.gameObject;
                    somethingInProximity = true;
                }
            }
            return somethingInProximity;
        }

        private void MoveGripPoint()
        {
#if UNITY_EDITOR
            if (isRightHand)
            {
                inputManager.rightGripPoint.position = transform.position;
            }
            else
            {
                inputManager.leftGripPoint.position = transform.position;
            }
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            if (!interactionBeam || !interactionBeam.holdingSomething)
            {
                if (inputManager.TryGetHandJointTransform(handInputSource.inputSourceKind, JointName.MiddleMetacarpal, out Vector3 palmPos, out quaternionCache))
                {
                    if (isRightHand)
                    {
                        inputManager.rightGripPoint.position = palmPos;
                    }
                    else
                    {
                        inputManager.leftGripPoint.position = palmPos;
                    }
                }
            }
#pragma warning restore CS0162 // Unreachable code detected
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
                SetupIndexFingerInputSource();
                setupComplete = true;
                BdkLogger.Log($"Found HandInputSource", LogSeverity.Info);
            }
            else
            {
                BdkLogger.Log($"Couldn't find HandInputSource. Trying again!", LogSeverity.Info);
                Invoke(nameof(Setup), 0.2f);
            }
        }

        private void SetupIndexFingerInputSource()
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