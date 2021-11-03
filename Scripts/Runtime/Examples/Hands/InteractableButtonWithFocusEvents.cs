using Bouvet.DevelopmentKit.Input;
using System;
using UnityEngine;

public class InteractableButtonWithFocusEvents : InteractableButton
{
    [Header("Focus events")]
    public bool ToggleEventsOnProximity;
    public bool ToggleEventsOnInteractionBeams;
    public bool ToggleEventsOnEyeGaze;
    public bool ToggleEventsOnHeadGaze;

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
        if (ToggleEventsOnEyeGaze && obj.collidedObjectIdentifier == inputManager.GetId(gameObject))
        {
            OnFocusStop?.Invoke();
        }
    }

    private void InputManager_OnGazeEnter(InputSource obj)
    {
        if (ToggleEventsOnEyeGaze && obj.collidedObjectIdentifier == inputManager.GetId(gameObject))
        {
            OnFocusStart?.Invoke();
        }
    }

    private void InputManager_OnHololensTransformUpdated(InputSource obj)
    {
        if (ToggleEventsOnHeadGaze && !beingTargetedByHeadGaze && obj.collidedObjectIdentifier == inputManager.GetId(gameObject))
        {
            beingTargetedByHeadGaze = true;
            OnFocusStart?.Invoke();
        }
        else if (ToggleEventsOnHeadGaze && beingTargetedByHeadGaze && obj.collidedObjectIdentifier != inputManager.GetId(gameObject))
        {
            beingTargetedByHeadGaze = false;
            OnFocusStop?.Invoke();
        }
    }

    public override void OnFocusBegin()
    {
        base.OnFocusBegin();
        if (ToggleEventsOnInteractionBeams)
        {
            OnFocusStart?.Invoke();
        }
    }

    public override void OnFocusEnd()
    {
        base.OnFocusEnd();
        if (ToggleEventsOnInteractionBeams)
        {
            OnFocusStop?.Invoke();
        }
    }

    protected override void InputManager_OnProximityStarted(InputSource inputSource)
    {
        base.InputManager_OnProximityStarted(inputSource);
        if (ToggleEventsOnProximity && inputSource.collidedObjectIdentifier == inputManager.GetId(gameObject))
        {
            OnFocusStart?.Invoke();
        }
    }

    protected override void InputManager_OnProximityEnded(InputSource inputSource)
    {
        if (ToggleEventsOnProximity && proximitySource != null && inputSource.inputSourceKind == proximitySource.inputSourceKind)
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
