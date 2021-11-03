using Bouvet.DevelopmentKit.Input.Gaze;
using Bouvet.DevelopmentKit.Input.Hands;
using Bouvet.DevelopmentKit.Input.Voice;
using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bouvet.DevelopmentKit.Input
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by InputManager and sets up all the different tracking functionality toggled on in InputSettings.
    /// </summary>
    internal class InputManagerInternal
    {
        // Events
        public event Action<InputSource> OnGazeEnter;
        public event Action<InputSource> OnGazeUpdate;
        public event Action<InputSource> OnGazeExit;
        public event Action<InputSource> OnSourceFound;
        public event Action<InputSource> OnSourceLost;
        public event Action<InputSource> OnInputDown;
        public event Action<InputSource> OnInputUpdated;
        public event Action<InputSource> OnInputUp;
        public event Action<InputSource> OnManipulationStarted;
        public event Action<InputSource> OnManipulationUpdated;
        public event Action<InputSource> OnManipulationEnded;
        public event Action<InputSource> OnProximityStarted;
        public event Action<InputSource> OnProximityUpdated;
        public event Action<InputSource> OnProximityEnded;
        public event Action<InputSource> OnPhraseRecognized;
        public event Action<InputSource, float> OnHandRotationToggle;

        // Private variables
        private HandGestureListenerInternal handGestureListenerInternal;
        private EyeGazeListenerInternal eyeGazeListenerInternal;
        private KeywordListenerInternal keywordListenerInternal;

        private InputSettings inputSettings;
        private bool internalListenersSetupComplete;
        private Task<bool> setupInternalListenersTask;

        // Constructor
        public InputManagerInternal(InputSettings newInputSettings)
        {
            inputSettings = newInputSettings;
        }

        /// <summary>
        /// AddEventHandlers sets up the different events for each of the different input methods. It only sets up those events enabeled in the settings.
        /// </summary>
        private void AddEventHandlers()
        {
            if (inputSettings.UseHands)
            {
                handGestureListenerInternal.OnSourceFound += inputSource => OnSourceFound?.Invoke(inputSource);
                handGestureListenerInternal.OnSourceLost += inputSource => OnSourceLost?.Invoke(inputSource);
                handGestureListenerInternal.OnInputDown += inputSource => OnInputDown?.Invoke(inputSource);
                handGestureListenerInternal.OnInputUpdated += inputSource => OnInputUpdated?.Invoke(inputSource);
                handGestureListenerInternal.OnInputUp += inputSouce => OnInputUp?.Invoke(inputSouce);
                handGestureListenerInternal.OnProximityStarted += inputSouce => OnProximityStarted?.Invoke(inputSouce);
                handGestureListenerInternal.OnProximityUpdated += inputSouce => OnProximityUpdated?.Invoke(inputSouce);
                handGestureListenerInternal.OnProximityEnded += inputSouce => OnProximityEnded?.Invoke(inputSouce);
                handGestureListenerInternal.OnHandRotationToggle += (inputSouce, rotation) => OnHandRotationToggle?.Invoke(inputSouce, rotation);

                if (inputSettings.UseManipulation)
                {
                    handGestureListenerInternal.OnManipulationStarted += inputSouce => OnManipulationStarted?.Invoke(inputSouce);
                    handGestureListenerInternal.OnManipulationUpdated += inputSouce => OnManipulationUpdated?.Invoke(inputSouce);
                    handGestureListenerInternal.OnManipulationEnded += inputSouce => OnManipulationEnded?.Invoke(inputSouce);
                }
            }

            if (inputSettings.UseEyeGaze)
            {
                eyeGazeListenerInternal.OnSourceFound += inputSource => OnSourceFound?.Invoke(inputSource);
                eyeGazeListenerInternal.OnSourceLost += inputSource => OnSourceLost?.Invoke(inputSource);
                eyeGazeListenerInternal.OnGazeEnter += inputSource => OnGazeEnter?.Invoke(inputSource);
                eyeGazeListenerInternal.OnGazeUpdate += inputSource => OnGazeUpdate?.Invoke(inputSource);
                eyeGazeListenerInternal.OnGazeExit += inputSource => OnGazeExit?.Invoke(inputSource);
            }

            if (inputSettings.UseVoice)
            {
                keywordListenerInternal.OnPhraseRecognized += inputSource => OnPhraseRecognized?.Invoke(inputSource);
            }
        }

        /// <summary>
        /// Initializes the task for the setup of the differnet listeners in BouvetDevelopmentKit
        /// </summary>
        /// <param name="token"></param>
        internal async Task InitializeAsync(CancellationToken token)
        {
            await SetupInternalListenersAsync();
        }

        /// <summary>
        /// Starts the task of setting up the different listeners.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupInternalListenersAsync(CancellationToken token = default)
        {
            if (internalListenersSetupComplete)
            {
                return true;
            }

            if (setupInternalListenersTask != null)
            {
                return await setupInternalListenersTask;
            }

            // Create setup task
            setupInternalListenersTask = SetupInputManagerInternal(token);
            bool result = await setupInternalListenersTask;
            setupInternalListenersTask = null;
            return result;
        }

        /// <summary>
        /// Task for setting up and initializing the different listeners asynconously.
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupInputManagerInternal(CancellationToken token)
        {
            try
            {
                if (inputSettings.UseHands)
                {
                    handGestureListenerInternal = new HandGestureListenerInternal(inputSettings);
                    await handGestureListenerInternal.InitializeAsync(token);
                }

                if (inputSettings.UseEyeGaze)
                {
                    eyeGazeListenerInternal = new EyeGazeListenerInternal(inputSettings);
                    await eyeGazeListenerInternal.InitializeAsync(token);
                }

                if (inputSettings.UseVoice)
                {
                    keywordListenerInternal = new KeywordListenerInternal(inputSettings);
                    await keywordListenerInternal.InitializeAsync(token);
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up InputManagerInternal", e);
                return false;
            }

            AddEventHandlers();
            internalListenersSetupComplete = true;

            BdkLogger.Log("InputManagerInternal setup succesfully", LogSeverity.Info);

            return true;
        }


        /// <summary>
        /// This function takes an input phrase and system action and generates a new voice recognizion command out of it. This function is called by InputManager.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="action"></param>
        /// <returns>True if successful, false otherwise</returns>
        internal async Task<bool> AddPhraseForVoiceRecognizion(string phrase, Action systemAction)
        {
            if (!internalListenersSetupComplete)
            {
                await setupInternalListenersTask;
            }

            return await keywordListenerInternal.AddPhraseForVoiceRecognizion(phrase, systemAction);
        }


        /// <summary>
        /// Tries to get the transform of a specfied joint from HandJointController. 
        /// </summary>
        /// <param name="inputSourceKind">Which hand to check</param>
        /// <param name="jointName">Which joint to check on that hand</param>
        /// <param name="jointTransform">The transfrom of that joint (out paramater)</param>
        /// <param name="handMustBeActive">Whether the hand must be active (visible to tracking cameras) or not</param>
        /// <returns>Returns true if a joint is found, false otherwise.</returns>
        internal bool TryGetHandJointTransform(InputSourceKind inputSourceKind, JointName jointName, out JointTransform jointTransform, bool handMustBeActive = false)
        {
            if (internalListenersSetupComplete)
            {
                return handGestureListenerInternal.TryGetHandJointTransform(inputSourceKind, jointName, out jointTransform, handMustBeActive);
            }

            jointTransform = new JointTransform();
            return false;
        }

        internal HandGestureListener GetHandGestureListener()
        {
            if (internalListenersSetupComplete)
            {
                return handGestureListenerInternal.GetHandGestureListener();
            }

            return null;
        }

        internal HandGestureListenerInternal GetHandGestureListenerInternal()
        {
            if (internalListenersSetupComplete)
            {
                return handGestureListenerInternal;
            }

            return null;
        }

        /// <summary>
        /// Gets the distance between a joint on the left and a joint on the right hand (the same joint type)
        /// </summary>
        /// <param name="jointName">The Joint name of the joint.</param>
        /// <param name="jointDistance">The out distance of between the joints.</param>
        /// <returns>True if found, false otherwise.</returns>
        internal bool TryGetDistanceBetweenJoints(JointName jointName, out float jointDistance)
        {
            if (internalListenersSetupComplete)
            {
                return handGestureListenerInternal.TryGetDistanceBetweenJoints(jointName, out jointDistance);
            }

            jointDistance = 0f;
            return false;
        }
    }
#pragma warning restore CS0649
}