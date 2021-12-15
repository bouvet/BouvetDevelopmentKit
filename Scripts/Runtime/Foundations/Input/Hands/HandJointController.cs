using System;
using System.Threading;
using System.Threading.Tasks;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
#if ENABLE_XR_SDK && (WINDOWS_UWP || DOTNETWINRT_PRESENT)
using UnityEngine.XR;
using System.Collections.Generic;
#endif

namespace Bouvet.DevelopmentKit.Input.Hands
{
#pragma warning disable CS0649
#pragma warning disable CS0414
#pragma warning disable CS0067
#pragma warning disable CS1998
    /// <summary>
    /// This class is set up by HandGestureListener if hand tracking is enabled in the InputSettings.
    /// </summary>
    public class HandJointController : MonoBehaviour
    {
        internal HandGestureListener handGestureListener;

        private bool handsInitialized;

        private InputSettings inputSettings;
        private bool[] jointsToUpdate = new bool[26];
        private JointTransform[] jointTransformLeft = new JointTransform[26];
        private JointTransform[] jointTransformRight = new JointTransform[26];
        private bool leftHandCurrentlyVisible;
        private bool leftHandObserved;

        // Private variables
        private bool rightHandCurrentlyVisible;
        private bool rightHandObserved;
        private Task<bool> setupHandsTask;

        internal bool spatialInteractionManagerFound;

        /// <summary>
        /// Setup function that gets all neccesary values to setup the hands and then sets them up.
        /// </summary>
        /// <param name="newInputSettings"></param>
        /// <param name="newHandGestureListener"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task InitializeHands(InputSettings newInputSettings, HandGestureListener newHandGestureListener, CancellationToken token)
        {
            inputSettings = newInputSettings;
            handGestureListener = newHandGestureListener;

            await SetupHandsAsync(token);
        }

        /// <summary>
        /// Method for setting up the asynconous initialization of Hands
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupHandsAsync(CancellationToken token)
        {
            if (handsInitialized)
            {
                return true;
            }

            if (setupHandsTask != null)
            {
                return await setupHandsTask;
            }

            // Create setup task
            setupHandsTask = SetupHands(token);
            bool result = await setupHandsTask;
            setupHandsTask = null;
            return result;
        }

        /// <summary>
        /// Internal method for setting up hands
        /// </summary>
        /// <returns>True if successful, false if not</returns>
        private async Task<bool> SetupHands(CancellationToken token)
        {
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
            try
            {
                await Task.Delay(50);
                if (SpatialCoordinateSystemAccess.spatialInteractionManagerFound)
                {
                    spatialInteractionManagerFound = true;
                    handGestureListener.handSetupCompleted = true;
                    BdkLogger.Log("Spatial interaction manager found", LogSeverity.Info);
                }
                else
                {
                    BdkLogger.Log("Couldn't find SpatialCoordinateSystems variable SpatialInteractionManager. Waiting 500ms and trying again!", LogSeverity.Info);
                    await Task.Delay(500);
                    await SetupHands(token);
                }
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up HandVisualization" + e.StackTrace, e);
                return false;
            }

            handsInitialized = true;

            Invoke(nameof(CheckInitialHandState), 0.1f);

            BdkLogger.Log("HandVisualization setup succesfully", LogSeverity.Info);
#endif
            return true;
        }

        private void CheckInitialHandState()
        {
            rightHandCurrentlyVisible = false;
            leftHandCurrentlyVisible = false;
            handGestureListener.SourceLost(handGestureListener.RightHandInputSource);
            handGestureListener.SourceLost(handGestureListener.LeftHandInputSource);
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
            if (!jointsToUpdate[(int) jointName] && spatialInteractionManagerFound)
            {
                if (inputSourceKind == InputSourceKind.HandLeft && (!handMustBeActive || leftHandCurrentlyVisible))
                {
                    jointTransform = jointTransformLeft[(int) jointName];
                    return true;
                }

                if (inputSourceKind == InputSourceKind.HandRight && (!handMustBeActive || rightHandCurrentlyVisible))
                {
                    jointTransform = jointTransformRight[(int) jointName];
                    return true;
                }
            }
            else
            {
                jointsToUpdate[(int) jointName] = true;
            }

            jointTransform = new JointTransform();
            return false;
        }

        internal bool TryGetDistanceBetweenJoints(JointName jointName, out float jointDistance)
        {
            if (!jointsToUpdate[(int) jointName] && spatialInteractionManagerFound && rightHandCurrentlyVisible && leftHandCurrentlyVisible)
            {
                JointTransform jointTransform = jointTransformLeft[(int) jointName];
                JointTransform jointTransform2 = jointTransformRight[(int) jointName];
                jointDistance = Vector3.Distance(jointTransform.position, jointTransform2.position);
                return true;
            }

            jointDistance = 0f;
            return false;
        }

#if ENABLE_XR_SDK && (WINDOWS_UWP || DOTNETWINRT_PRESENT)
        /// <summary>
        /// This method gets a list of hands available and toggles which hand is currently visible to the spatial cameras.
        /// This method calls the event functions based on which hands are currently visible to the spatial cameras.
        /// </summary>
        /// <param name="sources"></param>
        private void CheckHandActivation(List<InputDevice> sources)
        {
            // Updates the hand states
            if (sources.Count == 1)
            {
                rightHandObserved = (sources[0].characteristics & InputDeviceCharacteristics.Right) != 0;
                leftHandObserved = (sources[0].characteristics & InputDeviceCharacteristics.Left) != 0;
            }

            else if (sources.Count == 2)
            {
                rightHandObserved = true;
                leftHandObserved = true;
            }
            else
            {
                rightHandObserved = false;
                leftHandObserved = false;
            }

            // Calls event functions if anything has changed
            if (rightHandObserved && !rightHandCurrentlyVisible)
            {
                rightHandCurrentlyVisible = true;
                handGestureListener.SourceFound(handGestureListener.RightHandInputSource);
            }
            else if (!rightHandObserved && rightHandCurrentlyVisible)
            {
                rightHandCurrentlyVisible = false;
                handGestureListener.SourceLost(handGestureListener.RightHandInputSource);
            }

            if (leftHandObserved && !leftHandCurrentlyVisible)
            {
                leftHandCurrentlyVisible = true;
                handGestureListener.SourceFound(handGestureListener.LeftHandInputSource);
            }
            else if (!leftHandObserved && leftHandCurrentlyVisible)
            {
                leftHandCurrentlyVisible = false;
                handGestureListener.SourceLost(handGestureListener.LeftHandInputSource);
            }

            // Updates each input source state
            handGestureListener.RightHandInputSource.active = rightHandCurrentlyVisible;
            handGestureListener.LeftHandInputSource.active = leftHandCurrentlyVisible;
        }
        private static readonly HandFinger[] handFingers = Enum.GetValues(typeof(HandFinger)) as HandFinger[];
        private readonly List<Bone> fingerBones = new List<Bone>();
#endif

        /// <summary>
        /// Update method that matches the position of each joint in the virtual hand with that of the physical hand.
        /// </summary>
        private void Update()
        {
            if (spatialInteractionManagerFound)
            {
#if ENABLE_XR_SDK && (WINDOWS_UWP || DOTNETWINRT_PRESENT)
                try
                {
                    List<InputDevice> inputDevices = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking, inputDevices);
                    
                    CheckHandActivation(inputDevices);

                    foreach (InputDevice device in inputDevices)
                    {
                        if ((device.characteristics & InputDeviceCharacteristics.HandTracking) != 0)
                        {
                            UpdateHand(device, (device.characteristics & InputDeviceCharacteristics.Right) != 0);
                        }
                    }
                }
                catch (Exception e)
                {
                    BdkLogger.LogException("Error in HandJointController - Update", e);
                }
#endif
            }
        }

#if ENABLE_XR_SDK && (WINDOWS_UWP || DOTNETWINRT_PRESENT)
        private void UpdateHand(InputDevice device, bool isRightHand)
        {
            Hand hand;
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            if (device.TryGetFeatureValue(CommonUsages.handData, out hand))
            {
                // Update hand joint positions and rotations
                foreach (HandFinger finger in handFingers)
                {
                    if (hand.TryGetFingerBones(finger, fingerBones))
                    {
                        // Update joints per finger
                        for (int i = 0; i < fingerBones.Count; i++)
                        {
                            // Convert from local finger joint index to "global" hand joint index
                            int currentJoint = ConvertJointIndex(finger, i);
                            Bone bone = fingerBones[i];

                            bool positionAvailable = bone.TryGetPosition(out position);
                            bool rotationAvailable = bone.TryGetRotation(out rotation);

                            if (positionAvailable || rotationAvailable)
                            {
                                if (isRightHand)
                                {
                                    jointTransformRight[currentJoint].position = transform.TransformPoint(position);
                                    jointTransformRight[currentJoint].rotation = rotation;
                                }
                                else
                                {
                                    jointTransformLeft[currentJoint].position = transform.TransformPoint(position);
                                    jointTransformLeft[currentJoint].rotation = rotation;
                                }
                            }
                        }
                    }
                }
                // Update Palm position and rotation
                if (device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
                {
                    if (isRightHand)
                    {
                        jointTransformRight[(int)JointName.Palm].position = transform.TransformPoint(position);
                        jointTransformRight[(int)JointName.Palm].rotation = jointTransformRight[(int)JointName.MiddleMetacarpal].rotation;
                    }
                    else
                    {
                        jointTransformLeft[(int)JointName.Palm].position = transform.TransformPoint(position);
                        jointTransformLeft[(int)JointName.Palm].rotation = jointTransformLeft[(int)JointName.MiddleMetacarpal].rotation;
                    }
                }
            }
        }

        private int ConvertJointIndex(HandFinger finger, int index)
        {
            switch (finger)
            {
                case HandFinger.Thumb: return (index == 0) ? (int)JointName.Wrist : (int)JointName.ThumbMetacarpal + index - 1;
                case HandFinger.Index: return (int)JointName.IndexMetacarpal + index;
                case HandFinger.Middle: return (int)JointName.MiddleMetacarpal + index;
                case HandFinger.Ring: return (int)JointName.RingMetacarpal + index;
                case HandFinger.Pinky: return (int)JointName.LittleMetacarpal + index;
                default:
                    return 0;
            }
        }
#endif
    }
}