#if ENABLE_XR_SDK && UNITY_2020_1_OR_NEWER
using System.Collections.Generic;
using Bouvet.DevelopmentKit.Internal.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace Bouvet.DevelopmentKit
{
    public class WorldAnchorsXrsdk : MonoBehaviour
    {
        public static WorldAnchorsXrsdk Instance { get; private set; }

        [SerializeField]
        private bool enableLogging = false;

        private readonly Dictionary<TrackableId, Transform> anchorObjects = new Dictionary<TrackableId, Transform>();
        private XRAnchorSubsystem anchorPointsSubsystem;
        private bool initialized;

        protected void Awake()
        {
            Instance = this;
            CreateReferencePointSubSystem();
        }

        private void Start()
        {
            if (initialized && enableLogging)
            {
                BdkLogger.Log("Initialized anchor point subsystem.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
            }
            else if (!initialized)
            {
                BdkLogger.Log("Failed to start anchor point subsystem.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
            }
        }

        private void LateUpdate()
        {
            if (!initialized || anchorObjects.Count <= 0)
            {
                return;
            }

            NativeArray<XRAnchor> updatedAnchors = anchorPointsSubsystem.GetChanges(Allocator.Temp).updated;

            if (updatedAnchors.Length <= 0)
            {
                return;
            }

            for (int j = 0; j < updatedAnchors.Length; j++)
            {
                if (anchorObjects.TryGetValue(updatedAnchors[j].trackableId, out Transform anchorObject))
                {
                    anchorObject.position = updatedAnchors[j].pose.position;
                    anchorObject.rotation = updatedAnchors[j].pose.rotation;
                }
            }
        }

        protected void OnDestroy()
        {
            anchorPointsSubsystem?.Stop();
        }

        public bool TryAddAnchor(Transform transformToAnchor, out TrackableId trackableId)
        {
            if (!initialized)
            {
                if (enableLogging)
                {
                    BdkLogger.Log("Can't add anchor. AnchorStoreManager not initialized.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }

                trackableId = TrackableId.invalidId;
                return false;
            }

            Pose pose = new Pose(transformToAnchor.position, transformToAnchor.rotation);

            if (anchorPointsSubsystem.TryAddAnchor(pose, out XRAnchor anchor))
            {
                if (!anchorObjects.ContainsKey(anchor.trackableId))
                {
                    trackableId = anchor.trackableId;
                    anchorObjects.Add(anchor.trackableId, transformToAnchor);
                    if (enableLogging)
                    {
                        BdkLogger.Log($"Anchor for {transformToAnchor.name} created.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
                    }

                    return true;
                }

                if (enableLogging)
                {
                    BdkLogger.Log($"Anchor for {transformToAnchor.name} already exists.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }
            }
            else
            {
#if UNITY_EDITOR
                if (enableLogging)
                {
                    BdkLogger.Log($"Anchor for {transformToAnchor.name} created - Editor.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
                }

                trackableId = TrackableId.invalidId;
                return true;
#else
                if (enableLogging)
                {
                    BdkLogger.Log("Failed to add reference point for anchor.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }
#endif
            }

            trackableId = TrackableId.invalidId;
            return false;
        }

        public bool TryRemoveAnchor(TrackableId trackableId)
        {
            if (!initialized)
            {
                if (enableLogging)
                {
                    BdkLogger.Log("Can't remove anchor. AnchorStoreManager not initialized.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }

                return false;
            }

            if (anchorObjects.Remove(trackableId))
            {
                if (anchorPointsSubsystem.TryRemoveAnchor(trackableId))
                {
                    if (enableLogging)
                    {
                        BdkLogger.Log($"Anchor {trackableId} removed.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
                    }

                    return true;
                }

                if (enableLogging)
                {
                    BdkLogger.Log($"Couldn't remove anchor {trackableId}, doesn't exist.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }
            }
            else
            {
#if UNITY_EDITOR
                if (enableLogging)
                {
                    BdkLogger.Log($"Anchor {trackableId} removed - Editor.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
                }

                return true;
#else
                // Ignore that it doesn't exist when removing
#endif
            }

            return false;
        }

        public void RemoveAllAnchors()
        {
            if (!initialized)
            {
                if (enableLogging)
                {
                    BdkLogger.Log("Can't remove anchors. AnchorStoreManager not initialized.", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Error);
                }

                return;
            }

            foreach (TrackableId trackableId in anchorObjects.Keys)
            {
                anchorPointsSubsystem.TryRemoveAnchor(trackableId);
            }

            anchorObjects.Clear();
        }

        private void CreateReferencePointSubSystem()
        {
            List<XRAnchorSubsystemDescriptor> rpSubSystemsDescriptors = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(rpSubSystemsDescriptors);

            if (enableLogging)
            {
                string descriptors = "";
                foreach (XRAnchorSubsystemDescriptor descriptor in rpSubSystemsDescriptors)
                {
                    //descriptors += $"{descriptor.id} {descriptor.subsystemImplementationType}\r\n";
                    descriptors += $"{descriptor.id} {descriptor.supportsTrackableAttachments}\r\n";
                }

                BdkLogger.Log($"[{GetType()}] {rpSubSystemsDescriptors.Count} reference point subsystem descriptors:\r\n{descriptors}", Bouvet.DevelopmentKit.Internal.Utils.LogSeverity.Info);
            }

            if (rpSubSystemsDescriptors.Count > 0)
            {
                anchorPointsSubsystem = rpSubSystemsDescriptors[0].Create();
                anchorPointsSubsystem.Start();
            }

            if (anchorPointsSubsystem != null && anchorPointsSubsystem.running)
            {
                initialized = true;
            }
        }
    }
}
#endif