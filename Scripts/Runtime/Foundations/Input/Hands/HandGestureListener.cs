using System;
using System.Threading;
using System.Threading.Tasks;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
    /// <summary>
    /// This class is set up by HandGestureListenerInternal if hand tracking is enabled in the InputSettings. This class calls the internal event functions for dealing with hand tracking input.
    /// It also sets up access to the hand tracking information from Unity. 
    /// </summary>
    internal class HandGestureListener : MonoBehaviour
    {
        private const float MIN_RELEASE_DISTANCE = 0.05f;
        private const float MIN_PINCH_DISTANCE = 0.03f;
        private float angleOffset;
        private float distance;

        private bool handGestureListenerInitialized;
        private HandGestureListenerInternal handGestureListenerInternal;

        private HandJointController handJointController;
        internal bool handSetupCompleted;
        internal JointTransform indexTransform;
        private InputManager inputManager;

        // Private variables
        private InputSettings inputSettings;
        private InputSource inputSourceInteractionBeam;
        internal InputSource LeftHandInputSource;
        internal JointTransform palmTransform;
        private Transform palmTransformCheck;
        internal bool pinchingLeft;

        internal bool pinchingRight;
        internal InputSource RightHandInputSource;

        private Vector3 rotOffset = new Vector3(-90f, 0f, 0f);
        private Task<bool> setupInternalListenersTask;

        internal JointTransform thumbTransform;

        /// <summary>
        /// The Update function deal with gesture recognizion of hand tracking
        /// </summary>
        private void Update()
        {
            if (handSetupCompleted)
            {
                try
                {
                    if (!palmTransformCheck)
                    {
                        palmTransformCheck = new GameObject("PalmTransformCheck").transform;
                    }

                    // LEFT HAND
                    if (LeftHandInputSource.active && TryGetHandJointTransform(InputSourceKind.HandLeft, JointName.IndexTip, out indexTransform) && TryGetHandJointTransform(InputSourceKind.HandLeft, JointName.ThumbTip, out thumbTransform))
                    {
                        // Update hand position rotation and toggle InputUp, InputDown, and InputUpdated events
                        LeftHandInputSource.worldPosition = indexTransform.position;
                        LeftHandInputSource.worldRotation = indexTransform.rotation;
                        distance = Vector3.Distance(TypeHelpers.MakeUnityVector3(indexTransform.position), TypeHelpers.MakeUnityVector3(thumbTransform.position));
                        LeftHandInputSource.pinchDistance = distance;
                        if (!pinchingLeft && distance < MIN_PINCH_DISTANCE)
                        {
                            try
                            {
                                pinchingLeft = true;
                                handGestureListenerInternal.InputDown(LeftHandInputSource);
                            }
                            catch (Exception e)
                            {
                                BdkLogger.LogException("Error in HandGestureListener with starting left hand manipulation.", e);
                            }
                        }
                        else if (pinchingLeft && distance > MIN_RELEASE_DISTANCE)
                        {                           
                            pinchingLeft = false;
                            handGestureListenerInternal.InputUp(LeftHandInputSource);
                        }

                        handGestureListenerInternal.InputUpdated(LeftHandInputSource);

                        // Update hand rotation state
                        if (TryGetHandJointTransform(InputSourceKind.HandLeft, JointName.Palm, out palmTransform))
                        {
                            palmTransformCheck.position = TypeHelpers.MakeUnityVector3(palmTransform.position);
                            palmTransformCheck.LookAt(inputManager.Hololens);
                            palmTransformCheck.localEulerAngles += rotOffset;
                            angleOffset = Quaternion.Angle(palmTransformCheck.rotation, TypeHelpers.MakeUnityQuaternion(palmTransform.rotation));
                            OnHandRotationToggle?.Invoke(LeftHandInputSource, angleOffset);
                        }
                    }

                    // RIGHT HAND
                    if (RightHandInputSource.active && TryGetHandJointTransform(InputSourceKind.HandRight, JointName.IndexTip, out indexTransform) && TryGetHandJointTransform(InputSourceKind.HandRight, JointName.ThumbTip, out thumbTransform))
                    {
                        // Update hand position rotation and toggle InputUp, InputDown, and InputUpdated events
                        RightHandInputSource.worldPosition = indexTransform.position;
                        RightHandInputSource.worldRotation = indexTransform.rotation;
                        distance = Vector3.Distance(TypeHelpers.MakeUnityVector3(indexTransform.position), TypeHelpers.MakeUnityVector3(thumbTransform.position));
                        RightHandInputSource.pinchDistance = distance;
                        if (!pinchingRight && distance < MIN_PINCH_DISTANCE)
                        {
                            try
                            {
                                pinchingRight = true;
                                handGestureListenerInternal.InputDown(RightHandInputSource);
                            }
                            catch (Exception e)
                            {
                                BdkLogger.LogException("Error in HandGestureListener with starting right hand manipulation.", e);
                            }
                        }
                        else if (pinchingRight && distance > MIN_RELEASE_DISTANCE)
                        {
                            pinchingRight = false;
                            handGestureListenerInternal.InputUp(RightHandInputSource);
                        }

                        handGestureListenerInternal.InputUpdated(RightHandInputSource);

                        // Update hand rotation state
                        if (TryGetHandJointTransform(InputSourceKind.HandRight, JointName.Palm, out palmTransform))
                        {
                            palmTransformCheck.position = TypeHelpers.MakeUnityVector3(palmTransform.position);
                            palmTransformCheck.LookAt(inputManager.Hololens);
                            palmTransformCheck.localEulerAngles += rotOffset;
                            angleOffset = Quaternion.Angle(palmTransformCheck.rotation, TypeHelpers.MakeUnityQuaternion(palmTransform.rotation));
                            OnHandRotationToggle?.Invoke(RightHandInputSource, angleOffset);
                        }
                    }
                }
                catch (Exception e)
                {
                    BdkLogger.LogException("Error in HandGestureListener with updating hands.", e);
                }
            }
        }

        // Events
        public event Action<InputSource> OnSourceFound;
        public event Action<InputSource> OnSourceLost;
        public event Action<InputSource> OnProximityStarted;
        public event Action<InputSource> OnProximityUpdated;
        public event Action<InputSource> OnProximityEnded;
        public event Action<InputSource, float> OnHandRotationToggle;


        internal void SourceFound(InputSource source)
        {
            OnSourceFound?.Invoke(source);
        }

        internal void SourceLost(InputSource source)
        {
            OnSourceLost?.Invoke(source);
            handGestureListenerInternal.ManipulationEnded(source);
        }

        internal async Task InitializeAsync(InputSettings newInputSettings, HandGestureListenerInternal newHandGestureListenerInternal, CancellationToken token)
        {
            inputSettings = newInputSettings;
            inputManager = inputSettings.inputManager;
            LeftHandInputSource = new InputSource();
            LeftHandInputSource.inputSourceKind = InputSourceKind.HandLeft;
            inputSettings.inputManager.AddInputSource(LeftHandInputSource);

            RightHandInputSource = new InputSource();
            RightHandInputSource.inputSourceKind = InputSourceKind.HandRight;
            inputSettings.inputManager.AddInputSource(RightHandInputSource);

            handGestureListenerInternal = newHandGestureListenerInternal;
            await SetupHandGestureListenersAsync(token);
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of HandGestureListener
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupHandGestureListenersAsync(CancellationToken token)
        {
            if (handGestureListenerInitialized)
            {
                return true;
            }

            if (setupInternalListenersTask != null)
            {
                return await setupInternalListenersTask;
            }

            // Create setup task
            setupInternalListenersTask = SetupHandGestureListener(token);
            bool result = await setupInternalListenersTask;
            setupInternalListenersTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up HandGestureListener
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupHandGestureListener(CancellationToken token)
        {
            try
            {
                if (inputSettings.UseHands)
                {
                    handJointController = inputSettings.inputManager.gameObject.AddComponent<HandJointController>();
                    await handJointController.InitializeHands(inputSettings, this, token);
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up HandGestureListener" + e.StackTrace, e);
                return false;
            }

            AddEventListeners();

            handGestureListenerInitialized = true;

            BdkLogger.Log("HandGestureListener setup succesfully", LogSeverity.Info);

            return true;
        }

        /// <summary>
        /// Subscribes to event handlers.
        /// </summary>
        private void AddEventListeners()
        {
            handJointController.OnProximityStarted += inputSouce => OnProximityStarted?.Invoke(inputSouce);
            handJointController.OnProximityUpdated += inputSouce => OnProximityUpdated?.Invoke(inputSouce);
            handJointController.OnProximityEnded += inputSouce => OnProximityEnded?.Invoke(inputSouce);
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
            if (handGestureListenerInitialized)
            {
                return handJointController.TryGetHandJointTransform(inputSourceKind, jointName, out jointTransform, handMustBeActive);
            }

            jointTransform = new JointTransform();
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
            if (handGestureListenerInitialized)
            {
                return handJointController.TryGetDistanceBetweenJoints(jointName, out jointDistance);
            }

            jointDistance = 0f;
            return false;
        }

        /// <summary>
        /// Gets an input source from the system.
        /// </summary>
        /// <param name="inputSourceKind"></param>
        /// <param name="inputSource"></param>
        /// <returns>True if found, false otherwise</returns>
        internal bool TryGetHandInputSource(InputSourceKind inputSourceKind, out InputSource inputSource)
        {
            if (handGestureListenerInitialized)
            {
                if (inputSourceKind == InputSourceKind.HandRight)
                {
                    inputSource = RightHandInputSource;
                }
                else
                {
                    inputSource = LeftHandInputSource;
                }

                return true;
            }

            inputSource = new InputSource();
            return false;
        }
    }
#pragma warning restore CS0649
}