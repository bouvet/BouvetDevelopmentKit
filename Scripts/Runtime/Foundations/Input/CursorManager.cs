using Bouvet.DevelopmentKit.Functionality.Hands;
using TMPro;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
#pragma warning disable CS0414
#pragma warning disable CS0649
    public class CursorManager : MonoBehaviour
    {
        public CursorState RightHandCursorState;
        public CursorState LeftHandCursorState;

        [SerializeField]
        public InteractionBeam rightHandInteractionBeam;

        [SerializeField]
        public InteractionBeam leftHandInteractionBeam;

        [SerializeField]
        internal ProximityAndManipulationEvent rightHandManipulation;

        [SerializeField]
        internal ProximityAndManipulationEvent leftHandManipulation;

        [SerializeField]
        private TextMeshPro debugTextField;

        private InputSettings inputSettings;
        private bool itemsInProximityOfLeftHand;
        private bool itemsInProximityOfRightHand;
        private bool leftHandActive;

        private bool rightHandActive;

        private void Start()
        {
            InputManager inputManager = InputManager.Instance;
            inputSettings = inputManager.inputSettings;
            inputManager.OnSourceFound += InputManager_OnSourceFound;
            inputManager.OnSourceLost += InputManager_OnSourceLost;
        }

        private void Update()
        {
#if !UNITY_EDITOR
            try
            {
                if (rightHandActive)
                {
                    if (inputSettings.UseInteractionBeams && rightHandManipulation && !rightHandManipulation.CurrentlyManipulating 
                        && (rightHandInteractionBeam.handRotation > 60f || rightHandInteractionBeam.holdingSomething)
                        && (!itemsInProximityOfRightHand || rightHandInteractionBeam.holdingSomething))
                    {
                        RightHandCursorState = CursorState.InteractionBeamCursor;
                    }
                    else if (itemsInProximityOfRightHand || rightHandManipulation.CurrentlyManipulating)
                    {
                        RightHandCursorState = CursorState.IndexFingerCursor;
                    }
                    else
                    {
                        RightHandCursorState = CursorState.None;
                    }
                }
                else
                {
                    RightHandCursorState = CursorState.None;
                }

                if (leftHandActive)
                {
                    if (inputSettings.UseInteractionBeams && leftHandManipulation && !leftHandManipulation.CurrentlyManipulating 
                        && (leftHandInteractionBeam.handRotation > 60f || leftHandInteractionBeam.holdingSomething)
                        && (!itemsInProximityOfLeftHand || leftHandInteractionBeam.holdingSomething))
                    {
                        LeftHandCursorState = CursorState.InteractionBeamCursor;
                    }
                    else if (itemsInProximityOfLeftHand || leftHandManipulation.CurrentlyManipulating)
                    {
                        LeftHandCursorState = CursorState.IndexFingerCursor;
                    }
                    else
                    {
                        LeftHandCursorState = CursorState.None;
                    }
                }
                else
                {
                    LeftHandCursorState = CursorState.None;
                }

                if (!rightHandActive && !leftHandActive)
                {
                    if (inputSettings.ShowHeadGazeCursor)
                    {
                        RightHandCursorState = CursorState.HeadCursor;
                        LeftHandCursorState = CursorState.HeadCursor;
                    }
                    else if (inputSettings.ShowEyeGazeCursor)
                    {
                        RightHandCursorState = CursorState.EyeCursor;
                        LeftHandCursorState = CursorState.EyeCursor;
                    }
                }

                if (debugTextField)
                {
                    debugTextField.text = $"Left cursor: {LeftHandCursorState}/{itemsInProximityOfLeftHand} --- Right cursor: {RightHandCursorState}/{itemsInProximityOfRightHand}";
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error in CursorManager.", e);
            }
#endif
        }

        private void InputManager_OnSourceFound(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == InputSourceKind.HandRight)
            {
                rightHandActive = true;
            }

            if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
            {
                leftHandActive = true;
            }
        }

        private void InputManager_OnSourceLost(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == InputSourceKind.HandRight)
            {
                rightHandActive = false;
                itemsInProximityOfRightHand = false;
            }

            if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
            {
                leftHandActive = false;
                itemsInProximityOfLeftHand = false;
            }
        }

        internal void EnterProximity(bool isRightHand)
        {
            if (isRightHand)
            {
                itemsInProximityOfRightHand = true;
            }
            else
            {
                itemsInProximityOfLeftHand = true;
            }
        }

        internal void ExitProximity(bool isRightHand)
        {
            if (isRightHand)
            {
                itemsInProximityOfRightHand = false;
            }
            else
            {
                itemsInProximityOfLeftHand = false;
            }
        }
    }
#pragma warning restore CS0414
#pragma warning restore CS0649
}