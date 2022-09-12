using Bouvet.DevelopmentKit.Input;
using System;
using Bouvet.DevelopmentKit.Functionality.Hands;
using Bouvet.DevelopmentKit.Input.Hands;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools.Hands
{
    public class InteractableButtonWithFocusEvents : InteractableButton
    {
        [Header("Toggle Events on:")]
        [SerializeField]
        private bool proximity;
        [SerializeField]
        private bool interactionBeams;
        [SerializeField]
        private bool eyeGaze;
        [SerializeField]
        private bool headGaze;

        public event Action OnFocusStart;
        public event Action OnFocusStop;

        private bool beingTargetedByHeadGaze;

        public override void Initialize()
        {
            base.Initialize();
            inputManager.OnHololensTransformUpdated += InputManager_OnHololensTransformUpdated;
            inputManager.OnGazeEnter += InputManager_OnGazeEnter;
            inputManager.OnGazeExit += InputManager_OnGazeExit;
        }

        private void InputManager_OnGazeExit(InputSource obj)
        {
            if (eyeGaze && gameObject.Equals(obj.collidedObject))
            {
                OnFocusStop?.Invoke();
            }
        }

        private void InputManager_OnGazeEnter(InputSource obj)
        {
            if (eyeGaze && gameObject.Equals(obj.collidedObject))
            {
                OnFocusStart?.Invoke();
            }
        }

        private void InputManager_OnHololensTransformUpdated(InputSource obj)
        {
            if (headGaze && !beingTargetedByHeadGaze && gameObject.Equals(obj.collidedObject))
            {
                beingTargetedByHeadGaze = true;
                OnFocusStart?.Invoke();
            }
            else if (headGaze && beingTargetedByHeadGaze && !gameObject.Equals(obj.collidedObject))
            {
                beingTargetedByHeadGaze = false;
                OnFocusStop?.Invoke();
            }
        }

        public override void OnFocusBegin()
        {
            base.OnFocusBegin();
            if (interactionBeams)
            {
                OnFocusStart?.Invoke();
            }
        }

        public override void OnFocusEnd()
        {
            base.OnFocusEnd();
            if (interactionBeams)
            {
                OnFocusStop?.Invoke();
            }
        }

        protected override void InputManager_OnProximityStarted(InputSource inputSource)
        {
            base.InputManager_OnProximityStarted(inputSource);
            if (proximity && gameObject.Equals(inputSource.collidedObject))
            {
                OnFocusStart?.Invoke();
            }
        }

        protected override void InputManager_OnProximityEnded(InputSource inputSource)
        {
            if (proximity && proximitySource != null && inputSource.inputSourceKind == proximitySource.inputSourceKind)
            {
                OnFocusStop?.Invoke();
            }
            base.InputManager_OnProximityEnded(inputSource);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            inputManager.OnHololensTransformUpdated -= InputManager_OnHololensTransformUpdated;
            inputManager.OnGazeEnter -= InputManager_OnGazeEnter;
            inputManager.OnGazeExit -= InputManager_OnGazeExit;
        }
    }
}