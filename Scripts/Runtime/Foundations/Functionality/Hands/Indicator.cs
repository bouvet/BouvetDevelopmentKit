using Bouvet.DevelopmentKit.Internal.Utils;
using System;
using Bouvet.DevelopmentKit.Input;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
#pragma warning disable CS0649
    /// <summary>
    /// This class deals with correctly animating the indicator on the index fingers.
    /// </summary>
    public class Indicator : MonoBehaviour
    {
        // Public variables
        public bool isRightHand;

        [SerializeField]
        internal Transform indexIndicatorAnchor;

        [SerializeField]
        private float collisionDistance = 0.05f;

        public Collider[] itemsInProximity = new Collider[20];
        private InputManager inputManager;

        private float listMinDistance;
        private int listMinIndex;
        private float listTempDistance;

        private Transform otherTransform;

        private void Start()
        {
            inputManager = InputManager.Instance;
            inputManager.OnInputUpdated += InputManager_OnInputUpdated;
        }
        private void Update()
        {
#if UNITY_EDITOR
            if (!otherTransform)
            {
                otherTransform = FindClosest();
            }

            if (otherTransform)
            {
                float distance = Vector3.Distance(otherTransform.position, otherTransform.TransformPoint(Vector3.Scale(otherTransform.InverseTransformPoint(transform.position), Vector3.forward)));
                distance = Mathf.Abs(distance);
                UpdateVisuals(distance, true);
                RotateToSurfaceNormal(indexIndicatorAnchor, otherTransform.forward, transform.rotation, distance);
            }
            else
            {
                UpdateVisuals(1f, true);
            }
#endif
            itemsInProximity = Physics.OverlapSphere(transform.position, collisionDistance);
            if (itemsInProximity.Length > 0)
            {
                foreach(Collider other in itemsInProximity)
                {
                    HandProximityField handProximityField = other.GetComponent<HandProximityField>();

                    if (handProximityField && (isRightHand && handProximityField.ignoreHand != HandInteractionMode.Right || !isRightHand && handProximityField.ignoreHand != HandInteractionMode.Left))
                    {
                        inputManager.SetCursorState(true, isRightHand);
                        break;
                    }

                    if (other.gameObject.GetComponent<Grabbable>())
                    {
                        inputManager.SetCursorState(true, isRightHand);
                        break;
                    }
                    
                    if(other.gameObject.GetComponent<Interactable>())
                    {
                        inputManager.SetCursorState(true, isRightHand);
                        break;
                    }
                }
            }
            else
            {
                inputManager.SetCursorState(false, isRightHand);
            }
        }

        private void OnEnable()
        {
            HideIndicator();
        }

        /// <summary>
        /// Input update is called every frame. If the hand is visible and the current CursorState is CursorState.IndexFingerCursor
        /// </summary>
        /// <param name="inputSource"></param>
        private void InputManager_OnInputUpdated(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == InputSourceKind.HandRight && isRightHand
                || inputSource.inputSourceKind == InputSourceKind.HandLeft && !isRightHand)
            {
                if (inputManager.GetCursorState(isRightHand) == CursorState.IndexFingerCursor && inputManager.inputSettings.ShowIndexIndicator)
                {
                    otherTransform = FindClosest();
                    if (otherTransform)
                    {
                        float distance = Vector3.Distance(otherTransform.position, otherTransform.TransformPoint(Vector3.Scale(otherTransform.InverseTransformPoint(transform.position), Vector3.forward)));
                        distance = Mathf.Abs(distance);
                        UpdateVisuals(distance, true);
                        RotateToSurfaceNormal(indexIndicatorAnchor, otherTransform.forward, transform.rotation, distance);
                    }
                    else
                    {
                        if (!indexIndicatorAnchor.gameObject.activeSelf)
                        {
                            indexIndicatorAnchor.gameObject.SetActive(true);
                        }

                        indexIndicatorAnchor.localRotation = Quaternion.identity;
                        indexIndicatorAnchor.localScale = Vector3.one * 100f;
                    }
                }
                else
                {
                    if (otherTransform)
                    {
                        otherTransform = null;
                    }

                    HideIndicator();
                }
            }
        }

        /// <summary>
        /// Changes scale of indicator
        /// </summary>
        /// <param name="distance">Distance between the ring and surface.</param>
        /// <param name="visible">Should the ring be visible?</param>
        protected virtual void UpdateVisuals(float distance, bool visible)
        {
            if (visible)
            {
                if (!indexIndicatorAnchor.gameObject.activeSelf)
                {
                    indexIndicatorAnchor.gameObject.SetActive(true);
                }

                distance = Mathf.Clamp(distance, 0.02f, 0.05f);
                indexIndicatorAnchor.localScale = new Vector3(distance * 2000f, distance * 2000f, distance * 2000f);
            }
            else if (indexIndicatorAnchor.gameObject.activeSelf)
            {
                indexIndicatorAnchor.gameObject.SetActive(false);
            }
        }

        private void HideIndicator()
        {
            UpdateVisuals(1f, false);
        }

        private void RotateToSurfaceNormal(Transform target, Vector3 surfaceNormal, Quaternion pointerRotation, float distance)
        {
            float t = distance / 0.1f;
            Quaternion targetRotation = Quaternion.LookRotation(surfaceNormal);
            target.rotation = Quaternion.Slerp(targetRotation, pointerRotation, t);
        }

        private Transform FindClosest()
        {

            listMinDistance = 1000;
            listMinIndex = -1;
            for (int i = 0; i < itemsInProximity.Length; i++)
            {
                if (itemsInProximity[i] == null)
                {
                    continue;
                }
                listTempDistance = -itemsInProximity[i].transform.InverseTransformPoint(transform.position).z;
                if (listTempDistance < listMinDistance)
                {
                    listMinDistance = listTempDistance;
                    listMinIndex = i;
                }
            }

            if (listMinIndex != -1)
            {
                return itemsInProximity[listMinIndex].transform;
            }

            return null;
        }
    }
#pragma warning restore CS0649
}