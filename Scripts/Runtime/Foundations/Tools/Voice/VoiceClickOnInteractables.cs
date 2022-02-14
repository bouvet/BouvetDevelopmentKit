using Bouvet.DevelopmentKit.Input;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Tools.Voice
{
    public class VoiceClickOnInteractables : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        protected string voiceCommand;

        protected InputManager inputManager;
        protected GameObject target;

        protected void Start()
        {
            inputManager = InputManager.Instance;
            inputManager.AddPhraseForVoiceRecognizion(voiceCommand);
            inputManager.OnPhraseRecognized += InputManager_OnPhraseRecognized;
        }

        protected void InputManager_OnPhraseRecognized(InputSource obj)
        {        
            if (obj.message.Equals(voiceCommand) && inputManager.TryGetFocusedButton(out target) && target != null)
            {
                
            }
        }
#pragma warning restore CS0649
    }
}