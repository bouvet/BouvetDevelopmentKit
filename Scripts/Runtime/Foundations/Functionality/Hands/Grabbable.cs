using System;
using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    /// <summary>
    /// Class that deals with grabbable object. I.e. object that can be grabbed by hands or interaction beams.
    /// </summary>
    public class Grabbable : Interactable
    {
        [Header("Base properties:")]
        public Transform Anchor;
        public AudioClip onInteractionStartSound;
        public AudioClip onInteractionCompleteSound;


        [Header("Movement behaviour:")]
        public bool beingHeld;
        public HandInteractionMode currentState = HandInteractionMode.None;

        [Header("Rotation behaviour:")]
        public bool rotate = true;

        protected AudioSource audioSource;
        protected Vector3 gripOffsetL;
        protected Vector3 gripOffsetR;
        protected Transform gripPointL;
        protected Transform gripPointR;

        private Quaternion originalRotation;

        public override void Initialize()
        {
            base.Initialize();
            if (!Anchor)
            {
                Anchor = transform;
            }

            SetupAudio();
            primaryInputSource = new InputSource();
            inputManager.OnManipulationStarted += BeginInteraction;
            inputManager.OnManipulationUpdated += UpdateInteraction;
            inputManager.OnManipulationEnded += EndInteraction;
        }

        protected virtual void SetupAudio()
        {
            if (!audioSource)
            {
                audioSource = inputManager.Hololens.GetComponent<AudioSource>();
            }

            if (!onInteractionStartSound && UIManager.Instance)
            {
                onInteractionStartSound = UIManager.Instance.StartManipulation;
            }

            if (!onInteractionCompleteSound && UIManager.Instance)
            {
                onInteractionCompleteSound = UIManager.Instance.EndManipulation;
            }
        }

        public override void BeginInteraction(InputSource inputSource)
        {
            if (beingHeld || !gameObject.Equals(inputSource.collidedObject))
            {
                return;
            }

            try
            {
                if ((inputSource.inputSourceKind == InputSourceKind.HandRight && inputManager.GetCursorState(true) == CursorState.IndexFingerCursor)
                 || (inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight && inputManager.GetCursorState(true) == CursorState.InteractionBeamCursor))
                {
                    gripPointR = inputManager.rightGripPoint;
                    currentState = HandInteractionMode.Right;
                    gripOffsetR = gripPointR.position - Anchor.position;
                    gripPointR.rotation = Anchor.rotation;
                }
                else if ((inputSource.inputSourceKind == InputSourceKind.HandLeft && inputManager.GetCursorState(false) == CursorState.IndexFingerCursor)
                      || (inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft && inputManager.GetCursorState(false) == CursorState.InteractionBeamCursor))
                {
                    gripPointL = inputManager.leftGripPoint;
                    currentState = HandInteractionMode.Left;
                    gripOffsetL = gripPointL.position - Anchor.position;
                    gripPointL.rotation = Anchor.rotation;
                }
                else
                {
                    return;
                }

                originalRotation = Anchor.rotation;
                primaryInputSource.inputSourceKind = inputSource.inputSourceKind;
                beingHeld = true;
                audioSource.PlayOneShot(onInteractionStartSound);
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Exception in Grabbable on manipulation start", e);
            }
        }

        public override void UpdateInteraction(InputSource inputSource)
        {
            if (!beingHeld || inputSource.inputSourceKind != primaryInputSource.inputSourceKind)
            {
                return;
            }

            if (currentState == HandInteractionMode.Right)
            {
                Vector3 rotatedGripOffsetR = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetR;
                Anchor.position = Vector3.Lerp(Anchor.position, gripPointR.position - rotatedGripOffsetR, 0.5f);
            }
            else if (currentState == HandInteractionMode.Left)
            {
                Vector3 rotatedGripOffsetL = Anchor.rotation * Quaternion.Inverse(originalRotation) * gripOffsetL;
                Anchor.position = Vector3.Lerp(Anchor.position, gripPointL.position - rotatedGripOffsetL, 0.5f);
            }
        }

        public override void EndInteraction(InputSource inputSource)
        {
            if (!beingHeld || inputSource.inputSourceKind != primaryInputSource.inputSourceKind)
            {
                return;
            }

            primaryInputSource.inputSourceKind = InputSourceKind.None;
            beingHeld = false;
            currentState = HandInteractionMode.None;
            audioSource.PlayOneShot(onInteractionCompleteSound);
        }

        private void OnDestroy()
        {
            if (!inputManager) return;
            inputManager.OnManipulationStarted -= BeginInteraction;
            inputManager.OnManipulationUpdated -= UpdateInteraction;
            inputManager.OnManipulationEnded -= EndInteraction;
        }
    }
}