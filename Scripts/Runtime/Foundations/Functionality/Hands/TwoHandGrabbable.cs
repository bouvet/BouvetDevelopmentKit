using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using System.Collections;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    /// <summary>
    /// Class the inherits from Grabbable. Allows two-handed manipulation and scaling of object, both with interaction beams and directly.
    /// </summary>
    public class TwoHandGrabbable : Grabbable
    {
        [Header("Scaling behaviour")]
        public bool allowScaling;
        public float minimumScaleInXAxis = 0.1f;
        public float maximumScaleInXAxis = 1f;

        protected InputSource secondaryInputSource;
        protected float initialGripDistance;
        protected Vector3 initialScale;
        protected float initialRotation;
        protected float scaleFactor;
        protected Vector3 tempScale;

        private bool autoLerping = false;
        private float tweenDuration;
        private Vector3 autoTweenPosition;

        public override void Initialize()
        {
            base.Initialize();
            secondaryInputSource = new InputSource();
        }

        public void SetupGrabbable(bool canRotate, bool scale, float minScale, float maxScale)
        {
            rotate = canRotate;
            allowScaling = scale;
            minimumScaleInXAxis = minScale;
            maximumScaleInXAxis = maxScale;
        }

        protected float GetHandRotation()
        {
            Vector3 rPos;
            Vector3 lPos;
            Quaternion quat;
            inputManager.TryGetHandJointTransform(InputSourceKind.HandRight, JointName.IndexTip, out rPos, out quat);
            inputManager.TryGetHandJointTransform(InputSourceKind.HandLeft, JointName.IndexTip, out lPos, out quat);

            quat = Quaternion.LookRotation(lPos - rPos, Vector3.up);

            return quat.eulerAngles.y;
        }

        public override void BeginInteraction(InputSource inputSource)
        {
            if (!gameObject.Equals(inputSource.collidedObject))
            {
                return;
            }

            if (beingHeld)
            {
                if ((inputSource.inputSourceKind == InputSourceKind.HandRight
                    || inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight) && currentState != HandInteractionMode.Right)
                {
                    gripPointR = inputManager.rightGripPoint;
                    gripOffsetR = gripPointR.position - Anchor.position;
                    currentState = HandInteractionMode.Everything;
                }
                else if ((inputSource.inputSourceKind == InputSourceKind.HandLeft
                    || inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft) && currentState != HandInteractionMode.Left)
                {
                    gripPointL = inputManager.leftGripPoint;
                    gripOffsetL = gripPointL.position - Anchor.position;
                    currentState = HandInteractionMode.Everything;
                }

                if (!inputManager.TryGetDistanceBetweenJoints(JointName.IndexTip, out initialGripDistance))
                {
                    BdkLogger.Log("Failed to get initial distance between fingertips.", LogSeverity.Warn);
                }

                initialScale = Anchor.localScale;
                initialRotation = GetHandRotation();
                secondaryInputSource.inputSourceKind = inputSource.inputSourceKind;
            }
            else
            {
                base.BeginInteraction(inputSource);
            }
        }

        public override void UpdateInteraction(InputSource inputSource)
        {
            if (autoLerping)
            {
                if (!beingHeld || inputSource.inputSourceKind != primaryInputSource.inputSourceKind || tweenDuration > inputManager.inputSettings.autoTweenTime)
                {
                    autoLerping = false;
                    tweenDuration = 0;
                    return;
                }
                switch (inputManager.inputSettings.currentTweenCurve)
                {
                    case TweenCurve.Linear:
                        switch (currentState)
                        {
                            case HandInteractionMode.Right:
                                Vector3 rotatedGripOffsetR = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetR;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = Vector3.Lerp(autoTweenPosition,
                                    gripPointR.position - rotatedGripOffsetR, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            case HandInteractionMode.Left:
                                Vector3 rotatedGripOffsetL = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetL;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = Vector3.Lerp(autoTweenPosition,
                                    gripPointL.position - rotatedGripOffsetL, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            default:
                                autoLerping = false;
                                tweenDuration = 0;
                                break;
                        }
                        break;
                    case TweenCurve.EaseInOut:
                        switch (currentState)
                        {
                            case HandInteractionMode.Right:
                                Vector3 rotatedGripOffsetR = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetR;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = TweenHelpers.EaseInOut(autoTweenPosition,
                                    gripPointR.position - rotatedGripOffsetR, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            case HandInteractionMode.Left:
                                Vector3 rotatedGripOffsetL = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetL;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = TweenHelpers.EaseInOut(autoTweenPosition,
                                    gripPointL.position - rotatedGripOffsetL, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            default:
                                autoLerping = false;
                                tweenDuration = 0;
                                break;
                        }
                        break;
                    case TweenCurve.Bezier:
                        switch (currentState)
                        {
                            case HandInteractionMode.Right:
                                Vector3 rotatedGripOffsetR = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetR;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = TweenHelpers.QuadraticBezier(autoTweenPosition,
                                    gripPointR.position - rotatedGripOffsetR, autoTweenPosition, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            case HandInteractionMode.Left:
                                Vector3 rotatedGripOffsetL = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetL;
                                tweenDuration += Time.deltaTime;
                                Anchor.position = TweenHelpers.QuadraticBezier(autoTweenPosition,
                                    gripPointL.position - rotatedGripOffsetL, autoTweenPosition, tweenDuration / inputManager.inputSettings.autoTweenTime);
                                break;
                            default:
                                autoLerping = false;
                                tweenDuration = 0;
                                break;
                        }
                        break;
                }
            }
            else
            {
                if (beingHeld && (inputSource.inputSourceKind == primaryInputSource.inputSourceKind
                    || inputSource.inputSourceKind == secondaryInputSource.inputSourceKind))
                {
                    if (currentState == HandInteractionMode.Everything)
                    {
                        Anchor.position = Vector3.Lerp(Anchor.position, Vector3.Lerp(gripPointR.position - gripOffsetR, gripPointL.position - gripOffsetL, 0.5f), 0.5f);
                        if (allowScaling && inputManager.TryGetDistanceBetweenJoints(JointName.IndexTip, out scaleFactor))
                        {
                            tempScale = initialScale * (1 + 5f * (scaleFactor - initialGripDistance));
                            if (tempScale.x > minimumScaleInXAxis && tempScale.x < maximumScaleInXAxis)
                            {
                                Anchor.localScale = tempScale;
                            }
                        }

                        // TODO: SET UP ROTATION MANAGEMENT WITH CONSTRAINS AND STUFF
                        if (rotate)
                        {
                            Anchor.eulerAngles += new Vector3(0, GetHandRotation() - initialRotation, 0);
                            initialRotation = GetHandRotation();
                        }
                    }
                    else
                    {
                        base.UpdateInteraction(inputSource);
                    }
                }
            }
        }

        public override void EndInteraction(InputSource inputSource)
        {
            // TODO: ERROR SOMEWHERE HERE WHEN MOVING HAND OUTSIDE OF FOV WHILE HOLDING AN OBJECT
            if (!beingHeld || !(inputSource.inputSourceKind == primaryInputSource.inputSourceKind
                || inputSource.inputSourceKind == secondaryInputSource.inputSourceKind))
            {
                return;
            }

            if (currentState == HandInteractionMode.Everything)
            {
                autoLerping = true;
                autoTweenPosition = Anchor.position;
                if (inputSource.inputSourceKind == InputSourceKind.HandRight || inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight)
                {
                    currentState = HandInteractionMode.Left;
                    gripPointR = inputManager.leftGripPoint;
                    if (inputSource.inputSourceKind == primaryInputSource.inputSourceKind)
                    {
                        primaryInputSource.inputSourceKind = secondaryInputSource.inputSourceKind;
                        secondaryInputSource.inputSourceKind = InputSourceKind.None;
                    }
                }
                else if (inputSource.inputSourceKind == InputSourceKind.HandLeft || inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft)
                {
                    currentState = HandInteractionMode.Right;
                    gripPointR = inputManager.rightGripPoint;
                    if (inputSource.inputSourceKind == primaryInputSource.inputSourceKind)
                    {
                        primaryInputSource.inputSourceKind = secondaryInputSource.inputSourceKind;
                        secondaryInputSource.inputSourceKind = InputSourceKind.None;
                    }
                }

                audioSource.PlayOneShot(onInteractionCompleteSound);
            }
            else
            {
                base.EndInteraction(inputSource);
            }
        }
    }
}