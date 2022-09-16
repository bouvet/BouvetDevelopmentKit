using Bouvet.DevelopmentKit.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bouvet.DevelopmentKit.Input
{
    /// <summary>
    /// This class is set up by InputManager to toggle what functionality is enabled on the app. 
    /// </summary>
    [RequireComponent(typeof(InputManager))]
    public class InputSettings : MonoBehaviour
    {
        [HideInInspector] public InputManager inputManager;
        [Header("Input priority")]
        public List<InputSourceKind> InputPriority;

        [HideInInspector] public bool UseSpatialMap;
        [HideInInspector] public bool AlignWithPVCamera;

        [HideInInspector] public bool UseHands;
        [HideInInspector] public bool ShowIndexIndicator = true;
        [HideInInspector] public bool ShowHandJoints;
        [HideInInspector] public bool UseManipulation;

        [HideInInspector] public bool UseInteractionBeams;
        [HideInInspector] public float InteractionBeamsDistance = 10f;
        [HideInInspector] public float InteractionBeamDepthMultiplier = 5f;
        [HideInInspector] public bool UseHeadGaze;
        [HideInInspector] public bool ShowHeadGazeCursor;
        [HideInInspector] public bool AlwaysShowHeadGazeCursor;
        [HideInInspector] public bool UseEyeGaze;
        [HideInInspector] public bool ShowEyeGazeCursor;
        [HideInInspector] public bool AlwaysShowEyeGazeCursor;

        [HideInInspector] public bool UseVoice;

        [HideInInspector] public bool UseCustomTween;
        [HideInInspector] public float autoTweenTime = 0.25f;
        [HideInInspector] public TweenCurve currentTweenCurve;

        [HideInInspector] public CursorManager CursorManager;
    }
}
