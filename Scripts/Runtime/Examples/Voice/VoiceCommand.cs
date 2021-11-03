using Bouvet.DevelopmentKit.Input;
using System;
using UnityEngine;
using UnityEngine.Events;

public class VoiceCommand : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField]
    protected string command;

    [SerializeField]
    protected UnityEvent unityEvent;

    protected Action action;

    protected void Start()
    {
        action = ExecuteVoiceCommand;
        InputManager.Instance.AddPhraseForVoiceRecognizion(command, action);
    }

    protected void ExecuteVoiceCommand()
    {
        unityEvent?.Invoke();
    }
#pragma warning restore CS0649
}