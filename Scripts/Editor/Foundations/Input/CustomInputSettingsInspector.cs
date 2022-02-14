using Bouvet.DevelopmentKit.Input;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputSettings))]
public class CustomInputSettingsInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw input priority
        base.OnInspectorGUI();

        // Gets InputSettings 
        var inputSettings = target as InputSettings;

        // Setup of style
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleLeft;
        style.fontStyle = FontStyle.Bold;
        EditorGUILayout.Space();
        
        // Spatial map and hologram alignment
        EditorGUILayout.LabelField("Spatial maps and hologram alignment", style, GUILayout.ExpandWidth(true));
        inputSettings.UseSpatialMap = GUILayout.Toggle(inputSettings.UseSpatialMap, new GUIContent("Use Spatial Map", "Toggling this will enable the Spatial Mesh for your application")); 
        inputSettings.AlignWithPVCamera = GUILayout.Toggle(inputSettings.AlignWithPVCamera, new GUIContent("Align Holograms With Camera", "Toggling this will make Holograms align properly in screenshots and for screen sharing."));
        EditorGUILayout.Space();

        // Hand tracking
        EditorGUILayout.LabelField("Hand tracking", style, GUILayout.ExpandWidth(true));
        inputSettings.UseHands = GUILayout.Toggle(inputSettings.UseHands, new GUIContent("Use Hand Tracking", "Enabling this allows hands to be used for interaction in the program. Many other interaction methods are dependent on this."));
        if (inputSettings.UseHands)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.ShowIndexIndicator = GUILayout.Toggle(inputSettings.ShowIndexIndicator, new GUIContent("Show Index Finger Indicator", "Show a cursor on each index finger. A visual representation of your interaction point."));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.ShowHandJoints = GUILayout.Toggle(inputSettings.ShowHandJoints, new GUIContent("Show Hand Joints", "Show a representation of each hand joint on the Hololens. Useful for debugging."));
            GUILayout.EndHorizontal();

            inputSettings.UseManipulation = GUILayout.Toggle(inputSettings.UseManipulation, new GUIContent("Allow Manipulation", "Manipulation is the abiliy to grab, move, scale and rotate objects."));
        }
        else
        {
            inputSettings.ShowHandJoints = false;
            inputSettings.UseInteractionBeams = false;
            inputSettings.UseManipulation = false;
            inputSettings.InteractionBeamsDistance = 10f;
            inputSettings.InteractionBeamDepthMultiplier = 5f;
        }

        EditorGUILayout.Space();

        // Cursors and eye tracking
        EditorGUILayout.LabelField("Cursors and Eye tracking", style, GUILayout.ExpandWidth(true));
        if (inputSettings.UseHands)
            inputSettings.UseInteractionBeams = GUILayout.Toggle(inputSettings.UseInteractionBeams, new GUIContent("Use Interaction Beams", "Interaction beams are rays coming out of the palm of the user. Allows objects to be grabbed and clicked from afar."));
        if (inputSettings.UseInteractionBeams)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.InteractionBeamsDistance = EditorGUILayout.Slider("Interaction Beam Max Length", inputSettings.InteractionBeamsDistance, 10f, 1000f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.InteractionBeamDepthMultiplier = EditorGUILayout.Slider("Interaction Beam Depth Multiplier", inputSettings.InteractionBeamDepthMultiplier, 0f, 100f);
            GUILayout.EndHorizontal();
        }
        inputSettings.UseHeadGaze = GUILayout.Toggle(inputSettings.UseHeadGaze, new GUIContent("Use Head Gaze", "Head gaze is the raycast going straight out in front of the Hololens. Was the standard for interaction on Hololens 1."));
        if (inputSettings.UseHeadGaze)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.ShowHeadGazeCursor = GUILayout.Toggle(inputSettings.ShowHeadGazeCursor, new GUIContent("Create visible Hololens Cursor", "A Hololens Cursor is a cursor that appears in the position hit by a raycast straight in front of the user."));
            GUILayout.EndHorizontal();
            if (inputSettings.ShowHeadGazeCursor)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                inputSettings.AlwaysShowHeadGazeCursor = GUILayout.Toggle(inputSettings.AlwaysShowHeadGazeCursor, new GUIContent("Always show Hololens Cursor", "Always displays the visible cursor from the Hololens."));
                GUILayout.EndHorizontal();
            }
            else
            {
                inputSettings.AlwaysShowHeadGazeCursor = false;
            }
        }
        else
        {
            inputSettings.ShowHeadGazeCursor = false;
        }
        inputSettings.UseEyeGaze = GUILayout.Toggle(inputSettings.UseEyeGaze, new GUIContent("Use Eye Gaze", "Eye gaze is the raycast from the eyes of the user. Must be toggled on for eye tracking functionality."));
        if (inputSettings.UseEyeGaze)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            inputSettings.ShowEyeGazeCursor = GUILayout.Toggle(inputSettings.ShowEyeGazeCursor, new GUIContent("Create visible Eye Gaze Cursor", "An Eye Gaze Cursor is a cursor that appears in the position hit by a raycast from your eyes."));
            GUILayout.EndHorizontal();
            if (inputSettings.ShowEyeGazeCursor)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                inputSettings.AlwaysShowEyeGazeCursor = GUILayout.Toggle(inputSettings.AlwaysShowEyeGazeCursor, new GUIContent("Always show Eye Gaze Cursor", "Always displays the visible cursor from the eye gaze."));
                GUILayout.EndHorizontal();
            }
            else
            {
                inputSettings.AlwaysShowEyeGazeCursor = false;
            }
        }
        else
        {
            inputSettings.ShowEyeGazeCursor = false;
        }

        EditorGUILayout.Space();
        // Voice recognizion
        EditorGUILayout.LabelField("Voice Recognizion", style, GUILayout.ExpandWidth(true));
        inputSettings.UseVoice = GUILayout.Toggle(inputSettings.UseVoice, new GUIContent("Use Voice Recognizion", "Voice recognizion allows custom commands to be handled by BouvetDevelopmentKit."));

        // Generate prefabs based on input
        if (!Application.isPlaying)
        {
            if (!inputSettings.inputManager)
                inputSettings.inputManager = inputSettings.GetComponent<InputManager>();
            inputSettings.inputManager.GeneratePrefabs();
        }
    }
}