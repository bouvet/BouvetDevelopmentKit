using Bouvet.DevelopmentKit.Input;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Functionality.Head
{
    /// <summary>
    /// This class is initialized by InputManagerInternal if cursor is enabled in the InputSettings. 
    /// </summary>
    public class HeadGazeCursor : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        internal InputManager inputManager;

        protected RaycastHit hit;
        protected InputSource holoCursor;
        protected InputSettings inputSettings;
        protected MeshRenderer visualCursor;

        protected void Start()
        {
            inputSettings = inputManager.inputSettings;
            visualCursor = GetComponent<MeshRenderer>();
            holoCursor = new InputSource
            {
                inputSourceKind = InputSourceKind.HeadGaze
            };
            inputManager.OnHololensTransformUpdated += InputManager_OnHololensTransformUpdated;
        }

        protected void InputManager_OnHololensTransformUpdated(InputSource obj)
        {
            if (inputSettings.UseHeadGaze && (inputManager.GetCursorState(true) == CursorState.HeadCursor || inputSettings.AlwaysShowHeadGazeCursor))
            {
                if (Physics.Raycast(obj.worldPosition, obj.forwardVector, out hit, 20f))
                {
                    if (!visualCursor.enabled)
                    {
                        visualCursor.enabled = true;
                    }

                    transform.position = hit.point;
                    holoCursor.collidedObject = hit.transform.gameObject;
                }
                else
                {
                    visualCursor.enabled = false;
                }
            }
            else
            {
                visualCursor.enabled = false;
            }
        }
    }
#pragma warning restore CS0649
}