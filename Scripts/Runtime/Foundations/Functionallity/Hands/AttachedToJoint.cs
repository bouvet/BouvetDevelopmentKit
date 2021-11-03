using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
#pragma warning disable CS0649
    public class AttachedToJoint : MonoBehaviour
    {
        [SerializeField]
        public InputManager inputManager;

        [SerializeField]
        public InputSourceKind inputSourceKind;

        [SerializeField]
        public JointName joint;

        [SerializeField]
        protected GameObject visibleComponents;

        [SerializeField]
        protected bool matchTargetPosition = true;

        [SerializeField]
        protected bool matchTargetRotation = true;

        [SerializeField]
        protected bool interpolatePositionAndRotation;

        [Range(0.1f, 1f)]
        [SerializeField]
        protected float interpolationSpeed = 0.5f;

        protected bool active;
        protected Vector3 position;
        protected Quaternion rotation;

        protected virtual void Start()
        {
            if (!inputManager)
            {
                inputManager = InputManager.Instance;
            }

            inputManager.OnSourceFound += InputManager_OnSourceFound;
            inputManager.OnSourceLost += InputManager_OnSourceLost;
            
            if (visibleComponents && inputManager.TryGetInputSource(inputSourceKind, out InputSource inputSource, false))
            {
                visibleComponents.SetActive(false);
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (active && inputManager.TryGetHandJointTransform(inputSourceKind, joint, out position, out rotation))
            {
                if (visibleComponents && !visibleComponents.activeSelf)
                {
                    visibleComponents.SetActive(true);
                }

                if (matchTargetPosition)
                {
                    if (interpolatePositionAndRotation)
                    {
                        transform.position = Vector3.Lerp(transform.position, position, interpolationSpeed);
                    }
                    else
                    {
                        transform.position = position;
                    }
                }

                if (matchTargetRotation)
                {
                    if (interpolatePositionAndRotation)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, interpolationSpeed);
                    }
                    else
                    {
                        transform.rotation = rotation;
                    }
                }
            }
            else if (visibleComponents && visibleComponents.activeSelf)
            {
                visibleComponents.SetActive(false);
            }
        }

        protected virtual void InputManager_OnSourceFound(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == inputSourceKind)
            {
                active = true;
            }
        }

        public virtual void SetupAttachedJoint(InputManager manager, InputSourceKind sourceKind, JointName jointName, GameObject visibleComponent = null, bool matchPosition = true, bool matchRotation = true)
        {
            inputManager = manager;
            inputSourceKind = sourceKind;
            joint = jointName;
            visibleComponents = visibleComponent;
            matchTargetPosition = matchPosition;
            matchTargetRotation = matchRotation;
        }

        protected virtual void InputManager_OnSourceLost(InputSource inputSource)
        {
            if (inputSource.inputSourceKind == inputSourceKind)
            {
                active = false;
            }
        }
    }
#pragma warning restore CS0649
}