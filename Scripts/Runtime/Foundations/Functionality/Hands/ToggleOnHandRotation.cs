using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using System;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    public class ToggleOnHandRotation : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        protected HandInteractionMode Hand;

        [SerializeField]
        protected GameObject ComponentsToToggle;

        [SerializeField]
        [Tooltip("The minimum angle of your hand to activate the object. Palm facing towards Hololens will be 0 degrees.")]
        protected float minAngleToActivate = 0;

        [SerializeField]
        [Tooltip("The maximum angle of your hand to activate the object. Palm facing away from the Hololens will have a max angle of 180 degrees.")]
        protected float maxAngleToActivate = 60;

        [SerializeField]
        protected bool disableOnPinch;

        [SerializeField]
        protected bool disableInUnityEditor = true;

        public event Action<HandInteractionMode> OnChangedMainHand;

        protected InputManager inputManager;
        protected bool attachedToRightHand;
        protected bool menuCurrentlyConnectedToAHand;
        protected bool rightHandVisible;
        protected bool leftHandVisible;
        protected bool rightHandBusy;
        protected bool leftHandBusy;

        protected float leftHandState;
        protected float rightHandState;

        private void Start()
        {
    #if UNITY_EDITOR
            if (disableInUnityEditor)
            {
                this.enabled = false;
                return;
            }
    #endif
            inputManager = InputManager.Instance;
            inputManager.OnHandRotationToggle += InputManager_OnHandRotationToggle;
            inputManager.OnSourceLost += InputManager_OnSourceLost;
            inputManager.OnSourceFound += InputManager_OnSourceFound;
            if (disableOnPinch)
            {
                inputManager.OnInputDown += InputManager_OnInputDown;
                inputManager.OnInputUp += InputManager_OnInputUp;
            }

            if (ComponentsToToggle)
            {
                ComponentsToToggle.SetActive(false);
            }

            Initialize();
        }

        protected virtual void Initialize()
        {
        }

        protected void InputManager_OnInputUp(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == InputSourceKind.HandRight)
            {
                rightHandBusy = false;
            }

            if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
            {
                leftHandBusy = false;
            }
        }

        protected void InputManager_OnInputDown(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == InputSourceKind.HandRight)
            {
                rightHandBusy = true;
                if (menuCurrentlyConnectedToAHand && attachedToRightHand)
                {
                    menuCurrentlyConnectedToAHand = false;
                }
            }

            if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
            {
                leftHandBusy = true;
                if (menuCurrentlyConnectedToAHand && !attachedToRightHand)
                {
                    menuCurrentlyConnectedToAHand = false;
                }
            }
        }

        protected void InputManager_OnSourceFound(InputSource inputSource)
        {
            if ((inputSource.inputSourceKind == InputSourceKind.HandLeft && (Hand.HasFlag(HandInteractionMode.Left)))
                || (inputSource.inputSourceKind == InputSourceKind.HandRight && (Hand.HasFlag(HandInteractionMode.Right))))
            {
                if (inputSource.inputSourceKind == InputSourceKind.HandRight)
                {
                    rightHandVisible = true;
                }

                if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
                {
                    leftHandVisible = true;
                }
            }
        }

        protected void InputManager_OnSourceLost(InputSource inputSource)
        {
            if ((inputSource.inputSourceKind == InputSourceKind.HandLeft && (Hand.HasFlag(HandInteractionMode.Left)))
                || (inputSource.inputSourceKind == InputSourceKind.HandRight && (Hand.HasFlag(HandInteractionMode.Right))))
            {
                if (inputSource.inputSourceKind == InputSourceKind.HandRight)
                {
                    rightHandVisible = false;
                    if (attachedToRightHand)
                    {
                        ComponentsToToggle.SetActive(false);
                    }
                }

                if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
                {
                    leftHandVisible = false;
                    if (!attachedToRightHand)
                    {
                        ComponentsToToggle.SetActive(false);
                    }
                }
            }
        }

        protected virtual void InputManager_OnHandRotationToggle(InputSource inputSource, float handRotationState)
        {
            if ((inputSource.inputSourceKind == InputSourceKind.HandLeft && (Hand.HasFlag(HandInteractionMode.Left)))
                || (inputSource.inputSourceKind == InputSourceKind.HandRight && (Hand.HasFlag(HandInteractionMode.Right))))
            {
                bool newState = false;

                if (inputSource.inputSourceKind == InputSourceKind.HandRight)
                {
                    rightHandState = handRotationState;
                }

                if (inputSource.inputSourceKind == InputSourceKind.HandLeft)
                {
                    leftHandState = handRotationState;
                }

                // Deactivates if palm is rotated away from Hololens
                if (menuCurrentlyConnectedToAHand)
                {
                    if (inputSource.inputSourceKind == InputSourceKind.HandRight && attachedToRightHand)
                    {
                        if (handRotationState > minAngleToActivate && handRotationState < maxAngleToActivate)
                        {
                            newState = true;
                        }

                        menuCurrentlyConnectedToAHand = newState;
                    }

                    if (inputSource.inputSourceKind == InputSourceKind.HandLeft && !attachedToRightHand)
                    {

                        if (handRotationState > minAngleToActivate && handRotationState < maxAngleToActivate)
                        {
                            newState = true;
                        }

                        menuCurrentlyConnectedToAHand = newState;
                    }
                }

                if (!menuCurrentlyConnectedToAHand && rightHandVisible && !rightHandBusy)
                {
                    if (handRotationState > minAngleToActivate && handRotationState < maxAngleToActivate)
                    {
                        newState = true;
                    }

                    if (newState)
                    {
                        attachedToRightHand = true;
                        menuCurrentlyConnectedToAHand = true;
                        OnChangedMainHand?.Invoke(HandInteractionMode.Right);
                    }
                }

                if (!menuCurrentlyConnectedToAHand && leftHandVisible && !leftHandBusy)
                {
                    if (handRotationState > minAngleToActivate && handRotationState < maxAngleToActivate)
                    {
                        newState = true;
                    }

                    if (newState)
                    {
                        attachedToRightHand = false;
                        menuCurrentlyConnectedToAHand = true;
                        OnChangedMainHand?.Invoke(HandInteractionMode.Left);
                    }
                }

                if (menuCurrentlyConnectedToAHand && !ComponentsToToggle.activeSelf)
                {
                    ComponentsToToggle.SetActive(true);
                }

                if (!menuCurrentlyConnectedToAHand && ComponentsToToggle.activeSelf)
                {
                    ComponentsToToggle.SetActive(false);
                }
            }
        }
#pragma warning restore CS0649
    }
}