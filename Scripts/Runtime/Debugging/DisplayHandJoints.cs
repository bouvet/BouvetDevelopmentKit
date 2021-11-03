using Bouvet.DevelopmentKit;
using Bouvet.DevelopmentKit.Input;
using UnityEngine;

public class DisplayHandJoints : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField]
    private GameObject jointPrefab;

    [SerializeField]
    private Transform visualComponents;

    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.Instance;
        Invoke(nameof(ShowHands), 1f);
    }

    private void ShowHands()
    {
        //Left hand
        for (int i = 0; i < 26; i++)
        {
            AttachedToJoint joint = Instantiate(jointPrefab, visualComponents).GetComponent<AttachedToJoint>();
            joint.inputManager = inputManager;
            joint.inputSourceKind = InputSourceKind.HandLeft;
            joint.joint = (JointName) i;
        }

        //Right hand
        for (int i = 0; i < 26; i++)
        {
            AttachedToJoint joint = Instantiate(jointPrefab, visualComponents).GetComponent<AttachedToJoint>();
            joint.inputManager = inputManager;
            joint.inputSourceKind = InputSourceKind.HandRight;
            joint.joint = (JointName) i;
        }
    }
#pragma warning restore CS0649
}