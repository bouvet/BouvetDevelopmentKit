using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using Bouvet.DevelopmentKit.Internal.Utils;
using UnityEngine;

public class HandMesh : MonoBehaviour
{
    public Transform[] jointTransforms;
    public InputSourceKind inputSourceKind;
    public SkinnedMeshRenderer handMesh;

    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    private Vector3 eulerRotationOffset = new Vector3(90f, -90f, 0f);

    private bool active;
    private InputManager inputManager;
    private Vector3 position;
    private Quaternion rotation;

    private void Start()
    {
        inputManager = InputManager.Instance;
        inputManager.OnSourceFound += InputManager_OnSourceFound;
        inputManager.OnSourceLost += InputManager_OnSourceLost;
    }

    private void Update()
    {
        if (active)
        {
            eulerRotationOffset = new Vector3(xSlider.currentValue * 360f, ySlider.currentValue * 360f, zSlider.currentValue * 360f);

            for (int i = 0; i < jointTransforms.Length; i++)
            {
                if (jointTransforms[i] != null && inputManager.TryGetHandJointTransform(inputSourceKind, (JointName)i, out position, out rotation))
                {
                    // With smoothing
                    //transform.position = Vector3.Lerp(transform.position, position, interpolationSpeed);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, interpolationSpeed);

                    // without smoothing
                    jointTransforms[i].position = position;
                    jointTransforms[i].rotation = rotation;
                    jointTransforms[i].localEulerAngles += eulerRotationOffset;
                }
            }
        }
    }

    private void InputManager_OnSourceLost(InputSource inputSource)
    {
        if (inputSource.inputSourceKind == inputSourceKind)
        {
            active = false;
            handMesh.enabled = false;
        }
    }

    private void InputManager_OnSourceFound(InputSource inputSource)
    {
        if (inputSource.inputSourceKind == inputSourceKind)
        {
            active = true;
            handMesh.enabled = true;
        }
    }
}
