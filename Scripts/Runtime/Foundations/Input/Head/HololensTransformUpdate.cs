using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Input.Gaze
{
    public class HololensTransformUpdate : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        internal InputManager inputManager;

        private RaycastHit hit;
        private InputSource inputSource;

        private void Start()
        {
            inputSource = new InputSource();
            inputSource.inputSourceKind = InputSourceKind.Hololens;
            inputManager.AddInputSource(inputSource);
        }

        private void Update()
        {
            inputSource.worldPosition = TypeHelpers.MakeSystemVector3(transform.position);
            inputSource.forwardVector = TypeHelpers.MakeSystemVector3(transform.forward);
            if (Physics.Raycast(TypeHelpers.MakeUnityVector3(inputSource.worldPosition), TypeHelpers.MakeUnityVector3(inputSource.forwardVector), out hit, 20f))
            {
                inputSource.collidedObjectIdentifier = hit.transform.gameObject.GetInstanceID();
            }
            else
            {
                inputSource.collidedObjectIdentifier = 0;
            }

            inputManager.InvokeHololensUpdateTransform(inputSource);
        }
    }
#pragma warning restore CS0649
}