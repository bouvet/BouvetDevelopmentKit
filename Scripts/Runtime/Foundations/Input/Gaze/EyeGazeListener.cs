using System;
using System.Threading;
using System.Threading.Tasks;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;
using UnityEngine.XR;
#if WINDOWS_UWP
using Windows.Perception;
using Windows.UI.Input.Spatial;
#endif

namespace Bouvet.DevelopmentKit.Input.Gaze
{
#pragma warning disable CS0649
#pragma warning disable CS0414
#pragma warning disable CS1998
    /// <summary>
    /// This class is set up by EyeGazeListenerInternal if eye tracking is enabled in the InputSettings. This class calls the internal event functions for dealing with eye tracking input.
    /// It also sets up the system for accessing the eye tracking information from Unity. 
    /// </summary>
    public class EyeGazeListener : MonoBehaviour
    {
        private bool askedForAccessAlready;
        private int errorCount;
        private bool eyeGazeListenerInitialized;
        private EyeGazeListenerInternal eyeGazeListenerInternal;
        private InputSource gazeInputSource;
        private InputManager inputManager;

        private InputSettings inputSettings;

        // Private variables
        private bool noEyeTrackingFoundLastFrame;
        private RaycastHit raycastHit;
        private bool setupComplete;
        private InputDevice centerEye;
        private InputFeatureUsage<bool> eyeGazeAvailableInput;
        private InputFeatureUsage<Vector3> eyeGazePositionInput;
        private InputFeatureUsage<Quaternion> eyeGazeRotationInput;

        /// <summary>
        /// Update loop that accesses the eye tracking data from Unity.
        /// </summary>
        private void Update()
        {
#if WINDOWS_UWP
            if (!setupComplete || !inputSettings.UseEyeGaze || !eyeGazeListenerInitialized) return;
            try
            {
                centerEye = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
                if(!centerEye.isValid)
                {
                    LostTracking();
                    return;
                }
                if (!centerEye.TryGetFeatureValue(eyeGazeAvailableInput, out bool value) || !value)
                {
                    LostTracking();
                    return;
                }

                if (centerEye.TryGetFeatureValue(eyeGazeAvailableInput, out bool gazeTracked)
                    && gazeTracked
                    && centerEye.TryGetFeatureValue(eyeGazePositionInput, out Vector3 eyeGazePosition)
                    && centerEye.TryGetFeatureValue(eyeGazeRotationInput, out Quaternion eyeGazeRotation))
                {
                    gazeInputSource.worldPosition = TypeHelpers.MakeSystemVector3(eyeGazePosition);
                    gazeInputSource.forwardVector = TypeHelpers.MakeSystemVector3(eyeGazeRotation * Vector3.forward);
                    if (noEyeTrackingFoundLastFrame)
                    {
                        noEyeTrackingFoundLastFrame = false;
                        gazeInputSource.active = true;
                        eyeGazeListenerInternal.SourceFound(gazeInputSource);
                    }

                    BdkLogger.Log($"Eye gaze pos: {gazeInputSource.worldPosition}, Eye gaze forward: {gazeInputSource.forwardVector}");

                    Ray gaze = new Ray(TypeHelpers.MakeUnityVector3(gazeInputSource.worldPosition), TypeHelpers.MakeUnityVector3(gazeInputSource.forwardVector));
                    if (Physics.Raycast(gaze, out raycastHit))
                    {
                        gazeInputSource.worldPosition = new System.Numerics.Vector3(raycastHit.point.x, raycastHit.point.y, -raycastHit.point.z); // Converts to System Vector3
                        gazeInputSource.forwardVector = new System.Numerics.Vector3(raycastHit.normal.x, raycastHit.normal.y, -raycastHit.normal.z); // Converts to System Vector3
                        int newID = inputManager.GetId(raycastHit.transform.gameObject);
                        if (newID != gazeInputSource.collidedObjectIdentifier)
                        {
                            eyeGazeListenerInternal.GazeExit(gazeInputSource);
                            gazeInputSource.collidedObjectIdentifier = newID;
                            eyeGazeListenerInternal.GazeEnter(gazeInputSource);
                        }
                        eyeGazeListenerInternal.GazeUpdate(gazeInputSource);

                        BdkLogger.Log($"Eye cursor pos: {gazeInputSource.worldPosition}, Target: {gazeInputSource.collidedObjectIdentifier}");
                    }
                    else
                    {
                        eyeGazeListenerInternal.GazeExit(gazeInputSource);
                        gazeInputSource.collidedObjectIdentifier = 0;
                    }
                }

                else
                {
                    LostTracking();
                }
                // OLD:
                /*
                SpatialPointerPose pointerPose = SpatialPointerPose.TryGetAtTimestamp(SpatialCoordinateSystemAccess.SpatialCoordinateSystem, PerceptionTimestampHelper.FromHistoricalTargetTime(DateTimeOffset.Now));
                if (pointerPose != null)
                {
                    var eyes = pointerPose.Eyes;
                    if (eyes != null && eyes.IsCalibrationValid)
                    {
                        if (eyes.Gaze.HasValue)
                        {
                            gazeInputSource.worldPosition = eyes.Gaze.Value.Origin;
                            gazeInputSource.forwardVector = eyes.Gaze.Value.Direction;
                            if (noEyeTrackingFoundLastFrame)
                            {
                                noEyeTrackingFoundLastFrame = false;
                                gazeInputSource.active = true;
                                eyeGazeListenerInternal.SourceFound(gazeInputSource);
                            }

                            Ray gaze = new Ray(TypeHelpers.MakeUnityVector3(gazeInputSource.worldPosition), TypeHelpers.MakeUnityVector3(gazeInputSource.forwardVector));
                            if (Physics.Raycast(gaze, out raycastHit))
                            {
                                gazeInputSource.worldPosition = new System.Numerics.Vector3(raycastHit.point.x, raycastHit.point.y, -raycastHit.point.z); // Converts to System Vector3
                                gazeInputSource.forwardVector = new System.Numerics.Vector3(raycastHit.normal.x, raycastHit.normal.y, -raycastHit.normal.z); // Converts to System Vector3
                                int newID = inputManager.GetId(raycastHit.transform.gameObject); //raycastHit.transform.gameObject.GetInstanceID();
                                if (newID != gazeInputSource.collidedObjectIdentifier)
                                {
                                    eyeGazeListenerInternal.GazeExit(gazeInputSource);
                                    gazeInputSource.collidedObjectIdentifier = newID;
                                    eyeGazeListenerInternal.GazeEnter(gazeInputSource);
                                }
                                eyeGazeListenerInternal.GazeUpdate(gazeInputSource);
                            }
                            else
                            {
                                eyeGazeListenerInternal.GazeExit(gazeInputSource);
                                gazeInputSource.collidedObjectIdentifier = 0;
                            }
                        }
                        else
                        {
                            LostTracking();
                        }
                    }
                    else if(!eyes.IsCalibrationValid)
                    {
                        ToolkitLogger.Log($"Eye calibration is invalid. Please generate a calibration file. Disabling Eye Tracking!", LogSeverity.Warn);
                        inputSettings.UseEyeGaze = false;
                        LostTracking();                        
                    }
                    else
                    {
                        LostTracking();
                    }
                }
                else
                {
                    LostTracking();
                }*/
            }
            catch (Exception e)
            {
                BdkLogger.LogException($"Error in update of EyeGazeListener. {e.Source}", e);
                errorCount++;
                if(errorCount > 20)
                {
                    BdkLogger.Log($"Disabling Eye Tracking. Check if Eye calibration is invalid. {e.Source}", LogSeverity.Warn);
                    inputSettings.UseEyeGaze = false;
                }
            }
#endif
        }

        private void StartEyeTracking()
        {
            setupComplete = true;
        }

        /// <summary>
        /// Initialize method for EyeGazeListener
        /// </summary>
        /// <param name="newInputSettings"></param>
        /// <param name="newEyeGazeListenerInternal"></param>
        /// <param name="token"></param>
        /// <returns>True if successful, false if not</returns>
        internal async Task<bool> InitializeAsync(InputSettings newInputSettings, EyeGazeListenerInternal newEyeGazeListenerInternal, CancellationToken token)
        {
            try
            {
#if (UNITY_WSA && DOTNETWINRT_PRESENT) || WINDOWS_UWP
                if (!askedForAccessAlready && Windows.Perception.People.EyesPose.IsSupported())
                {
                    askedForAccessAlready = true;
                    await Windows.Perception.People.EyesPose.RequestAccessAsync();
                }
#endif
                inputSettings = newInputSettings;
                inputManager = inputSettings.inputManager;
                eyeGazeListenerInternal = newEyeGazeListenerInternal;
                gazeInputSource = new InputSource();
                gazeInputSource.inputSourceKind = InputSourceKind.EyeGaze;
                inputSettings.inputManager.AddInputSource(gazeInputSource);
                Invoke(nameof(StartEyeTracking), 2f);
                //StartEyeTracking();
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up EyeGazeListener", e);
                return false;
            }

            eyeGazeListenerInitialized = true;

            BdkLogger.Log("EyeGazeListener setup succesfully", LogSeverity.Info);

            return true;
        }

        /// <summary>
        /// Method called when eye tracking is lost.
        /// </summary>
        private void LostTracking()
        {
            if (noEyeTrackingFoundLastFrame)
            {
                return;
            }

            noEyeTrackingFoundLastFrame = true;
            gazeInputSource.active = false;
            eyeGazeListenerInternal.GazeExit(gazeInputSource);
            gazeInputSource.collidedObjectIdentifier = 0;
            eyeGazeListenerInternal.SourceLost(gazeInputSource);
        }
    }
#pragma warning restore CS0649
#pragma warning restore CS0414
#pragma warning restore CS1998
}