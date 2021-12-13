using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

public class EyeGazeCursor : MonoBehaviour
{
    public InputManager inputManager;
    public GameObject visualComponent;

    protected void Start()
    {
        inputManager.OnGazeUpdate += InputManager_OnGazeUpdate;
        inputManager.OnGazeExit += InputManager_OnGazeExit;
        if (visualComponent)
            visualComponent.SetActive(false);
    }

    protected void InputManager_OnGazeExit(InputSource obj)
    {
        visualComponent.SetActive(false);
    }

    protected void InputManager_OnGazeUpdate(InputSource obj)
    {        
        if(inputManager.inputSettings.ShowEyeGazeCursor || inputManager.inputSettings.AlwaysShowEyeGazeCursor)
        {
            if (!visualComponent.activeSelf)
                visualComponent.SetActive(true);
        }

        transform.position = Vector3.Lerp(visualComponent.transform.position, TypeHelpers.MakeUnityVector3(obj.worldPosition), 0.5f);
        transform.rotation = Quaternion.FromToRotation(visualComponent.transform.forward, TypeHelpers.MakeUnityVector3(-obj.forwardVector));
    }
}
