using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeadGazeButton : InteractableButton
{
#pragma warning disable CS0649
    [Header("Head Gaze properties:")]
    [Range(0.1f, 5f)]
    [SerializeField]
    protected float timeWatchedUntilActivation;

    [SerializeField]
    protected Image radialProgressBar;

    protected bool beingWatched;
    protected float timeWatchBegan;

    public override void Initialize()
    {
        base.Initialize();
        inputManager.OnHololensTransformUpdated += InputManager_OnHololensTransformUpdated;
    }

    protected virtual void InputManager_OnHololensTransformUpdated(InputSource obj)
    {
        if (obj.collidedObjectIdentifier == gameObject.GetInstanceID() && !beingWatched && (inputManager.GetCursorState(true) == CursorState.HeadCursor || inputManager.inputSettings.AlwaysShowHeadGazeCursor))
        {
            beingWatched = true;
            timeWatchBegan = Time.time;
            Invoke(nameof(ResetTimer), timeWatchedUntilActivation);
            StartCoroutine(Loading());
        }
        else if (obj.collidedObjectIdentifier != gameObject.GetInstanceID() && beingWatched)
        {
            CancelInvoke(nameof(ResetTimer));
            StopCoroutine(Loading());
            beingWatched = false;
            radialProgressBar.fillAmount = 0f;
        }
    }

    protected virtual IEnumerator Loading()
    {
        while (beingWatched)
        {
            radialProgressBar.fillAmount = Mathf.Clamp((Time.time - timeWatchBegan) / timeWatchedUntilActivation, 0f, 1f);
            yield return null;
        }
    }

    protected virtual void ResetTimer()
    {
        ButtonClicked();
        beingWatched = false;
        radialProgressBar.fillAmount = 0f;
    }
#pragma warning restore CS0649
}