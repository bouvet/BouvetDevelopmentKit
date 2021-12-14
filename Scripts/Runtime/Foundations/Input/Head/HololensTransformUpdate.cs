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
            inputSource.worldPosition = transform.position;
            inputSource.forwardVector = transform.forward;
            if (Physics.Raycast(inputSource.worldPosition, inputSource.forwardVector, out hit, 20f))
            {
                inputSource.collidedObject = hit.transform.gameObject;
            }
            else
            {
                inputSource.collidedObject = null;
            }

            inputManager.InvokeHololensUpdateTransform(inputSource);
        }
    }
#pragma warning restore CS0649
}