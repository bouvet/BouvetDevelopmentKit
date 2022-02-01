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

    [TextArea(4,5)]
    public string controls = ("Right click+hold to look around. \nWASD to WASD-move, Q/E to down/up. \nShift to boost. Speed is meters/sec.");

    public float speed = 0.5f;
    public float shiftSpeedMultiply = 5.0f;
    private float actualSpeed;
    private float speedMulti = 1.0f;
    
#if UNITY_EDITOR
    private void Update()
    {
    #if ENABLE_INPUT_SYSTEM

        speedMulti = 1.0f;
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed) { speedMulti = shiftSpeedMultiply; }
        actualSpeed = (speed * speedMulti) * Time.deltaTime;

        if      (Keyboard.current.wKey.isPressed)   { transform.Translate(0, 0, actualSpeed, Space.Self); }
        else if (Keyboard.current.sKey.isPressed)   { transform.Translate(0, 0, -actualSpeed, Space.Self); }
        if      (Keyboard.current.aKey.isPressed)   { transform.Translate(-actualSpeed, 0, 0, Space.Self); }
        else if (Keyboard.current.dKey.isPressed)   { transform.Translate(actualSpeed, 0, 0, Space.Self); }

        if (Keyboard.current.eKey.isPressed)        { transform.Translate(0, actualSpeed, 0, Space.Self); }
        if (Keyboard.current.qKey.isPressed)        { transform.Translate(0, -actualSpeed, 0, Space.Self); }

    #else

        speedMulti = 1.0f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) { speedMulti = shiftSpeedMultiply; }
        actualSpeed = (speed * speedMulti) * Time.deltaTime;

        if      (Input.GetKey(KeyCode.W))   { transform.Translate(0, 0, actualSpeed, Space.Self); }
        else if (Input.GetKey(KeyCode.S))   { transform.Translate(0, 0, -actualSpeed, Space.Self); }
        if      (Input.GetKey(KeyCode.A))   { transform.Translate(-actualSpeed, 0, 0, Space.Self); }
        else if (Input.GetKey(KeyCode.D))   { transform.Translate(actualSpeed, 0, 0, Space.Self); }

        if      (Input.GetKey(KeyCode.E))   { transform.Translate(0, actualSpeed, 0, Space.Self); }
        else if (Input.GetKey(KeyCode.Q))   { transform.Translate(0, -actualSpeed, 0, Space.Self); }

    #endif
    }
#endif
#pragma warning restore CS0414
}