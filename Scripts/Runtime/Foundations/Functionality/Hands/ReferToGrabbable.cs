using Bouvet.DevelopmentKit.Input;

namespace Bouvet.DevelopmentKit.Functionality.Hands
{
    public class ReferToGrabbable : Interactable
    {
#pragma warning disable CS0649
        public Grabbable referableGrabbable;

        public bool alignWithGrabbableTransform = false;

        public override void Initialize()
        {
            base.Initialize();
            inputManager.OnManipulationStarted += BeginInteraction;
        }

        public override void BeginInteraction(InputSource inputSource)
        {
            if (gameObject.Equals(inputSource.collidedObject))
            {
                inputSource.collidedObject = referableGrabbable.gameObject;
                referableGrabbable.BeginInteraction(inputSource);
                if (inputSource.inputSourceKind == InputSourceKind.InteractionBeamRight)
                {
                    inputManager.inputSettings.CursorManager.rightHandInteractionBeam.UpdateTargetTransfrom(referableGrabbable.transform, alignWithGrabbableTransform);
                }

                if (inputSource.inputSourceKind == InputSourceKind.InteractionBeamLeft)
                {
                    inputManager.inputSettings.CursorManager.leftHandInteractionBeam.UpdateTargetTransfrom(referableGrabbable.transform, alignWithGrabbableTransform);
                }
            }
        }

        private void OnDestroy()
        {
            inputManager.OnManipulationStarted -= BeginInteraction;
        }
#pragma warning restore CS0649
    }
}