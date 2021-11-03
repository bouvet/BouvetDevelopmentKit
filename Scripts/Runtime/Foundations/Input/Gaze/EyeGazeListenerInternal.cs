using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bouvet.DevelopmentKit.Input.Gaze
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by InputManagerInternal if eye tracking is enabled in the InputSettings. This class sets up the internal event functions for dealing with eye tracking input.
    /// It also sets up the class that gets the eye tracking information from Unity. 
    /// </summary>
    internal class EyeGazeListenerInternal
    {
        // Events
        public event Action<InputSource> OnSourceFound;
        public event Action<InputSource> OnSourceLost;
        public event Action<InputSource> OnGazeEnter;
        public event Action<InputSource> OnGazeUpdate;
        public event Action<InputSource> OnGazeExit;

        // Private variables
        private InputSettings inputSettings;
        private EyeGazeListener eyeGazeListener;

        internal EyeGazeListenerInternal(InputSettings newInputSettings)
        {
            inputSettings = newInputSettings;
            eyeGazeListener = inputSettings.inputManager.gameObject.AddComponent<EyeGazeListener>();
        }

        /// <summary>
        /// Methods called by the EyeGazeListener script to call the appropriate actions/events
        /// </summary>

#region Action invoke functions

        internal void SourceFound(InputSource source)
        {
            OnSourceFound.Invoke(source);
        }

        internal void SourceLost(InputSource source)
        {
            OnSourceLost.Invoke(source);
        }

        internal void GazeEnter(InputSource source)
        {
            OnGazeEnter.Invoke(source);
        }

        internal void GazeUpdate(InputSource source)
        {
            OnGazeUpdate.Invoke(source);
        }

        internal void GazeExit(InputSource source)
        {
            OnGazeExit.Invoke(source);
        }

#endregion

        private bool internalListenersSetupComplete;
        private Task<bool> eyeGazeListenerInternalInitialized;

        /// <summary>
        /// Initializes the task for the setup of the different listeners in BouvetDevelopmentKit
        /// </summary>
        /// <param name="token"></param>
        internal async Task InitializeAsync(CancellationToken token)
        {
            await SetupEyeGazeListenerInternalAsync(token);
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of EyeGazeListenerInternal
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupEyeGazeListenerInternalAsync(CancellationToken token)
        {
            if (internalListenersSetupComplete)
            {
                return true;
            }

            if (eyeGazeListenerInternalInitialized != null)
            {
                return await eyeGazeListenerInternalInitialized;
            }

            // Create setup task
            eyeGazeListenerInternalInitialized = SetupEyeGazeListenerInternal(token);
            bool result = await eyeGazeListenerInternalInitialized;
            eyeGazeListenerInternalInitialized = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up InputManagerInternal
        /// </summary>
        private async Task<bool> SetupEyeGazeListenerInternal(CancellationToken token)
        {
            try
            {
                await eyeGazeListener.InitializeAsync(inputSettings, this, token);
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up EyeGazeListenerInternal", e);
                return false;
            }

            internalListenersSetupComplete = true;

            BdkLogger.Log("EyeGazeListenerInternal setup succesfully", LogSeverity.Info);

            return true;
        }
    }
#pragma warning restore CS0649
}