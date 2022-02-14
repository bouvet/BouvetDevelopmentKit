using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools
{
    public class ToggleObject : MonoBehaviour
    {
#pragma warning disable CS0649
    [SerializeField]
        protected GameObject toggleObject;

        [SerializeField]
        protected bool initialState;

        protected void Start()
        {
            toggleObject.SetActive(initialState);
        }

        public void Toggle()
        {
            toggleObject.SetActive(!toggleObject.activeSelf);
        }
#pragma warning restore CS0649
    }
}