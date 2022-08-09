using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
    /// <summary>
    /// Base class for all interactable object in the scene
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        protected InputManager inputManager;

        public InputSource primaryInputSource;

        private void Start()
        {
            inputManager = InputManager.Instance;
            Initialize();
        }

        public virtual void Initialize()
        {
        }

        public virtual void BeginInteraction(InputSource source)
        {
        }

        public virtual void UpdateInteraction(InputSource source)
        {
        }

        public virtual void EndInteraction(InputSource source)
        {
        }

        public virtual void OnFocusBegin()
        {
        }

        public virtual void OnFocusEnd()
        {
        }
    }
}
