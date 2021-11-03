using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

#endif

/// <summary>
/// Editor script for moving the Hololens/Camera in the Unity Editor play mode for debugging purposes.
/// </summary>
public class WASDMovement : MonoBehaviour
{
#pragma warning disable CS0414
    private float speedFactor = 0.01f;
    private bool isRunning = false;

#if UNITY_EDITOR
    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
#else
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
#endif
        {
            speedFactor = 0.03f;
        }
        else
        {
            speedFactor = 0.01f;
        }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.wKey.isPressed)
#else
        if (Input.GetKey(KeyCode.W))
#endif
        {
            transform.Translate(0, 0, speedFactor, Space.Self);
        }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.sKey.isPressed)
#else
        if (Input.GetKey(KeyCode.S))
#endif
        {
            transform.Translate(0, 0, -speedFactor, Space.Self);
        }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.aKey.isPressed)
#else
        if (Input.GetKey(KeyCode.A))
#endif
        {
            transform.Translate(-speedFactor, 0, 0, Space.Self);
        }
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.dKey.isPressed)
#else
        if (Input.GetKey(KeyCode.D))
#endif
        {
            transform.Translate(speedFactor, 0, 0, Space.Self);
        }
    }
#endif
#pragma warning restore CS0414
}