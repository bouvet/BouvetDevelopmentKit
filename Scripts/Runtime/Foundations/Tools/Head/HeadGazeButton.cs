using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using System.Collections;
using Bouvet.DevelopmentKit.Functionality.Hands;
using Bouvet.DevelopmentKit.Input.Hands;
using UnityEngine;
using UnityEngine.UI;

namespace Bouvet.DevelopmentKit.Tools.Head
{
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
            if (gameObject.Equals(obj.collidedObject) && !beingWatched && (inputManager.GetCursorState(true) == CursorState.HeadCursor || inputManager.inputSettings.AlwaysShowHeadGazeCursor))
            {
                beingWatched = true;
                timeWatchBegan = Time.time;
                Invoke(nameof(ResetTimer), timeWatchedUntilActivation);
                StartCoroutine(Loading());
            }
            else if (!gameObject.Equals(obj.collidedObject) && beingWatched)
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
}