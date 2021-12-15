using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
#pragma warning disable CS0649
    public class Slider : Grabbable
    {
        [Header("Slider properties:")]
        [SerializeField]
        protected PinchSlider ps;

        [SerializeField]
        protected AudioClip notchSound;

        [SerializeField]
        protected bool snapToNotches;

        public float currentValue;

        protected int currentNotch;
        protected float length;

        protected float notchDistance;
        protected float offset;
        protected float previousPosition;
        protected float scale;
        private Quaternion quaternionCache;

        public override void Initialize()
        {
            base.Initialize();
            Vector3 position = ps.trackBottom.localPosition;
            Vector3 localPosition = ps.trackTop.localPosition;
            offset = -position.y;
            scale = 1f / (localPosition.y + offset);
            length = localPosition.y - position.y;
            currentValue = scale * (transform.localPosition.y + offset);
            notchDistance = scale * (localPosition.y - position.y) / ps.amountOfDots;
            SnapToNotch();
        }

        public override void UpdateInteraction(InputSource inputSource)
        {
            if (beingHeld && inputSource.inputSourceKind == primaryInputSource.inputSourceKind)
            {
                if (inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft)
                {
                    Anchor.position = Vector3.Lerp(Anchor.position, gripPointL.position, 0.5f);
                } 
                else if (inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight)
                {
                    Anchor.position = Vector3.Lerp(Anchor.position, gripPointR.position, 0.5f);
                } 
                else if ((inputManager.TryGetHandJointTransform(currentState == HandInteractionMode.Right ? InputSourceKind.HandRight : InputSourceKind.HandLeft, JointName.IndexTip, out Vector3 indexPos, out quaternionCache)
                    && inputManager.TryGetHandJointTransform(currentState == HandInteractionMode.Right ? InputSourceKind.HandRight : InputSourceKind.HandLeft, JointName.ThumbTip, out Vector3 thumbPos, out quaternionCache)))
                {
                    Anchor.position = Vector3.Lerp(Anchor.position, Vector3.Lerp(indexPos, thumbPos, 0.5f), 0.5f);
                }

                if (snapToNotches)
                {
                    SnapToNotch();
                }
                else
                {
                    Anchor.localPosition = Vector3.up * Mathf.Clamp(Anchor.localPosition.y, ps.trackBottom.localPosition.y, ps.trackTop.localPosition.y);
                    UpdateValue();
                }

                if (Mathf.Abs(previousPosition - Anchor.localPosition.y) > 0.9f * notchDistance)
                {
                    previousPosition = Anchor.localPosition.y;
                    audioSource.PlayOneShot(notchSound);
                }
            }
        }

        public override void EndInteraction(InputSource inputSource)
        {
            if (!beingHeld || inputSource.inputSourceKind != primaryInputSource.inputSourceKind)
            {
                return;
            }

            base.EndInteraction(inputSource);
            previousPosition = Anchor.localPosition.y;
            audioSource.PlayOneShot(notchSound);
        }

        protected virtual void SnapToNotch()
        {
            if (ps.dotHeights.Count == 0) return;
            currentNotch = Mathf.Clamp(Mathf.FloorToInt(scale * (Anchor.localPosition.y + offset) * ps.amountOfDots), 0, ps.amountOfDots - 1);
            Anchor.localPosition = Vector3.up * Mathf.Clamp(-ps.dotHeights[currentNotch], ps.trackBottom.localPosition.y, ps.trackTop.localPosition.y);
            currentValue = scale * (Anchor.localPosition.y + offset);
        }

        public void SetPercentage(float newValue)
        {
            currentValue = Mathf.Clamp(newValue, 0f, 1f);
            Anchor.localPosition = Vector3.up * ((currentValue / scale) - offset);
        }

        public virtual void UpdateValue()
        {
            currentValue = scale * (Anchor.localPosition.y + offset);
        }
    }
#pragma warning restore CS0649
}