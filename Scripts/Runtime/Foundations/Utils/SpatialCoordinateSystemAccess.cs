using UnityEngine;
#if WINDOWS_UWP
using Application = UnityEngine.WSA.Application;
using UnityEngine.XR.WSA;
using System;
using System.Runtime.InteropServices;
using Windows.Perception.Spatial;
using Windows.UI.Input.Spatial;

#endif

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    /// <summary>
    /// This class holds variables that allows the user to access the spatial coordinate system.
    /// </summary>
    public class SpatialCoordinateSystemAccess : MonoBehaviour
    {
#pragma warning disable CS0618
#pragma warning disable CS0067
#if WINDOWS_UWP
        public event Action OnSpatialInteractionManagerFound;
        internal static bool spatialInteractionManagerFound;
        private static SpatialCoordinateSystem spatialCoordinateSystem;
        private static IntPtr currentSpatialCoordinateSystemPtr;
        private static SpatialInteractionManager spatialInteractionManager;

        private void Awake()
        {
            try
            {
                Application.InvokeOnUIThread(() =>
                {
                    spatialInteractionManager = SpatialInteractionManager.GetForCurrentView();
                }, true);
                BdkLogger.Log("Initialized", LogSeverity.Info);
                spatialInteractionManagerFound = true;
            }
            catch (Exception e)
            {
                BdkLogger.LogException("Error setting up SpatialCoordinateSystemAccess for SpatialInteractionManager in Awake", e);
            }
        }

        public static SpatialInteractionManager SpatialInteractionManager
        {
            get
            {
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
                if (spatialInteractionManagerFound)
                {
                    return spatialInteractionManager;
                }

                if (spatialInteractionManager != null)
                {
                    spatialInteractionManagerFound = true;
                    return spatialInteractionManager;
                }

                return null;
#endif
            }
        }

        /// <summary>
        /// Static access to the Spatial Coordinate System
        /// </summary>
        public static SpatialCoordinateSystem SpatialCoordinateSystem
        {
            get
            {
#if ENABLE_XR_SDK
                IntPtr newSpatialCoordinateSystemPtr = UnityEngine.XR.XRDevice.GetNativePtr();
#else
                IntPtr newSpatialCoordinateSystemPtr = WorldManager.GetNativeISpatialCoordinateSystemPtr();
#endif
                if (newSpatialCoordinateSystemPtr != currentSpatialCoordinateSystemPtr && newSpatialCoordinateSystemPtr != IntPtr.Zero)
                {
#if ENABLE_DOTNET
                        spatialCoordinateSystem = GetSpatialCoordinateSystem(newSpatialCoordinateSystemPtr);
#elif WINDOWS_UWP
                    spatialCoordinateSystem = Marshal.GetObjectForIUnknown(newSpatialCoordinateSystemPtr) as SpatialCoordinateSystem;
#elif DOTNETWINRT_PRESENT
                        spatialCoordinateSystem = SpatialCoordinateSystem.FromNativePtr(newSpatialCoordinateSystemPtr);
#endif
                    currentSpatialCoordinateSystemPtr = newSpatialCoordinateSystemPtr;
                }

                return spatialCoordinateSystem;
            }
        }
#endif
#if ENABLE_DOTNET
        [DllImport("DotNetNativeWorkaround.dll", EntryPoint = "MarshalIInspectable")]
        private static extern void GetSpatialCoordinateSystem(IntPtr nativePtr, out SpatialCoordinateSystem coordinateSystem);

        private static SpatialCoordinateSystem GetSpatialCoordinateSystem(IntPtr nativePtr)
        {
            try
            {
                SpatialCoordinateSystem coordinateSystem;
                GetSpatialCoordinateSystem(nativePtr, out coordinateSystem);
                return coordinateSystem;
            }
            catch
            {
                UnityEngine.Debug.LogError("Call to the DotNetNativeWorkaround plug-in failed. The plug-in is required for correct behavior when using .NET Native compilation");
                return Marshal.GetObjectForIUnknown(nativePtr) as SpatialCoordinateSystem;
            }
        }
#endif
#pragma warning restore CS0618
#pragma warning restore CS0067
    }
}