using Bouvet.DevelopmentKit.Input;
using UnityEngine;

public class VoiceClickOnInteractables : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField]
    protected string voiceCommand;

    protected InputManager inputManager;
    protected int target;

    protected void Start()
    {
        inputManager = InputManager.Instance;
        inputManager.AddPhraseForVoiceRecognizion(voiceCommand);
        inputManager.OnPhraseRecognized += InputManager_OnPhraseRecognized;
    }

    protected void InputManager_OnPhraseRecognized(InputSource obj)
    {
        target = inputManager.GetFocusedButtonID();
        if (obj.message.Equals(voiceCommand) && target != -1)
        {
            
        }
    }
#pragma warning restore CS0649
}