using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Input.Gaze;
using Bouvet.DevelopmentKit.Input.Hands;
using UnityEditor;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    [RequireComponent(typeof(InputManager))]
    public class FunctionalityCreator : MonoBehaviour
    {
        [Header("General")]
        public InputManager inputManager;

        [Header("Prefabs")]
        public GameObject indexPrefab;

        public GameObject handRayPrefab;
        public GameObject eyeGazePrefab;
        public GameObject headGazePrefab;
        public GameObject spatialMapPrefab;
        public GameObject handJointsPrefab;

        [Header("Reference Fields")]
        [SerializeField]
        internal GameObject LIndex;

        [SerializeField]
        internal GameObject LHandRay;

        [SerializeField]
        internal GameObject RIndex;

        [SerializeField]
        internal GameObject RHandRay;

        [SerializeField]
        internal GameObject SpatialMap;

        [SerializeField]
        internal GameObject EyeGazeCursor;

        [SerializeField]
        internal GameObject HeadGazeCursor;

        [SerializeField]
        internal GameObject HandJoints;

        internal void OnValidate()
        {
#if UNITY_EDITOR
            if (!inputManager)
            {
                inputManager = GetComponent<InputManager>();
            }

            // Spatial Map
            if (inputManager.inputSettings.UseSpatialMap && !SpatialMap)
            {
                SpatialMap = Instantiate(spatialMapPrefab);
            }
            else if (!inputManager.inputSettings.UseSpatialMap && SpatialMap)
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(SpatialMap.gameObject);
                };
            }

            CursorManager cursorManager = inputManager.gameObject.GetComponent<CursorManager>();
            if (!cursorManager)
            {
                cursorManager = inputManager.gameObject.AddComponent<CursorManager>();
            }

            inputManager.inputSettings.CursorManager = cursorManager;

            // Hand tracking
            if (inputManager.inputSettings.UseHands && indexPrefab && !LIndex && !RIndex)
            {
                LIndex = Instantiate(indexPrefab, transform);
                LIndex.GetComponent<AttachedToJoint>().inputManager = inputManager;
                LIndex.GetComponent<AttachedToJoint>().inputSourceKind = InputSourceKind.HandLeft;
                LIndex.GetComponentInChildren<ProximityAndManipulationEvent>().inputManager = inputManager;
                LIndex.GetComponentInChildren<ProximityAndManipulationEvent>().isRightHand = false;
                LIndex.GetComponentInChildren<Indicator>().isRightHand = false;
                cursorManager.leftHandManipulation = LIndex.GetComponentInChildren<ProximityAndManipulationEvent>();


                RIndex = Instantiate(indexPrefab, transform);
                RIndex.GetComponent<AttachedToJoint>().inputManager = inputManager;
                RIndex.GetComponent<AttachedToJoint>().inputSourceKind = InputSourceKind.HandRight;
                RIndex.GetComponentInChildren<ProximityAndManipulationEvent>().inputManager = inputManager;
                RIndex.GetComponentInChildren<ProximityAndManipulationEvent>().isRightHand = true;
                RIndex.GetComponentInChildren<Indicator>().isRightHand = true;
                cursorManager.rightHandManipulation = RIndex.GetComponentInChildren<ProximityAndManipulationEvent>();
            }
            else if (!inputManager.inputSettings.UseHands && (LIndex || RIndex))
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(LIndex.gameObject);
                    DestroyImmediate(RIndex.gameObject);
                };
            }

            // Showing hands (for debugging)
            if (inputManager.inputSettings.ShowHandJoints && !HandJoints)
            {
                HandJoints = Instantiate(handJointsPrefab, transform);
            }
            else if (!inputManager.inputSettings.ShowHandJoints && HandJoints)
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(HandJoints.gameObject);
                };
            }

            // Interaction beams
            if (inputManager.inputSettings.UseInteractionBeams)
            {
                if (!LHandRay)
                {
                    LHandRay = Instantiate(handRayPrefab, transform);
                    LHandRay.GetComponent<InteractionBeam>().inputManager = inputManager;
                    LHandRay.GetComponent<InteractionBeam>().isRightHand = false;
                    LIndex.GetComponentInChildren<ProximityAndManipulationEvent>().interactionBeam = LHandRay.GetComponent<InteractionBeam>();

                    cursorManager.leftHandInteractionBeam = LHandRay.GetComponent<InteractionBeam>();
                }

                if (!RHandRay)
                {
                    RHandRay = Instantiate(handRayPrefab, transform);
                    RHandRay.GetComponent<InteractionBeam>().inputManager = inputManager;
                    RHandRay.GetComponent<InteractionBeam>().isRightHand = true;
                    RIndex.GetComponentInChildren<ProximityAndManipulationEvent>().interactionBeam = RHandRay.GetComponent<InteractionBeam>();

                    cursorManager.rightHandInteractionBeam = RHandRay.GetComponent<InteractionBeam>();
                }
            }
            else if (!inputManager.inputSettings.UseInteractionBeams && (LHandRay || RHandRay))
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(LHandRay.gameObject);
                    DestroyImmediate(RHandRay.gameObject);
                };
            }

            // Eye gaze
            if (inputManager.inputSettings.UseEyeGaze && !EyeGazeCursor)
            {
                EyeGazeCursor = Instantiate(eyeGazePrefab, transform);
                EyeGazeCursor.GetComponent<EyeGazeCursor>().inputManager = inputManager;
            }
            else if (!inputManager.inputSettings.UseEyeGaze && EyeGazeCursor)
            {
                EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(EyeGazeCursor.gameObject);
                };
            }

            // Head gaze
            if (inputManager.inputSettings.UseHeadGaze && !inputManager.Hololens.gameObject.GetComponent<HololensTransformUpdate>()
                || inputManager.inputSettings.ShowHeadGazeCursor && !HeadGazeCursor)
            {
                if (!inputManager.Hololens.gameObject.GetComponent<HololensTransformUpdate>())
                {
                    inputManager.Hololens.gameObject.AddComponent<HololensTransformUpdate>().inputManager = inputManager;
                }

                if (inputManager.inputSettings.ShowHeadGazeCursor && !HeadGazeCursor)
                {
                    HeadGazeCursor = Instantiate(headGazePrefab, transform);
                    HeadGazeCursor.GetComponent<HeadGazeCursor>().inputManager = inputManager;
                }
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    if (!inputManager.inputSettings.UseHeadGaze && inputManager.Hololens && inputManager.Hololens.gameObject.GetComponent<HololensTransformUpdate>())
                    {
                        DestroyImmediate(inputManager.Hololens.gameObject.GetComponent<HololensTransformUpdate>());
                    }

                    if (HeadGazeCursor && !inputManager.inputSettings.ShowHeadGazeCursor)
                    {
                        DestroyImmediate(HeadGazeCursor.gameObject);
                    }
                };
            }
#endif
        }
    }
}