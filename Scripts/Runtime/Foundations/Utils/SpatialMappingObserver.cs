#pragma warning disable 649
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_XR_SDK
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;
#else
using UnityEngine.XR.WSA;
#endif // ENABLE_XR_SDK

namespace Bouvet.DevelopmentKit.Internal.Utils
{
    /// <summary>
    /// Simple spatialmapping class. Modified for both old and new input system, though new input system is not thoroughly tested yet.
    /// </summary>
    public class SpatialMappingObserver : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Main camera")]
        private Transform mainCamera;

        [SerializeField]
        [Tooltip("Prefab used for spatial mesh creation. A sample prefab can be found in Packages/Bouvet MR WebRTC/Sample/Runtime/Other/SpatialMesh.prefab")]
        private GameObject spatialMeshPrefab;

        [SerializeField]
        [Tooltip("Material override for spatial mesh")]
        private Material spatialMeshMaterial;

        [SerializeField]
        [Tooltip("Extents that are used for spatial mesh updates in (x, y, z)")]
        private Vector3 mappingExtents = new Vector3(7, 2, 7);

        [SerializeField]
        [Range(0, 2000)]
        [Tooltip("Amount of triangles (quality) of spatial mesh")]
        private int trianglesPerMesh = 1000;

        [SerializeField]
        [Tooltip("Update time for spatial mesh")]
        private float updateTimeSeconds = 1.5f;

        [SerializeField]
        [Tooltip("View-distance for spatial mesh, to prevent it rendering everything when toggled on.")]
        private float spatialMeshViewDistance = 15f;

        [SerializeField]
        [Tooltip("Sets spatial mesh visible/invisible when generating the mesh")]
        private bool visibleMesh;

        [SerializeField]
        [Tooltip("Should the observer start on application start?")]
        private bool startImmediately = true;

        [SerializeField]
        [Tooltip("If the GameObject that this script is attached to is disabled, should spatialmapping pause?")]
        private bool startStopOnGameObjectSetActive = true;

        [SerializeField]
        [Tooltip("Layer assigned to spatial mesh prefab")]
        private int spatialMeshLayer = 31;

        public static SpatialMappingObserver Instance;
        public static bool IsInitialized;

        private Coroutine updateMeshCoroutine;
        private bool usePrefab;
        private bool useMaterial;

#if ENABLE_XR_SDK
        private readonly Dictionary<MeshId, MeshObject> spatialMeshes = new Dictionary<MeshId, MeshObject>();
        private readonly List<MeshInfo> meshInfos = new List<MeshInfo>();
        private static XRMeshSubsystem meshSystem;
#else // ENABLE_XR_SDK
        private readonly Dictionary<SurfaceId, MeshObject> spatialMeshes = new Dictionary<SurfaceId, MeshObject>();
        private SurfaceObserver surfaceObserver;
#endif // !ENABLE_XR_SDK

        private void Start()
        {
            if (startImmediately)
            {
                StartObserver();
            }
        }

        private void OnEnable()
        {
            if (startStopOnGameObjectSetActive)
            {
                StartObserver();
            }
        }

        private void OnDisable()
        {
            if (startStopOnGameObjectSetActive)
            {
                StopObserver();
            }
        }

        /// <summary>
        /// Starts the observer
        /// </summary>
        public void StartObserver()
        {
#if UNITY_EDITOR
            Debug.Log("Spatialmapping not available in Unity Editor.");
            enabled = false;
            return;
#endif // UNITY_EDITOR

#pragma warning disable 162
#if !WINDOWS_UWP
            enabled = false;
            return;
#endif

            if (updateMeshCoroutine != null)
            {
                return;
            }

            Initialize();

#if ENABLE_XR_SDK
            meshSystem.meshDensity = trianglesPerMesh / 2000f;
            meshSystem.Start();
#endif // ENABLE_XR_SDK
            updateMeshCoroutine = StartCoroutine(nameof(UpdateLoop));
#pragma warning restore 162
        }

        /// <summary>
        /// Stops the observer
        /// </summary>
        public void StopObserver()
        {
            if (updateMeshCoroutine == null)
            {
                return;
            }

#if ENABLE_XR_SDK
            meshSystem.Stop();
#endif // ENABLE_XR_SDK

            StopCoroutine(updateMeshCoroutine);
            updateMeshCoroutine = null;
        }

        /// <summary>
        /// Toggles spatial mesh visiblity to given bool value
        /// </summary>
        /// <param name="setTo"></param>
        public void ToggleSpatialMesh(bool setTo)
        {
            if (visibleMesh == setTo)
            {
                return;
            }

            visibleMesh = setTo;

            UpdateSpatialMeshVisibility();
            //foreach (MeshObject entry in spatialMeshes.Values)
            //{
            //    entry.meshRenderer.enabled = visibleMesh;
            //}
        }

        /// <summary>
        /// Toggles spatial mesh visibility to the opposite of the current setting
        /// </summary>
        public void ToggleSpatialMesh()
        {
            visibleMesh = !visibleMesh;
            UpdateSpatialMeshVisibility();
        }

        /// <summary>
        /// Clears generated spatial mesh
        /// </summary>
        public void ClearSpatialMesh()
        {
            foreach (MeshObject entry in spatialMeshes.Values)
            {
                Destroy(entry.gameObject);
            }

            spatialMeshes.Clear();
        }

        /// <summary>
        /// Attempts to load all existing spatial mesh chunks.
        /// </summary>
        public void ForceLoadSpatialMesh()
        {
            if (meshSystem.TryGetMeshInfos(meshInfos))
            {
                LoadAllSpatialMeshes(meshInfos);
            }
        }

        #region PRIVATE

#if !ENABLE_XR_SDK
        [Obsolete]
#endif // !ENABLE_XR_SDK
        private IEnumerator UpdateLoop()
        {
            while (enabled)
            {
                try
                {
#if ENABLE_XR_SDK
                    meshSystem.SetBoundingVolume(mainCamera.position, mappingExtents);
                    if (meshSystem.TryGetMeshInfos(meshInfos))
                    {
                        UpdateMeshes(meshInfos);
                    }
#else // ENABLE_XR_SDK
                    surfaceObserver.SetVolumeAsAxisAlignedBox(mainCamera.position, mappingExtents);
                    surfaceObserver.Update(OnSurfaceChanged);
#endif // !ENABLE_XR_SDK
                }
                catch (Exception e)
                {
                    BdkLogger.Log($"Error updating mesh. {e.Message}", LogSeverity.Error);
                }

                UpdateSpatialMeshVisibility();

                yield return new WaitForSeconds(updateTimeSeconds);
            }
        }

        /// <summary>
        /// Updates spatial mesh visibility, only making meshes within a certain distance visible to cut down on perf loss.
        /// Otherwise large enough spatial meshes can slow the hololens and even make the app unresponsive.
        /// </summary>
        private void UpdateSpatialMeshVisibility()
        {
            if (!visibleMesh)
            {
                foreach (MeshObject entry in spatialMeshes.Values)
                {
                    entry.meshRenderer.enabled = false;
                }
            }

            else foreach (MeshObject entry in spatialMeshes.Values)
                {
                    if (Vector3.Distance(mainCamera.position, entry.meshRenderer.bounds.center) < spatialMeshViewDistance)
                    {
                        entry.meshRenderer.enabled = visibleMesh;
                    }


                    else if (entry.meshRenderer.enabled == true)
                        entry.meshRenderer.enabled = false;
                }
        }

#if ENABLE_XR_SDK
        /// <summary>
        /// Updates meshes based on the result of the MeshSubsystem.TryGetMeshInfos method.
        /// </summary>
        private void UpdateMeshes(IEnumerable<MeshInfo> meshInfoList)
        {
            if (!meshSystem.running) { return; }

            foreach (MeshInfo meshInfo in meshInfoList)
            {
                switch (meshInfo.ChangeState)
                {
                    case MeshChangeState.Added:
                    case MeshChangeState.Updated:
                        RequestMesh(meshInfo.MeshId);
                        break;

                    case MeshChangeState.Removed:
                        RemoveMesh(meshInfo.MeshId);
                        break;
                }
            }
        }

        private void LoadAllSpatialMeshes(IEnumerable<MeshInfo> meshInfoList)
        {
            BdkLogger.Log($"LoadAllSpatialMeshes start: Spatial mesh chunks: {spatialMeshes.Count}. ");
            int meshCount = spatialMeshes.Count;

            foreach (MeshInfo meshInfo in meshInfoList)
            {
                RequestMesh(meshInfo.MeshId);
            }

            meshCount -= spatialMeshes.Count;
            BdkLogger.Log($"LoadAllSpatialMeshes complete: Spatial mesh chunks: {spatialMeshes.Count}. {meshCount} new chunks. ");
        }

        private void RequestMesh(MeshId meshId)
        {
            MeshObject surface;
            if (!spatialMeshes.ContainsKey(meshId))
            {
                surface = GetMeshObject(meshId.ToString());
                spatialMeshes.Add(meshId, surface);
            }
            else
            {
                surface = spatialMeshes[meshId];
            }

            meshSystem.GenerateMeshAsync(meshId, surface.meshFilter.mesh, surface.meshCollider, MeshVertexAttributes.Normals, OnMeshGenerationComplete);
        }

        private void RemoveMesh(MeshId meshId)
        {
            if (spatialMeshes.ContainsKey(meshId))
            {
                GameObject obj = spatialMeshes[meshId].gameObject;
                spatialMeshes.Remove(meshId);
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }

        private static void OnMeshGenerationComplete(MeshGenerationResult result)
        {
            //WebRtcLogger.Log($"Status:{result.Status}", LogSeverity.Custom);
        }
#else // ENABLE_XR_SDK
        [Obsolete]
        private void OnSurfaceChanged(SurfaceId surfaceId, SurfaceChange changeType, Bounds bounds, DateTime updateTime)
        {
            try
            {
                switch (changeType)
                {
                    case SurfaceChange.Added:
                    case SurfaceChange.Updated:
                        MeshObject surface;
                        if (!spatialMeshes.ContainsKey(surfaceId))
                        {
                            surface = GetMeshObject(surfaceId.ToString());
                            spatialMeshes.Add(surfaceId, surface);
                        }
                        else
                        {
                            surface = spatialMeshes[surfaceId];
                        }

                        SurfaceData sd = new SurfaceData(
                            // the surface id returned from the system
                            surfaceId,
                            // the mesh filter that is populated with the spatial mapping data for this mesh
                            surface.MeshFilter,
                            // the world anchor used to position the spatial mapping mesh in the world
                            surface.GameObject.GetComponent<WorldAnchor>(),
                            // the mesh collider that is populated with collider data for this mesh, if true is passed to bakeMeshes below
                            surface.MeshCollider,
                            // triangles per cubic meter requested for this mesh
                            trianglesPerMesh,
                            // bakeMeshes - if true, the mesh collider is populated, if false, the mesh collider is empty.
                            true
                        );

                        _ = surfaceObserver.RequestMeshAsync(sd, OnDataReady);
                        break;
                    case SurfaceChange.Removed:
                        if (spatialMeshes.ContainsKey(surfaceId))
                        {
                            GameObject obj = spatialMeshes[surfaceId].GameObject;
                            spatialMeshes.Remove(surfaceId);
                            if (obj != null)
                            {
                                Destroy(obj);
                            }
                        }

                        break;
                    default:
                        return;
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        private static void OnDataReady(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
        {
            // Ignore
        }
#endif // !ENABLE_XR_SDK

        private MeshObject GetMeshObject(string id)
        {
            MeshObject surface = new MeshObject();
            if (usePrefab)
            {
                surface.gameObject = Instantiate(spatialMeshPrefab, transform);

                surface.meshRenderer = surface.gameObject.GetComponent<MeshRenderer>();
                surface.meshFilter = surface.gameObject.GetComponent<MeshFilter>();
                surface.meshCollider = surface.gameObject.GetComponent<MeshCollider>();
            }
            else
            {
                surface.gameObject = new GameObject();
                surface.gameObject.transform.SetParent(transform);
                surface.gameObject.layer = spatialMeshLayer;

                surface.meshRenderer = surface.gameObject.AddComponent<MeshRenderer>();
                surface.meshFilter = surface.gameObject.AddComponent<MeshFilter>();
                surface.meshCollider = surface.gameObject.AddComponent<MeshCollider>();
            }

            if (useMaterial)
            {
                surface.meshRenderer.sharedMaterial = spatialMeshMaterial;
            }

            surface.meshRenderer.enabled = visibleMesh;
            surface.gameObject.name = $"spatial-mapping-{id}";

            return surface;
        }

        private void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (IsInitialized)
            {
                return;
            }

            usePrefab = spatialMeshPrefab != null;
            useMaterial = spatialMeshMaterial != null;

            if (!mainCamera)
            {
                mainCamera = Camera.main.transform;
            }

#if ENABLE_XR_SDK
            if (meshSystem == null)
            {
                List<XRMeshSubsystem> xrMeshSubsystems = new List<XRMeshSubsystem>();
                SubsystemManager.GetInstances(xrMeshSubsystems);
                foreach (XRMeshSubsystem xrMeshSubsystem in xrMeshSubsystems)
                {
                    if (xrMeshSubsystem.running)
                    {
                        meshSystem = xrMeshSubsystem;
                        break;
                    }
                }
            }

            if (meshSystem == null)
            {
                BdkLogger.Log("Failed to load mesh system", LogSeverity.Error);
                enabled = false;
                return;
            }
#else // ENABLE_XR_SDK
            surfaceObserver = new SurfaceObserver();
#endif // !ENABLE_XR_SDK
            IsInitialized = true;
        }

        [Serializable]
        private class MeshObject
        {
            public GameObject gameObject;
            public MeshRenderer meshRenderer;
            public MeshFilter meshFilter;
            public MeshCollider meshCollider;
        }

        #endregion // PRIVATE
    }
}