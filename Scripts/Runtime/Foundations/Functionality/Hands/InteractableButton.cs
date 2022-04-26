using System;
using System.Collections;
using System.Linq;
using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    /// <summary>
    /// Button that can be pressed either by the index finger directly or by air tapping while pointing at it with the interaction beam.
    /// </summary>
    public class InteractableButton : Interactable
    {
#pragma warning disable CS0649
        [Header("Base properties:")]
        [SerializeField]
        protected Transform compressable;

        [SerializeField]
        public UnityEvent OnClicked;

        [SerializeField]
        protected float minTimeBetweenPresses = 0.2f;

        [SerializeField]
        [Range(10f, 60f)]
        protected float ButtonUnpressAnimationSpeed = 20f;

        [SerializeField]
        [Range(0.1f, 0.5f)]
        protected float buttonCompressionBeforeClick = 0.3f;

        [Header("Audio properties:")]

        [SerializeField]
        protected AudioClip ProximityStart;

        [SerializeField]
        protected bool PlayAudioOnProximityStart = false;

        [SerializeField]
        protected AudioClip ProximityEnd;

        [SerializeField]
        protected bool PlayAudioOnProximityEnd = false;

        [SerializeField]
        protected AudioClip ButtonClick;

        [SerializeField]
        protected bool PlayAudioOnButtonClicked = true;

        [Header("Air tap functionality:")]
        [SerializeField]
        protected bool AnimateButtonOnAirTap = true;

        [SerializeField]
        [Range(10f, 60f)]
        protected float AirTapButtonAnimationSpeed = 25f;

        [SerializeField]
        protected ButtonInteractionBeamMode WhenToCallButtonClickEvent = ButtonInteractionBeamMode.OnInputUpLocked;

        protected bool currentlyAnimating;
        protected bool active;
        protected bool blockButton;
        protected bool InProximity;
        protected bool buttonWasPressedPhysically;
        protected AudioSource audioSource;
        protected InputSource proximitySource;
        protected Vector3 maxCompression;

        /// <summary>
        /// Quick fix for buttons missing compressable field.
        /// </summary>
        private void OnValidate()
        {
            if(!compressable)
            {
                compressable = GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name.Equals("Compressable"));
            }
        }

        // Start is called before the first frame update
        public override void Initialize()
        {
            base.Initialize();
            SetupAudio();
            OnEnable();
            maxCompression = new Vector3(1f, 1f, buttonCompressionBeforeClick);
        }

        protected virtual void SetupAudio()
        {
            if (!audioSource)
            {
                audioSource = inputManager.Hololens.GetComponent<AudioSource>();
            }

            if (PlayAudioOnProximityStart && !ProximityStart && UIManager.Instance)
            {
                ProximityStart = UIManager.Instance.StartProximity;
            }

            if (PlayAudioOnProximityEnd && !ProximityEnd && UIManager.Instance)
            {
                ProximityEnd = UIManager.Instance.EndProximity;
            }

            if (PlayAudioOnButtonClicked && !ButtonClick && UIManager.Instance)
            {
                ButtonClick = UIManager.Instance.ButtonPressed;
            }
        }

        private void OnInput(InputSource obj)
        {
            try
            {
                if (gameObject.Equals(obj.collidedObject))
                {
                    TryPressButton();
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error in OnInput in InteractableButton. ", e);
            }
        }

        private void OnInputDownLocked(InputSource obj)
        {
            if (gameObject.Equals(obj.collidedObject))
            {
                primaryInputSource = obj;
            }
        }

        private void OnInputUpLocked(InputSource obj)
        {
            if (gameObject.Equals(obj.collidedObject) && primaryInputSource != null && primaryInputSource.inputSourceKind == obj.inputSourceKind)
            {
                TryPressButton();
                primaryInputSource = null;
            }
        }

        protected virtual void TryPressButton()
        {
            if (!blockButton)
            {
                blockButton = true;
                Invoke(nameof(UnblockButton), minTimeBetweenPresses);

                OnClicked?.Invoke();

                if (AnimateButtonOnAirTap)
                {
                    if (!currentlyAnimating && gameObject.activeInHierarchy)
                    {
                        StartCoroutine(PressButton());
                    }
                }
                else
                {
                    if (PlayAudioOnButtonClicked)
                        audioSource?.PlayOneShot(ButtonClick);
                }
            }
        }

        protected virtual void InputManager_OnProximityUpdated(InputSource inputSource)
        {
            if (InProximity && inputSource.inputSourceKind == proximitySource.inputSourceKind)
            {
                compressable.localScale = new Vector3(1f, 1f, Mathf.Clamp(-transform.InverseTransformPoint(inputSource.worldPosition).z, 0f, 1f));
                if (compressable.localScale.z < maxCompression.z)
                {
                    compressable.localScale = maxCompression;
                    if (!blockButton)
                    {
                        TryPressButton();
                        InputManager_OnProximityEnded(inputSource);
                    }
                }
            }
        }

        protected virtual void InputManager_OnProximityStarted(InputSource inputSource)
        {
            if (!InProximity && (inputSource.inputSourceKind == InputSourceKind.IndexFingerLeft || inputSource.inputSourceKind == InputSourceKind.IndexFingerRight) && gameObject.Equals(inputSource.collidedObject) && 0 > transform.InverseTransformPoint(inputSource.worldPosition).z)
            {
                InProximity = true;
                proximitySource = inputSource;
                if (PlayAudioOnProximityStart)
                    audioSource?.PlayOneShot(ProximityStart);
            }
        }

        protected virtual void InputManager_OnProximityEnded(InputSource inputSource)
        {
            if (InProximity && inputSource.inputSourceKind == proximitySource.inputSourceKind)
            {
                InProximity = false;
                buttonWasPressedPhysically = true;
                StartCoroutine(nameof(UnPressButton));
                if (PlayAudioOnProximityEnd)
                    audioSource?.PlayOneShot(ProximityEnd);
            }
        }

        public virtual IEnumerator PressButton()
        {
            currentlyAnimating = true;
            while (gameObject.activeInHierarchy && compressable.localScale.z > maxCompression.z)
            {
                compressable.localScale = Vector3.Lerp(compressable.localScale, Vector3.up + Vector3.right, AirTapButtonAnimationSpeed * Time.deltaTime);
                yield return null;
            }
            currentlyAnimating = false;
            ButtonClicked();
        }


        public virtual void ButtonClicked()
        {
            if (PlayAudioOnButtonClicked)
                audioSource?.PlayOneShot(ButtonClick);
            if (compressable.localScale != Vector3.one)
            {
                StartCoroutine(nameof(UnPressButton));
            }
        }

        protected virtual IEnumerator UnPressButton()
        {
            currentlyAnimating = true;
            while (compressable.localScale.z < 0.99f)
            {
                compressable.localScale = Vector3.Lerp(compressable.localScale, Vector3.one, (buttonWasPressedPhysically ? ButtonUnpressAnimationSpeed : AirTapButtonAnimationSpeed) * Time.deltaTime);
                yield return null;
            }
            buttonWasPressedPhysically = false;
            currentlyAnimating = false;
            compressable.localScale = Vector3.one;
        }

        private void UnblockButton()
        {
            blockButton = false;
        }

        protected virtual void OnEnable()
        {
            if (!inputManager)
                inputManager = InputManager.Instance;

            if (!inputManager || active) return;
            active = true;
            inputManager.OnProximityStarted += InputManager_OnProximityStarted;
            inputManager.OnProximityUpdated += InputManager_OnProximityUpdated;
            inputManager.OnProximityEnded += InputManager_OnProximityEnded;
            if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputDown)
            {
                inputManager.OnInputDown += OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUp)
            {
                inputManager.OnInputUp += OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUpLocked)
            {
                inputManager.OnInputDown += OnInputDownLocked;
                inputManager.OnInputUp += OnInputUpLocked;
            }

        }

        protected virtual void OnDestroy()
        {
            if (!inputManager || !active) return;
            active = false;
            inputManager.OnProximityStarted -= InputManager_OnProximityStarted;
            inputManager.OnProximityUpdated -= InputManager_OnProximityUpdated;
            inputManager.OnProximityEnded -= InputManager_OnProximityEnded;
            if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputDown)
            {
                inputManager.OnInputDown -= OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUp)
            {
                inputManager.OnInputUp -= OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUpLocked)
            {
                inputManager.OnInputDown -= OnInputDownLocked;
                inputManager.OnInputUp -= OnInputUpLocked;
            }
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            currentlyAnimating = false;
            compressable.localScale = Vector3.one;
            blockButton = false;

            if (!inputManager || !active) return;
            active = false;
            inputManager.OnProximityStarted -= InputManager_OnProximityStarted;
            inputManager.OnProximityUpdated -= InputManager_OnProximityUpdated;
            inputManager.OnProximityEnded -= InputManager_OnProximityEnded;
            if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputDown)
            {
                inputManager.OnInputDown -= OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUp)
            {
                inputManager.OnInputUp -= OnInput;
            }
            else if (WhenToCallButtonClickEvent == ButtonInteractionBeamMode.OnInputUpLocked)
            {
                inputManager.OnInputDown -= OnInputDownLocked;
                inputManager.OnInputUp -= OnInputUpLocked;
            }
        }
#pragma warning restore CS0649
    }
}