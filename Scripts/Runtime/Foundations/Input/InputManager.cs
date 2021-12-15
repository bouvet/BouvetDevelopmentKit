using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bouvet.DevelopmentKit.Input.Gaze;
using Bouvet.DevelopmentKit.Input.Hands;
using Bouvet.DevelopmentKit.Input.Voice;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
using Windows.Foundation.Metadata;
using Windows.Graphics.Holographic;
#endif

namespace Bouvet.DevelopmentKit.Input
{
#pragma warning disable CS0649
    [RequireComponent(typeof(InputSettings))]
    [RequireComponent(typeof(SpatialCoordinateSystemAccess))]
    public class InputManager : MonoBehaviour //InputManagerSingleton<InputManager>
    {
        // Public variables 
        public static InputManager Instance;
        public Transform Hololens;
        public InputSettings inputSettings;

        [HideInInspector] public Transform rightGripPoint;
        [HideInInspector] public Transform leftGripPoint;

        private readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

        // Private variables
        private EyeGazeListener eyeGazeListener;
        private HandGestureListener handGestureListener;
        private KeywordListener keywordListener;
        public List<InputSource> InputSources = new List<InputSource>();

        private Dictionary<int, GameObject> objectList = new Dictionary<int, GameObject>();

        private async void Awake()
        {
            if (!inputManagerSetupComplete)
            {
                inputSettings = GetComponent<InputSettings>();
                DefineHologramAlignment();
                Instance = this;

                await SetupInputManagerAsync();

#if UNITY_EDITOR
                await Task.Delay(1000);
                //ShowDebugHeadCursor();
#endif
            }
        }

        /// <summary>
        /// Fires events on mainthread
        /// </summary>
        private void Update()
        {
            if (mainThreadQueue.IsEmpty)
            {
                return;
            }

            // Process workloads queued from background threads
            while (mainThreadQueue.TryDequeue(out Action action))
            {
                action();
            }
        }

        // Events
        public event Action<InputSource> OnGazeEnter;
        public event Action<InputSource> OnGazeUpdate;
        public event Action<InputSource> OnGazeExit;
        public event Action<InputSource> OnSourceFound;
        public event Action<InputSource> OnSourceLost;
        public event Action<InputSource> OnInputUp;
        public event Action<InputSource> OnInputUpdated;
        public event Action<InputSource> OnInputDown;
        public event Action<InputSource> OnManipulationStarted;
        public event Action<InputSource> OnManipulationUpdated;
        public event Action<InputSource> OnManipulationEnded;
        public event Action<InputSource> OnProximityStarted;
        public event Action<InputSource> OnProximityUpdated;
        public event Action<InputSource> OnProximityEnded;
        public event Action<InputSource> OnPhraseRecognized;
        public event Action<InputSource> OnHololensTransformUpdated;
        public event Action<InputSource, float> OnHandRotationToggle;

        /// <summary>
        /// Sets up the Unity dependent objects 
        /// </summary>
        private void DefineHologramAlignment()
        {
            if (inputSettings.AlignWithPVCamera)
            {
                AlignHologramsOnPvCamera();
            }
        }

        /// <summary>
        /// Sets a holographic view configuration setting that may improve hologram stability on PV camera sync
        /// </summary>
        public static void AlignHologramsOnPvCamera()
        {
#if WINDOWS_UWP && !UNITY_EDITOR
            if (ApiInformation.IsMethodPresent("Windows.Graphics.Holographic.HolographicDisplay", "TryGetViewConfiguration"))
            {
                // If the default display has configuration for a PhotoVideoCamera, we want to enable it
                HolographicViewConfiguration viewConfiguration = HolographicDisplay.GetDefault()?.TryGetViewConfiguration(HolographicViewConfigurationKind.PhotoVideoCamera);
                if (viewConfiguration != null && !viewConfiguration.IsEnabled)
                {
                    viewConfiguration.IsEnabled = true;
                }
            }
#endif // WINDOWS_UWP && !UNITY_EDITOR
        }

        /// <summary>
        /// Function that generates neccessary prefabs while developing
        /// </summary>
        public void GeneratePrefabs()
        {
#if UNITY_EDITOR
            Hololens = Camera.main.transform;
            GetComponent<FunctionalityCreator>()?.OnValidate();
            if (!inputSettings)
            {
                inputSettings = GetComponent<InputSettings>();
            }

            if (!rightGripPoint)
            {
                rightGripPoint = new GameObject().transform;
                rightGripPoint.parent = transform;
                rightGripPoint.name = "RightGripPoint";
            }

            if (!leftGripPoint)
            {
                leftGripPoint = new GameObject().transform;
                leftGripPoint.parent = transform;
                leftGripPoint.name = "LeftGripPoint";
            }
#endif
        }

        /// <summary>
        /// Public functions that can be accessed by custom scripts.
        /// </summary>

        #region
        /// <summary>
        /// Function that returns the game object currently being focused by the main input source.
        /// </summary>
        /// <param name="returnObject">The GameObject being focused on.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool TryGetFocusedObject(out GameObject returnObject)
        {
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].collidedObject != null)
                {
                    returnObject = InputSources[i].collidedObject;
                    return true;
                }
            }

            returnObject = null;
            return false;
        }

        /// <summary>
        /// Function that returns the Interactable currently being focused by the main input source.
        /// </summary>
        /// <param name="returnObject">The GameObject of the Interactable being focused on.</param>
        /// <returns>True if found, false otherwise.</returns>
        public bool TryGetFocusedInteractable(out GameObject returnObject)
        {
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].collidedObject != null)
                {
                    returnObject = InputSources[i].collidedObject;
                    if (returnObject.activeSelf && returnObject.GetComponent<Interactable>())
                    {
                        return true;
                    }
                }
            }

            returnObject = null;
            return false;
        }

        /// <summary>
        /// Checks if any of the InputSources are currently targeting a button.
        /// </summary>
        /// <returns>The id of the button, if found. -1 otherwise</returns>
        public bool TryGetFocusedButton(out GameObject target)
        {
            target = null;
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].collidedObject != null)
                {
                    target = InputSources[i].collidedObject;
                    if (target.activeSelf && target.GetComponent<InteractableButton>())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryGetCursorPosition(out Vector3 vector3)
        {
            if (TryGetActiveCursorTransform(out Transform cursor))
            {
                vector3 = cursor.position;
                return true;
            }
            vector3 = Vector3.zero;
            return false;
        }

        public bool TryGetCursorRotation(out Quaternion quaternion)
        {
            if (TryGetActiveCursorTransform(out Transform cursor))
            {
                quaternion = cursor.rotation;
                return true;
            }
            quaternion = Quaternion.identity;
            return false;
        }

        public bool TryGetActiveCursorTransform(out Transform activeCursor)
        {
            activeCursor = null;
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].collidedObject != null)
                {
                    switch (InputSources[i].inputSourceKind)
                    {
                        case InputSourceKind.HandRight:
                            activeCursor = GetComponent<FunctionalityCreator>()?.RIndex.transform;
                            break;
                        case InputSourceKind.HandLeft:
                            activeCursor = GetComponent<FunctionalityCreator>()?.LIndex.transform;
                            break;
                        case InputSourceKind.InteractionBeamRight:
                            activeCursor = GetComponent<FunctionalityCreator>()?.RHandRay.GetComponent<InteractionBeam>()?.rayTarget.transform;
                            break;
                        case InputSourceKind.InteractionBeamLeft:
                            activeCursor = GetComponent<FunctionalityCreator>()?.LHandRay.GetComponent<InteractionBeam>()?.rayTarget.transform;
                            break;
                        case InputSourceKind.HeadGaze:
                            activeCursor = GetComponent<FunctionalityCreator>()?.HeadGazeCursor.transform;
                            break;
                        case InputSourceKind.Hololens:
                            activeCursor = GetComponent<FunctionalityCreator>()?.HeadGazeCursor.transform;
                            break;
                        case InputSourceKind.EyeGaze:
                            activeCursor = GetComponent<FunctionalityCreator>()?.EyeGazeCursor.transform;
                            break;
                    }
                    if (activeCursor != null)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the Transform of the requested cursor if all the parameters are met. Returns false if none is found.
        /// </summary>
        /// <param name="inputSourceKind"></param>
        /// <param name="cursorTransform"></param>
        /// <param name="mustBeActive"></param>
        /// <returns></returns>
        public bool TryGetCursorTransform(InputSourceKind inputSourceKind, out Transform cursorTransform, bool mustBeActive)
        {
            cursorTransform = null;
            if (mustBeActive)
            {
                foreach (InputSource inputSource in InputSources)
                {
                    if (inputSource.inputSourceKind == inputSourceKind && !inputSource.active)
                    {
                        return false;
                    }
                }
            }

            switch (inputSourceKind)
            {
                case InputSourceKind.HandRight:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.RIndex.transform;
                    break;
                case InputSourceKind.HandLeft:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.LIndex.transform;
                    break;
                case InputSourceKind.InteractionBeamRight:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.RHandRay.GetComponent<InteractionBeam>()?.cursor.transform;
                    break;
                case InputSourceKind.InteractionBeamLeft:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.LHandRay.GetComponent<InteractionBeam>()?.cursor.transform;
                    break;
                case InputSourceKind.HeadGaze:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.HeadGazeCursor.transform;
                    break;
                case InputSourceKind.Hololens:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.HeadGazeCursor.transform;
                    break;
                case InputSourceKind.EyeGaze:
                    cursorTransform = GetComponent<FunctionalityCreator>()?.EyeGazeCursor.transform;
                    break;
            }
            if (cursorTransform != null)
                return true;

            return false;
        }

        /// <summary>
        /// Tries to get the main, active InputSource.
        /// </summary>
        /// <param name="inputSource">The out parameter of the found InputSource</param>
        /// <returns>True if found, false if not found.</returns>
        public bool TryGetMainActiveInputSource(out InputSource inputSource)
        {
            inputSource = null;
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].active)
                {
                    inputSource = InputSources[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to get an input source.
        /// </summary>
        /// <param name="inputSourceKind">The InputSourceKind the user wants to access.</param>
        /// <param name="inputSource">The out parameter that passes out the InputSource.</param>
        /// <param name="canBeInactive">True: the InputSource can be inactive. False: the InputSource must be active/visible/tracking.</param>
        /// <returns>True if InputSource is found and passes that InputSource as an out parameter. False if the InputSource is not found.</returns>
        public bool TryGetInputSource(InputSourceKind inputSourceKind, out InputSource inputSource, bool canBeInactive = true)
        {
            inputSource = default;
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (inputSourceKind == InputSources[i].inputSourceKind && (canBeInactive || InputSources[i].active))
                {
                    inputSource = InputSources[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function takes an input phrase and system action and generates a new voice recognizion command out of it. This function can be called by custom scripts to add new functions and update the dictionary.
        /// PS: needs not pass in an action if the user prefers to listen to the OnPhraseRecognized instead.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="action"></param>
        public async void AddPhraseForVoiceRecognizion(string phrase, Action action = null)
        {
            if (!inputManagerSetupComplete)
            {
                await setupInputManagerTask;
            }

            await keywordListener.AddPhraseForVoiceRecognizion(phrase, action);
        }

        private JointTransform jointTransform;

        /// <summary>
        /// Tries to get the transform of a specfied joint from HandJointController. 
        /// </summary>
        /// <param name="inputSourceKind">Which hand to check</param>
        /// <param name="jointName">Which joint to check on that hand</param>
        /// <param name="jointPosition">The position of that joint (out paramater)</param>
        /// <param name="jointRotation">The rotation of that joint (out paramater)</param>
        /// <param name="handMustBeActive">Whether the hand must be active (visible to tracking cameras) or not</param>
        /// <returns>True if joints found and the joint position and rotation as out paramaters, false if joint is not found.</returns>
        public bool TryGetHandJointTransform(InputSourceKind inputSourceKind, JointName jointName, out Vector3 jointPosition, out Quaternion jointRotation, bool handMustBeActive = false)
        {
            if (inputManagerSetupComplete)
            {
                if (handGestureListener.TryGetHandJointTransform(inputSourceKind, jointName, out jointTransform, handMustBeActive))
                {
                    jointPosition = jointTransform.position;
                    jointRotation = jointTransform.rotation;
                    return true;
                }
            }

            jointPosition = Vector3.zero;
            jointRotation = Quaternion.identity;
            return false;
        }

        /// <summary>
        /// Gets the distance between a joint on the left and a joint on the right hand (the same joint type)
        /// </summary>
        /// <param name="jointName">The Joint name of the joint.</param>
        /// <param name="jointDistance">The distance of between the joints.</param>
        /// <returns>True if found and the joint distance as an out paramater, false if joints are not found.</returns>
        public bool TryGetDistanceBetweenJoints(JointName jointName, out float jointDistance)
        {
            if (inputManagerSetupComplete)
            {
                if (handGestureListener.TryGetDistanceBetweenJoints(jointName, out jointDistance))
                {
                    return true;
                }
            }

            jointDistance = 0f;
            return false;
        }

        #endregion

        /// <summary>
        /// Internal functions that cannot be accessed by custom scripts.
        /// </summary>

        #region

        internal void AddInputSource(InputSource newInputSource)
        {
            for (int i = 0; i < InputSources.Count; i++)
            {
                if (InputSources[i].inputSourceKind.Equals(newInputSource.inputSourceKind))
                {
                    BdkLogger.Log($"Failed to add new input source: {newInputSource.inputSourceKind}. Already in list", LogSeverity.Info);
                    return;
                }
            }

            for (int i = 0; i < inputSettings.InputPriority.Count; i++)
            {
                if (newInputSource.inputSourceKind == inputSettings.InputPriority[i])
                {
                    if (InputSources.Count > i)
                    {
                        InputSources.Insert(i, newInputSource);
                        return;
                    }
                }
            }

            InputSources.Add(newInputSource);
            BdkLogger.Log($"Added input source: {newInputSource.inputSourceKind}", LogSeverity.Info);
        }

        public CursorState GetCursorState(bool getRightHand)
        {
            if (getRightHand)
            {
                return inputSettings.CursorManager.RightHandCursorState;
            }

            return inputSettings.CursorManager.LeftHandCursorState;
        }

        internal void SetCursorState(bool enterProximity, bool isRightHand)
        {
            if (enterProximity)
            {
                inputSettings.CursorManager.EnterProximity(isRightHand);
            }
            else
            {
                inputSettings.CursorManager.ExitProximity(isRightHand);
            }
        }

        internal void InvokeHololensUpdateTransform(InputSource source)
        {
            OnHololensTransformUpdated?.Invoke(source);
        }

        internal HandGestureListener GetHandGestureListener()
        {
            if (inputManagerSetupComplete)
            {
                return handGestureListener;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Initialization/Setup of all things InputManager
        /// </summary>

        #region

        private bool inputManagerSetupComplete;

        private Task<bool> setupInputManagerTask;

        private async Task<bool> SetupInputManagerAsync(CancellationToken token = default)
        {
            if (inputManagerSetupComplete)
            {
                return true;
            }

            if (setupInputManagerTask != null)
            {
                return await setupInputManagerTask;
            }

            // Create setup task
            setupInputManagerTask = SetupInputManager(token);
            bool result = await setupInputManagerTask;
            setupInputManagerTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up InputManagerInternal
        /// </summary>
        private async Task<bool> SetupInputManager(CancellationToken token)
        {
            try
            {
                if (inputSettings.UseHands)
                {
                    handGestureListener = inputSettings.inputManager.gameObject.AddComponent<HandGestureListener>();
                    await handGestureListener.InitializeAsync(inputSettings, token);
                }

                if (inputSettings.UseEyeGaze)
                {
                    eyeGazeListener = inputSettings.inputManager.gameObject.AddComponent<EyeGazeListener>();
                    await eyeGazeListener.InitializeAsync(inputSettings, token);
                }

                if (inputSettings.UseVoice)
                {
                    if (keywordListener == null)
                    {
                        keywordListener = new KeywordListener();
                    }

                    await keywordListener.InitializeAsync(inputSettings, token);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
                BdkLogger.LogException("Error setting up InputManager", e);
                return false;
            }

            AddEventHandlers();
            inputManagerSetupComplete = true;

            BdkLogger.Log("InputManager setup succesfully", LogSeverity.Info);

            return true;
        }

        private void ShowDebugHeadCursor()
        {
#if UNITY_EDITOR
            if (!inputSettings.UseHeadGaze)
            {
                inputSettings.UseHeadGaze = true;
                if (!Hololens.gameObject.GetComponent<HololensTransformUpdate>())
                {
                    Hololens.gameObject.AddComponent<HololensTransformUpdate>().inputManager = this;
                }
            }
            if (!inputSettings.ShowHeadGazeCursor)
            {
                inputSettings.ShowHeadGazeCursor = true;
                var HeadGazeCursor = Instantiate(GetComponent<FunctionalityCreator>()?.headGazePrefab, transform);
                HeadGazeCursor.GetComponent<HeadGazeCursor>().inputManager = this;
            }
            inputSettings.AlwaysShowHeadGazeCursor = true;
#endif
        }

        /// <summary>
        /// Subscribes to InputManagerInternal event handlers.
        /// </summary>
        private void AddEventHandlers()
        {
            if (inputSettings.UseHands)
            {
                handGestureListener.OnSourceFound += inputSource => OnSourceFound?.Invoke(inputSource);
                handGestureListener.OnSourceLost += inputSource => OnSourceLost?.Invoke(inputSource);
                handGestureListener.OnInputDown += inputSource => OnInputDown?.Invoke(inputSource);
                handGestureListener.OnInputUpdated += inputSource => OnInputUpdated?.Invoke(inputSource);
                handGestureListener.OnInputUp += inputSouce => OnInputUp?.Invoke(inputSouce);
                handGestureListener.OnProximityStarted += inputSouce => OnProximityStarted?.Invoke(inputSouce);
                handGestureListener.OnProximityUpdated += inputSouce => OnProximityUpdated?.Invoke(inputSouce);
                handGestureListener.OnProximityEnded += inputSouce => OnProximityEnded?.Invoke(inputSouce);
                handGestureListener.OnHandRotationToggle += (inputSouce, rotation) => OnHandRotationToggle?.Invoke(inputSouce, rotation);

                if (inputSettings.UseManipulation)
                {
                    handGestureListener.OnManipulationStarted += inputSouce => OnManipulationStarted?.Invoke(inputSouce);
                    handGestureListener.OnManipulationUpdated += inputSouce => OnManipulationUpdated?.Invoke(inputSouce);
                    handGestureListener.OnManipulationEnded += inputSouce => OnManipulationEnded?.Invoke(inputSouce);
                }
            }

            if (inputSettings.UseEyeGaze)
            {
                eyeGazeListener.OnSourceFound += inputSource => OnSourceFound?.Invoke(inputSource);
                eyeGazeListener.OnSourceLost += inputSource => OnSourceLost?.Invoke(inputSource);
                eyeGazeListener.OnGazeEnter += inputSource => OnGazeEnter?.Invoke(inputSource);
                eyeGazeListener.OnGazeUpdate += inputSource => OnGazeUpdate?.Invoke(inputSource);
                eyeGazeListener.OnGazeExit += inputSource => OnGazeExit?.Invoke(inputSource);
            }

            if (inputSettings.UseVoice)
            {
                keywordListener.OnPhraseRecognized += inputSource => OnPhraseRecognized?.Invoke(inputSource);
            }
        }

        public async Task WaitForInputManagerSetup()
        {
            if (!inputManagerSetupComplete)
            {
                await setupInputManagerTask;
            }
        }

        #endregion
    }
#pragma warning restore CS0649
}