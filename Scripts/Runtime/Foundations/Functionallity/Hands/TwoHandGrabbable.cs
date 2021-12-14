using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
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
                if ((inputSource.inputSourceKind == InputSourceKind.HandRight || inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight) && currentState != HandInteractionMode.Right)
                {
                    gripPointR = inputManager.rightGripPoint;
                    gripOffsetR = gripPointR.position - Anchor.position;
                    currentState = HandInteractionMode.Everything;
                }
                else if ((inputSource.inputSourceKind == InputSourceKind.HandLeft || inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft) && currentState != HandInteractionMode.Left)
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
            if (beingHeld && (inputSource.inputSourceKind == primaryInputSource.inputSourceKind || inputSource.inputSourceKind == secondaryInputSource.inputSourceKind))
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

        public override void EndInteraction(InputSource inputSource)
        {
            // TODO: ERROR SOMEWHERE HERE WHEN MOVING HAND OUTSIDE OF FOV WHILE HOLDING AN OBJECT
            if (!beingHeld || !(inputSource.inputSourceKind == primaryInputSource.inputSourceKind || inputSource.inputSourceKind == secondaryInputSource.inputSourceKind))
            {
                return;
            }

            if (currentState == HandInteractionMode.Everything)
            {
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