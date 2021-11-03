using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by InputManagerInternal if hand tracking is enabled in the InputSettings. This class sets up the internal event functions for dealing with hand tracking input.
    /// It also sets up the class that gets the hand tracking information from Unity. 
    /// </summary>
    internal class HandGestureListenerInternal
    {
        // Events 
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
        public event Action<InputSource, float> OnHandRotationToggle;

        // Private variables
        private InputSettings inputSettings;
        private HandGestureListener handGestureListener;

        internal HandGestureListenerInternal(InputSettings newInputSettings)
        {
            inputSettings = newInputSettings;
        }

        /// <summary>
        /// Methods called by the EyeGazeListener script to call the appropriate actions/events
        /// </summary>

#region Action invoke functions

        internal void InputDown(InputSource source)
        {
            OnInputDown?.Invoke(source);
        }

        internal void InputUp(InputSource source)
        {
            OnInputUp?.Invoke(source);
        }

        internal void InputUpdated(InputSource source)
        {
            OnInputUpdated?.Invoke(source);
        }

        internal void ManipulationStarted(InputSource source)
        {
            OnManipulationStarted?.Invoke(source);
        }

        internal void ManipulationUpdated(InputSource source)
        {
            OnManipulationUpdated?.Invoke(source);
        }

        internal void ManipulationEnded(InputSource source)
        {
            OnManipulationEnded?.Invoke(source);
        }

        internal void ProximityStarted(InputSource source)
        {
            OnProximityStarted?.Invoke(source);
        }

        internal void ProximityUpdated(InputSource source)
        {
            OnProximityUpdated?.Invoke(source);
        }

        internal void ProximityEnded(InputSource source)
        {
            OnProximityEnded?.Invoke(source);
        }

#endregion

        private bool handGestureListenerInternalInitialized;
        private Task<bool> setupInternalListenersTask;

        /// <summary>
        /// Initializes the task for the setup of the different listeners in BouvetDevelopmentKit
        /// </summary>
        /// <param name="token"></param>
        internal async Task InitializeAsync(CancellationToken token)
        {
            await SetupInternalHandGestureListenerAsync(token);
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of HandGestureListenerInternal
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupInternalHandGestureListenerAsync(CancellationToken token)
        {
            if (handGestureListenerInternalInitialized)
            {
                return true;
            }

            if (setupInternalListenersTask != null)
            {
                return await setupInternalListenersTask;
            }

            // Create setup task
            setupInternalListenersTask = SetupHandGestureListenerInternal(token);
            bool result = await setupInternalListenersTask;
            setupInternalListenersTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up HandGestureListenerInternal
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupHandGestureListenerInternal(CancellationToken token)
        {
            try
            {
                handGestureListener = inputSettings.inputManager.gameObject.AddComponent<HandGestureListener>();
                await handGestureListener.InitializeAsync(inputSettings, this, token);
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up HandGestureListenerInternal", e);
                return false;
            }

            AddEventHandlers();
            handGestureListenerInternalInitialized = true;

            BdkLogger.Log("HandGestureListenerInternal setup succesfully", LogSeverity.Info);

            return true;
        }

        /// <summary>
        /// Subscribes to event handlers.
        /// </summary>
        private void AddEventHandlers()
        {
            handGestureListener.OnSourceFound += inputSource => OnSourceFound?.Invoke(inputSource);
            handGestureListener.OnSourceLost += inputSource => OnSourceLost?.Invoke(inputSource);
            handGestureListener.OnProximityStarted += inputSouce => OnProximityStarted?.Invoke(inputSouce);
            handGestureListener.OnProximityUpdated += inputSouce => OnProximityUpdated?.Invoke(inputSouce);
            handGestureListener.OnProximityEnded += inputSouce => OnProximityEnded?.Invoke(inputSouce);
            handGestureListener.OnHandRotationToggle += (inputSouce, rotation) => OnHandRotationToggle?.Invoke(inputSouce, rotation);
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
            if (handGestureListenerInternalInitialized)
            {
                return handGestureListener.TryGetHandJointTransform(inputSourceKind, jointName, out jointTransform, handMustBeActive);
            }

            jointTransform = new JointTransform();
            return false;
        }

        internal HandGestureListener GetHandGestureListener()
        {
            if (handGestureListenerInternalInitialized)
            {
                return handGestureListener;
            }

            return null;
        }


        /// <summary>
        /// Gets an input source from the system.
        /// </summary>
        /// <param name="inputSourceKind"></param>
        /// <param name="inputSource"></param>
        /// <returns>True if found, false otherwise</returns>
        internal bool TryGetHandInputSource(InputSourceKind inputSourceKind, out InputSource inputSource)
        {
            if (handGestureListenerInternalInitialized)
            {
                return handGestureListener.TryGetHandInputSource(inputSourceKind, out inputSource);
            }

            inputSource = new InputSource();
            return false;
        }

        /// <summary>
        /// Gets the distance between a joint on the left and a joint on the right hand (the same joint type)
        /// </summary>
        /// <param name="jointName">The Joint name of the joint.</param>
        /// <param name="jointDistance">The out distance of between the joints.</param>
        /// <returns>True if found, false otherwise.</returns>
        internal bool TryGetDistanceBetweenJoints(JointName jointName, out float jointDistance)
        {
            if (handGestureListenerInternalInitialized)
            {
                return handGestureListener.TryGetDistanceBetweenJoints(jointName, out jointDistance);
            }

            jointDistance = 0f;
            return false;
        }
    }
#pragma warning restore CS0649
}