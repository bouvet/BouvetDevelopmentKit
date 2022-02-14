using Bouvet.DevelopmentKit;
using UnityEngine;

public class HandMenuExample : ToggleOnHandRotation
{
#pragma warning disable CS0649
    [SerializeField]
    protected Transform menuAnchor;

    [SerializeField]
    [Tooltip("Hand rotation which will make the menu face the HoloLens. 0 degrees is palm facing HoloLens.")]
    protected bool faceHololens = true;

    [SerializeField]
    [Tooltip("Offset variables:\nX: left/right\nY: up/down\nZ: forward/backwards")]
    protected Vector3 offset = new Vector3(0.1f, 0f, 0f);

    [SerializeField]
    [Tooltip("How far away from the handmenu target position the menu is allowed to be. 0 means it has to be at the position. Recommended 0.01.")]
    [Range(0f, 0.5f)]
    protected float slack = 0.01f;

    protected Transform Hololens;

    protected Vector3 palmPosition;
    protected Quaternion palmRotation;
    protected Transform handRotationCheck;

    protected override void Initialize()
    {
        base.Initialize();
        Hololens = inputManager.Hololens;
        handRotationCheck = new GameObject("HandMenuRotationCheck").transform;
        ComponentsToToggle.transform.localScale = Vector3.zero;
        ComponentsToToggle.SetActive(false);
    }

    protected virtual void Update()
    {
        if (menuCurrentlyConnectedToAHand)
        {
            UpdateMenuVariables();
            MoveMenu();
            RotateMenu();

            if (ComponentsToToggle.transform.localScale.x < 1f)
            {
                if (!ComponentsToToggle.activeSelf)
                {
                    ComponentsToToggle.SetActive(true);
                }

                ComponentsToToggle.transform.localScale = Vector3.Lerp(ComponentsToToggle.transform.localScale, Vector3.one, 0.3f);
                if (ComponentsToToggle.transform.localScale.x >= 0.99f)
                {
                    ComponentsToToggle.transform.localScale = Vector3.one;
                }
            }
        }
        else if (ComponentsToToggle.transform.localScale.x > 0.01f)
        {
            ComponentsToToggle.transform.localScale = Vector3.Lerp(ComponentsToToggle.transform.localScale, Vector3.zero, 0.3f);
            if (ComponentsToToggle.transform.localScale.x < 0.01f)
            {
                ComponentsToToggle.transform.localScale = Vector3.zero;
                ComponentsToToggle.SetActive(false);
            }
        }
    }

    private void RotateMenu()
    {
        if ((attachedToRightHand && faceHololens)
                || (!attachedToRightHand && faceHololens))
        {
            menuAnchor.LookAt(Hololens);
        }
        else
        {
            menuAnchor.rotation = Quaternion.Slerp(menuAnchor.rotation, handRotationCheck.rotation * Quaternion.Euler(90f, 0, 0), 0.3f);
        }
    }

    private void UpdateMenuVariables()
    {
        if (attachedToRightHand)
        {
            inputManager.TryGetHandJointTransform(InputSourceKind.HandRight, JointName.Palm, out palmPosition, out palmRotation, true);
        }
        else
        {
            inputManager.TryGetHandJointTransform(InputSourceKind.HandLeft, JointName.Palm, out palmPosition, out palmRotation, true);
        }


        handRotationCheck.position = palmPosition;
        handRotationCheck.rotation = palmRotation;
    }

    protected virtual void MoveMenu()
    {
        menuAnchor.position = Vector3.Lerp(menuAnchor.position, palmPosition + OffsetManager(handRotationCheck, offset), Mathf.Clamp(10f * (Vector3.Distance(menuAnchor.position, palmPosition + OffsetManager(handRotationCheck, offset)) - slack), 0f, 1f));
    }

    private Vector3 OffsetManager(Transform offsetFrom, Vector3 offsetTo)
    {
        if (attachedToRightHand)
        {
            return offsetFrom.right * offsetTo.x + offsetFrom.up * offsetTo.z + offsetFrom.forward * offsetTo.y;
        }
        else
        {
            return -offsetFrom.right * offsetTo.x + offsetFrom.up * offsetTo.z + offsetFrom.forward * offsetTo.y;
        }
    }
#pragma warning restore CS0649
}